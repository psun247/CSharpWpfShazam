using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Web.WebView2.Wpf;
using CSharpWpfShazam.Helpers;
using CSharpWpfShazam.Models;

namespace CSharpWpfShazam.ViewModelsViews
{
    // MainViewModel.Azure.cs
    public partial class MainViewModel
    {
        [ObservableProperty]
        ObservableCollection<SongInfo> _songInfoListFromAzure = new ObservableCollection<SongInfo>();
        [ObservableProperty]
        SongInfo? _selectedSongInfoFromAzure;
        public WebView2 AzureWebView2Control { get; private set; } = new();
        [ObservableProperty]
        bool _isDeleteAzureEnabled;
        [ObservableProperty]
        bool _isWebApiViaAuth;
        [ObservableProperty]
        string? _webApiAuthOptionDescription;

        private void InitializeAzureLTab()
        {
            AzureWebView2Control = new WebView2
            {
                Name = "AzureWebView2",
                Source = _YouTubeHomeUri,
            };
            OnPropertyChanged(nameof(AzureWebView2Control));
        }

        public async void OnAzureTabActivated(bool isActivated)
        {
            _isAzureTabActive = isActivated;
            if (_isAzureTabActive)
            {
                _appService.AppSettings.SelectedTabName = AppSettings.AzureTabName;
                IsWebApiViaAuth = _appService.AppSettings.IsWebApiViaAuth;
                OnIsWebApiViaAuthChanged(IsWebApiViaAuth);
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
            UpdateAzureTabButtons();
        }

        private async Task LoadSongInfoListOnAzureTabAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                StatusMessage = $"Loading song info list from Azure SQL DB ({WebApiAuthInfo})...please wait";

                var list = await _azureService!.GetAllSongInfoListAsync(IsWebApiViaAuth);
                SongInfoListFromAzure = new ObservableCollection<SongInfo>(list);

                StatusMessage = list.Count == 0 ? $"No song info found at Azure SQL DB ({WebApiAuthInfo})" : $"Song info list loaded from Azure SQL DB ({WebApiAuthInfo})";                
            }
            catch (HttpRequestException ex)
            {
                SongInfoListFromAzure = new ObservableCollection<SongInfo>();
                await HandleHttpRequestExceptionAsync(ex);
            }
            catch (Exception ex)
            {
                // e.g. when ex isHttpRequestException, Response status code does not indicate success: 401 (Unauthorized).

                SongInfoListFromAzure = new ObservableCollection<SongInfo>();
                ErrorStatusMessage = ex.Message;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        [RelayCommand]
        private async Task DeleteAzure()
        {
            if (string.IsNullOrWhiteSpace(SelectedSongInfoFromAzure?.CoverUrl))
            {
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete the selected song info from Azure SQL DB via Web API?", "Confirmation",
                           MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                StatusMessage = $"Deleting song info from Azure SQL DB ({WebApiAuthInfo})...please wait";

                string error = await _azureService!.DeleteSongInfoAsync(SelectedSongInfoFromAzure.SongUrl, IsWebApiViaAuth);
                if (error.IsBlank())
                {
                    SongInfoListFromAzure = new ObservableCollection<SongInfo>(
                                                    await _azureService!.GetAllSongInfoListAsync(IsWebApiViaAuth));                    
                    UpdateAzureTabButtons();
                    StatusMessage = $"Song info deleted from Azure SQL DB ({WebApiAuthInfo})";
                }
                else
                {
                    ErrorStatusMessage = error;
                }
            }
            catch (HttpRequestException ex)
            {
                SongInfoListFromAzure = new ObservableCollection<SongInfo>();
                await HandleHttpRequestExceptionAsync(ex);
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
        private async Task RefreshAzure()
        {
            await LoadSongInfoListOnAzureTabAsync();
        }

        partial void OnSelectedSongInfoFromAzureChanged(SongInfo? value)
        {
            if (value == null)
            {
                SongCoverUrl = null;
                SongInfoText = _ReadyToListen;
                SongLyrics = string.Empty;
                MySQLWebView2Control.Source = _YouTubeHomeUri;
                _appService.AppSettings.SelectedSongUrl = string.Empty;
            }
            else
            {
                SongCoverUrl = value.CoverUrl;
                SongInfoText = value.ToString();
                SongLyrics = value.Lyrics;
                AzureWebView2Control.Source = new Uri(value.SongUrl);
                _appService.AppSettings.SelectedSongUrl = value.SongUrl;
            }            
            UpdateAzureTabButtons();
        }      

        partial void OnIsWebApiViaAuthChanged(bool value)
        {
            _appService.AppSettings.IsWebApiViaAuth = value;
            if (value)
            {
                WebApiAuthOptionDescription = $"Use Web API with JWT token ({_azureService?.WebApiUrlAuth})";
            }
            else
            {
                WebApiAuthOptionDescription = $"Use Web API with no-auth ({_azureService?.WebApiUrlNoAuth})";
            }
        }

        private void UpdateAzureTabButtons()
        {
            IsDeleteAzureEnabled = _isAzureTabActive &&
                                    SongInfoListFromAzure.Count > 0 && SelectedSongInfoFromAzure != null;                                    
        }
    }
}
