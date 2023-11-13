﻿// Copyright(c) 2023-2024 Peter Sun
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Net.Http;
using Microsoft.Web.WebView2.Wpf;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CSharpWpfShazam.Models;
using CSharpWpfShazam.Services;
using CSharpWpfShazam.Helpers;

namespace CSharpWpfShazam.ViewModelsViews
{
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
        private string RestApiAuthInfo => IsRestApiViaAuth ? "via authorized REST API" : "via not authorized REST API";

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
        }

        public string AppTitle { get; private set; }
        public AppSettings AppSettings => _appService.AppSettings;
        public WebView2 YouTubeWebView2Control { get; private set; } = new WebView2();
        // Video address in the textbox (whenever navigated to)
        [ObservableProperty]
        string _currentVideoUri = string.Empty;
        [ObservableProperty]
        List<DeviceSetting> _deviceSettingList = new List<DeviceSetting>();
        public bool IsCommandNotBusy => !_isCommandBusy;
        // "Listen To" or "Cancel"
        [ObservableProperty]
        string _listenButtonText = string.Empty;
        [ObservableProperty]
        bool _isListenButonEnabled;
        [ObservableProperty]
        bool _isProgressOn;
        [ObservableProperty]
        DeviceSetting? _selectedDeviceSetting;
        [ObservableProperty]
        private Visibility _songInfoSectionVisibility = Visibility.Visible;
        [ObservableProperty]
        bool _isAddAzureEnabled;
        [ObservableProperty]
        bool _isDeleteAzureEnabled;
        [ObservableProperty]
        bool _isAddMySQLEnabled;
        [ObservableProperty]
        bool _isDeleteMySQLEnabled;
        [ObservableProperty]
        string _statusMessage = string.Empty;
        [ObservableProperty]
        bool _isErrorStatusMessage;        

        public async Task InitializeAsync()
        {
            ReloadDeviceList(isAppStartup: true);
            _azureService = await AzureService.CreateAsync();
        }

        public void OnShazamTabActivated(bool isActivated)
        {
            _isShazamTabActive = isActivated;
            if (_isShazamTabActive)
            {
                _appService.AppSettings.SelectedTabName = AppSettings.ShazamTabName;
                IsListenButonEnabled = true;
                StatusMessage = _DefaultListenToMessage;
            }
            UpdateAzureAddDeleteButtonStates();
            UpdateMySQLAddDeleteButtonStates();
        }

        public void OnMySQLTabActivated(bool isActivated)
        {
            _isMySQLTabActive = isActivated;
            if (_isMySQLTabActive)
            {
                _appService.AppSettings.SelectedTabName = AppSettings.MySQLTabName;
                IsListenButonEnabled = false;
                StatusMessage = "To listen to a song to identify, go back to Shazam tab";

                if (!_isMySQLTabInSync)
                {
                    // Add happened, hence requiring sync

                    if (!LoadSongInfoListOnMySQLTab())
                    {
                        // Ensure demo mode
                        DemoModeBindSongInfoListFromMySQL();
                        _appService.UpdateMySQLEnabled(false);
                        OnPropertyChanged(nameof(SwitchModeButtonText));
                        OnPropertyChanged(nameof(SwitchModeDescriptionText));
                    }

                    // Auto-select SelectedSongInfoFromMySQL
                    var songInfo = SongInfoListFromMySQL.FirstOrDefault(x => x.SongUrl == _appService.AppSettings.SelectedSongUrl);
                    if (songInfo != null && songInfo != SelectedSongInfoFromMySQL)
                    {
                        SelectedSongInfoFromMySQL = songInfo;
                    }

                    _isMySQLTabInSync = true;
                }
            }
            UpdateMySQLAddDeleteButtonStates();
        }

        public async void OnAzureTabActivated(bool isActivated)
        {
            _isAzureTabActive = isActivated;
            if (_isAzureTabActive)
            {
                _appService.AppSettings.SelectedTabName = AppSettings.AzureTabName;
                IsListenButonEnabled = false;
                IsRestApiViaAuth = _appService.AppSettings.IsRestApiViaAuth;
                OnIsRestApiViaAuthChanged(IsRestApiViaAuth);
                StatusMessage = "To listen to a song to identify, go back to Shazam tab";

                if (!_isAzureTabInSync)
                {
                    await LoadSongInfoListOnAzureTabAsync();

                    // Auto-select SongInfoListFromAzure
                    var songInfo = SongInfoListFromAzure.FirstOrDefault(x => x.SongUrl == _appService.AppSettings.SelectedSongUrl);
                    if (songInfo != null && songInfo != SelectedSongInfoFromAzure)
                    {
                        SelectedSongInfoFromAzure = songInfo;
                    }

                    _isAzureTabInSync = true;
                }
            }
            UpdateAzureAddDeleteButtonStates();
        }

        private void UpdateAzureAddDeleteButtonStates()
        {
            IsAddAzureEnabled = _isShazamTabActive && _lastVideoInfo != null;
            IsDeleteAzureEnabled = _isAzureTabActive &&
                                    SongInfoListFromAzure.Count > 0 && SelectedSongInfoFromAzure != null;
        }

        private void UpdateMySQLAddDeleteButtonStates()
        {
            IsAddMySQLEnabled = _appService.AppSettings.IsMySQLEnabled && _isShazamTabActive && _lastVideoInfo != null;
            IsDeleteMySQLEnabled = _appService.AppSettings.IsMySQLEnabled && _isMySQLTabActive &&
                                    SongInfoListFromMySQL.Count > 0 && SelectedSongInfoFromMySQL != null;
        }

        [RelayCommand]
        private void ReloadDeviceList()
        {
            ReloadDeviceList(isAppStartup: false);
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

            // As of 2023-10-20+, only SelectedTabName, SelectedSongUrl, and IsRestApiViaAuth need to be saved on app shutdown
            _appService.SaveAppSettings();

            return true;
        }

        // Note: with 'async Task', Listen button will be automatically disabled when the command is being executed,
        //          hence leaving it as 'async void'        
        [RelayCommand]
        private async void ListenOrCancel()
        {
            if (SelectedDeviceSetting == null || SelectedDeviceSetting.DeviceID.IsBlank())
            {
                ErrorStatusMessage = "Please select a device";
                return;
            }

            if (_isCommandBusy)
            {
                // Cause to throw OperationCanceledException in Listen() on a previously created
                // _cancelTokenSource in this method                
                _userCanceledListen = true;
                _cancelTokenSource?.Cancel();
                return;
            }

            try
            {
                StatusMessage = $"Listening to '{SelectedDeviceSetting.DeviceName}'...please wait";
                ListenButtonText = "Cancel";
                _userCanceledListen = false;

                _cancelTokenSource = new CancellationTokenSource();
#pragma warning disable 4014
                // disable without await, and it's OK because we want to _cancelTokenSource.Cancel() on timeout
                Task.Delay(IDENTIFY_TIMEOUT).ContinueWith((_) =>
                {
                    // Cause to throw OperationCanceledException in Listen()
                    _cancelTokenSource?.Cancel();
                });

                SetCommandBusy(true);
                ShowProgress(true);

                Tuple<VideoInfo?, string> result = await _deviceService.Listen(SelectedDeviceSetting, _cancelTokenSource);
                _cancelTokenSource = null;
                VideoInfo? videoInfo = result.Item1;
                if (videoInfo != null)
                {
                    DebugDumpVideoInfo(videoInfo);

                    // Note: Stopping video here (instead of before Listen() call) means
                    // I can listen to and identify my own video in the embedded video player!
                    // But it would stop the current and show a list of similar videos
                    StopCurrentVideo();

                    BindWebView2Control(videoInfo.YouTubeWebSiteSearch);

                    if (await UpdateSongInfoSectionAsync(videoInfo))
                    {
                        // Hang on this for MySQL
                        _lastVideoInfo = videoInfo;

                        UpdateAzureAddDeleteButtonStates();
                        UpdateMySQLAddDeleteButtonStates();

                        if (_appService.AppSettings.IsMySQLEnabled)
                        {
                            // Kind of short description for status bar
                            StatusMessage = $"Identified as '{videoInfo}'";
                        }
                        else
                        {
                            StatusMessage = $"Identified as '{videoInfo}' (to add to DB, switch to MySQL mode on MySQL tab)";
                        }
                    }
                }
                else
                {
                    // See OperationCanceledException in Listen() for info
                    ErrorStatusMessage = result.Item2.IsBlank() && _userCanceledListen ? "Canceled" : "Timed out";
                }
            }
            catch (Exception ex)
            {
                ErrorStatusMessage = ex.Message;
            }

            ShowProgress(false);
            SetCommandBusy(false);
            ListenButtonText = _ListenToButtonText;
        }

        [RelayCommand]
        private void ExpandOrCollapseSongInfoSection()
        {
            SongInfoSectionVisibility = (SongInfoSectionVisibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            _appService.UpdateSongInfoSectionVisibility(SongInfoSectionVisibility == Visibility.Visible);
        }

        [RelayCommand]
        private void AddMySQL()
        {
            if (_lastVideoInfo == null)
            {
                return;
            }

            try
            {
                var songInfo = new SongInfo
                {
                    Artist = _lastVideoInfo.Artist,
                    Description = _lastVideoInfo.Song,
                    CoverUrl = _lastVideoInfo.CoverUrl,
                    Lyrics = SongLyrics,
                    SongUrl = CurrentVideoUri // Assume CurrentVideoUri is a matching song or YouTube search
                };
                if (_mysqlService.AddSongInfo(songInfo, out string error))
                {
                    _isMySQLTabInSync = false;
                    StatusMessage = "Song info added to MySQL DB";
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

                string error = await _azureService!.AddSongInfoAsync(songInfo, IsRestApiViaAuth);
                if (error.IsBlank())
                {
                    _isAzureTabInSync = false;
                    StatusMessage = $"Song info added to Azure SQL DB ({RestApiAuthInfo})";
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

        private void ShowProgress(bool isProgressOn)
        {
            if (SelectedDeviceSetting != null)
            {
                IsProgressOn = isProgressOn;
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

        private void DebugDumpVideoInfo(VideoInfo videoInfo)
        {
            Debug.WriteLine("****VideoInfo");
            Debug.WriteLine($"Artist: {videoInfo.Artist}");
            Debug.WriteLine($"Song: {videoInfo.Song}");            
            Debug.WriteLine($"CoverUrl: {videoInfo.CoverUrl}");
        }
    }
}