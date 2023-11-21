using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Microsoft.Web.WebView2.Wpf;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpWpfShazam.Helpers;
using CSharpWpfShazam.Models;

namespace CSharpWpfShazam.ViewModelsViews
{
    // MainViewModel.Shazam.cs
    public partial class MainViewModel
    {
        public WebView2 YouTubeWebView2Control { get; private set; } = new WebView2();
        // Video address in the textbox (whenever navigated to)
        [ObservableProperty]
        string _currentVideoUri = string.Empty;
        [ObservableProperty]
        List<DeviceSetting> _deviceSettingList = new List<DeviceSetting>();
        // "Listen To" or "Cancel"
        [ObservableProperty]
        string _listenButtonText = string.Empty;
        [ObservableProperty]
        bool _isProgressOn;
        [ObservableProperty]
        DeviceSetting? _selectedDeviceSetting;
        [ObservableProperty]
        bool _isAddAzureEnabled;
        [ObservableProperty]
        bool _isAddMySQLEnabled;

        public void OnShazamTabActivated(bool isActivated)
        {
            _isShazamTabActive = isActivated;
            if (_isShazamTabActive)
            {
                _appService.AppSettings.SelectedTabName = AppSettings.ShazamTabName;
                StatusMessage = _DefaultListenToMessage;
            }            
            UpdateShazamTabButtons();
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
                        
                        UpdateShazamTabButtons();
                        StatusMessage = $"Identified as '{videoInfo}'";
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
        private void ReloadDeviceList()
        {
            ReloadDeviceList(isAppStartup: false);
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
                StatusMessage = $"Adding song info to Azure SQL DB ({RestApiAuthInfo})...please wait";

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

        private void UpdateShazamTabButtons()
        {
            IsAddAzureEnabled = _isShazamTabActive && _lastVideoInfo != null;
            IsAddMySQLEnabled = _appService.AppSettings.IsMySQLEnabled && _isShazamTabActive && _lastVideoInfo != null;
        }

        private void ShowProgress(bool isProgressOn)
        {
            if (SelectedDeviceSetting != null)
            {
                IsProgressOn = isProgressOn;
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
