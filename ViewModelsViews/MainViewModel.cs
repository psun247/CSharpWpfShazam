using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Reflection;
using Microsoft.Web.WebView2.Wpf;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CSharpWpfShazam.Models;
using CSharpWpfShazam.Services;
using CSharpWpfShazam.Helpers;

namespace CSharpWpfShazam.ViewModelsViews
{
    // MainViewModel.cs
    public partial class MainViewModel : ObservableObject
    {
        private const string _ListenToButtonText = "Listen to";
        private const string _DefaultListenToMessage = "Select a microphone or speaker to 'Listen to' while a song is being played (in this app or another)";
        private const int IDENTIFY_TIMEOUT = 25000;
        private readonly Uri _YouTubeHomeUri = new Uri("https://www.youtube.com");
        private static readonly HttpClient _HttpClient = new() { Timeout = TimeSpan.FromSeconds(6) }; // 3 would be too short for Listen()

        private string _appConfigFilePath;
        private AppService _appService;
        private DeviceService _deviceService;
        private MySQLService _mysqlService;
        private AzureService? _azureService;
        private VideoInfo? _lastVideoInfo;
        private bool _isShazamTabActive;
        private bool _isMySQLTabActive;
        private bool _isMySQLTabInSync;
        private bool _isAzureTabActive;
        private bool _isAzureTabInSync;
        private bool _isCommandBusy;
        private CancellationTokenSource? _cancelTokenSource;
        private bool _userCanceledListen;
        private string WebApiAuthInfo => IsWebApiViaAuth ? "via authorized Web API" : "via no-auth Web API";

        public MainViewModel(string appConfigFilePath)
        {
            _appConfigFilePath = appConfigFilePath;
            _appService = new AppService(appConfigFilePath);
            _deviceService = new DeviceService(_HttpClient);
            _mysqlService = new MySQLService();
            InitializeMain();
            SetCommandBusy(false);
            ListenButtonText = _ListenToButtonText;
            Version ver = Environment.Version;
            AppTitle = $"CSharpWpfShazam (.NET {ver.Major}.{ver.Minor}.{ver.Build} runtime) by Peter Sun";
#if DEBUG
            AppTitle += " - Debug";
#endif
            InitializeMySQLTab();
            InitializeAzureLTab();
            LoadContentForAboutTab();
        }

        public string AppTitle { get; private set; }
        public AppSettings AppSettings => _appService.AppSettings;       
        public bool IsCommandNotBusy => !_isCommandBusy;        
        [ObservableProperty]
        private Visibility _songInfoSectionVisibility = Visibility.Visible;
        [ObservableProperty]
        string _statusMessage = string.Empty;
        [ObservableProperty]
        bool _isErrorStatusMessage;
        public AboutContent AboutTabContent { get; private set; } = new AboutContent();

        public async Task InitializeAsync()
        {
            ReloadDeviceList(isAppStartup: true);
            _azureService = await AzureService.CreateAsync();
        }                      
       
        
        private void ReloadDeviceList(bool isAppStartup)
        {
            try
            {
                List<DeviceInfo> deviceInfoList = _deviceService.GetDeviceList();
                DeviceSettingList = deviceInfoList.Select(x => new DeviceSetting { DeviceName = x.DeviceName, DeviceID = x.DeviceID }).ToList();
                // Note: leave SelectedDeviceSetting as null to force the user to select a right device
                SelectedDeviceSetting = DeviceSettingList.FirstOrDefault(x => x.DeviceID == _appService.AppSettings.SelectedDeviceID);
                if (isAppStartup)
                {
                    StatusMessage = _DefaultListenToMessage;
                }
                else
                {
                    StatusMessage = "Reloaded device list";
                }
            }
            catch (Exception ex)
            {
                ErrorStatusMessage = ex.Message;
            }
        }

        public bool Shutdown()
        {
            if (_isCommandBusy)
            {
                ErrorStatusMessage = "A command is in progress...please wait";

                return false;
            }

            StopCurrentVideo(blankUriOnly: true);

            // As of 2023-10-20+, only SelectedTabName, SelectedSongUrl, and IsWebApiViaAuth need to be saved on app shutdown
            _appService.SaveAppSettings();

            return true;
        }        

        [RelayCommand]
        private void ExpandOrCollapseSongInfoSection()
        {
            SongInfoSectionVisibility = (SongInfoSectionVisibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            _appService.UpdateSongInfoSectionVisibility(SongInfoSectionVisibility == Visibility.Visible);
        }
      
        // Note: using AddAzureAsync() won't bind (maybe a bug with 'Async')
        [RelayCommand]
        private async Task AddAzure()
        {
            if (_lastVideoInfo == null)
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var songInfo = new SongInfo
                {
                    Artist = _lastVideoInfo.Artist,
                    Description = _lastVideoInfo.Song,
                    CoverUrl = _lastVideoInfo.CoverUrl,
                    Lyrics = SongLyrics,
                    SongUrl = CurrentVideoUri
                };

                string error = await _azureService!.AddSongInfoAsync(songInfo, IsWebApiViaAuth);
                if (error.IsBlank())
                {
                    _isAzureTabInSync = false;                    
                    StatusMessage = $"Song info added to Azure SQL DB ({WebApiAuthInfo})";
                }
                else
                {
                    ErrorStatusMessage = error;
                }
            }
            catch (Exception ex)
            {
                ErrorStatusMessage = ex.Message;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }        

        [RelayCommand]
        private async Task OpenInExternalBrowser()
        {
            try
            {
                if (CurrentVideoUri.IsNotBlank())
                {
                    await GeneralHelper.ExecuteOpenUrlCommandAsync(CurrentVideoUri);
                }
                else
                {
                    ErrorStatusMessage = "YouTube video or search query not found";
                }
            }
            catch (Exception ex)
            {
                ErrorStatusMessage = ex.Message;
            }
        }

        // Key="Enter"
        [RelayCommand]
        private void GoVideoUrl()
        {
            try
            {
                BindWebView2Control(CurrentVideoUri);
            }
            catch (Exception ex)
            {
                ErrorStatusMessage = ex.Message;
            }
        }

        // partial method hook (after / device selected)
        partial void OnSelectedDeviceSettingChanged(DeviceSetting? value)
        {
            if (value != null)
            {
                _appService.UpdateDeviceinfo(value.DeviceName, value.DeviceID);
                StatusMessage = $"Selected listening device '{value.DeviceName}'";
            }
        }

        // Handel text red color via DataTrigger with IsErrorStatusMessage (not necessarily an error message)
        private string ErrorStatusMessage
        {
            set
            {
                IsErrorStatusMessage = true;
                StatusMessage = value;

#pragma warning disable MVVMTK0034                
                // Set it back to false (without binding, hence _isErrorStatusMessage) for next StatusMessage
                // (see OnStatusMessageChanging())
                _isErrorStatusMessage = false;
            }
        }

        partial void OnStatusMessageChanging(string value)
        {
            if (!IsErrorStatusMessage)
            {
                // 'IsErrorStatusMessage = false;' doesn't work since IsErrorStatusMessage is already false
                OnPropertyChanged(nameof(IsErrorStatusMessage));
            }
        }
        
        private void SetCommandBusy(bool isCommandBusy)
        {
            _isCommandBusy = isCommandBusy;
            OnPropertyChanged(nameof(IsCommandNotBusy));
        }

        private void InitializeMain()
        {
            YouTubeWebView2Control = new WebView2
            {
                Name = "YouTubeWebView2",
                Source = _YouTubeHomeUri,
            };
            YouTubeWebView2Control.SourceChanged += (s, e) =>
            {
                // Always update the textbox (so can copy to clipboard)
                // not the same as YouTube behavior (only update at the top)                
                CurrentVideoUri = YouTubeWebView2Control.Source.AbsoluteUri;
            };
            OnPropertyChanged(nameof(YouTubeWebView2Control));

            SongInfoSectionVisibility = _appService.AppSettings.IsSongInfoSectionVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BindWebView2Control(string youTubeWebView2Source)
        {
            if (YouTubeWebView2Control.Source == null || YouTubeWebView2Control.Source.AbsoluteUri != youTubeWebView2Source)
            {
                YouTubeWebView2Control.Source = new Uri(youTubeWebView2Source, UriKind.RelativeOrAbsolute);
            }
        }

        // blankUriOnly to avoid UI flickering on app exit
        private void StopCurrentVideo(bool blankUriOnly = false)
        {
            if (YouTubeWebView2Control != null)
            {
                // Only stop the playing (if going on), but can't pause the video though (would be nice) because it's a website uri
                Uri uri = YouTubeWebView2Control.Source;
                YouTubeWebView2Control.Source = _YouTubeHomeUri;
                if (!blankUriOnly)
                {
                    YouTubeWebView2Control.Source = uri;
                }
            }
        }

        // Since this is the only method for 'About' tab, don't create MainViewModel.About.cs
        private void LoadContentForAboutTab()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                string[] names = asm.GetManifestResourceNames();
                string? path = names.FirstOrDefault(x => x == "CSharpWpfShazam.Assets.AboutTabContent.txt");
                if (path != null)
                {
                    using (Stream? stream = asm.GetManifestResourceStream(path))
                    {
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                string[] lines = reader.ReadToEnd().Split("\r\n").Where(x => !x.StartsWith("--")).ToArray();
                                Paragraph? paragraph = null;
                                foreach (string line in lines)
                                {
                                    if (line.StartsWith("=="))
                                    {
                                        paragraph = new Paragraph
                                        {
                                            Header = line.Remove(0, 2)
                                        };
                                        AboutTabContent.ParagraphList.Add(paragraph);
                                    }
                                    else if (paragraph != null)
                                    {
                                        paragraph.Detail = $"{paragraph.Detail}\r\n{line}";
                                    }
                                }
                            }
                        }
                    }
                }

                if (AboutTabContent.ParagraphList.Count == 0)
                {
                    AboutTabContent.Error = "About content not found";
                }
            }
            catch (Exception ex)
            {
                AboutTabContent.Error = $"Error: {ex.Message}";
            }
        }
    }
}