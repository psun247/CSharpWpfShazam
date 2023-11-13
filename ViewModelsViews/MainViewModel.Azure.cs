using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        bool _isRestApiViaAuth;
        [ObservableProperty]
        string? _restApiAuthOptionDescription;

        private void InitializeAzureLTab()
        {
            AzureWebView2Control = new WebView2
            {
                Name = "AzureWebView2",
                Source = _YouTubeHomeUri,
            };
            OnPropertyChanged(nameof(AzureWebView2Control));
        }

        private async Task LoadSongInfoListOnAzureTabAsync(bool isFromRefresh = false)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var list = await _azureService!.GetAllSongInfoListAsync(IsRestApiViaAuth);
                SongInfoListFromAzure = new ObservableCollection<SongInfo>(list);

                if (isFromRefresh)
                {
                    StatusMessage = list.Count == 0 ? $"No song info found at Azure SQL DB ({RestApiAuthInfo})" : $"Song info list loaded from Azure SQL DB ({RestApiAuthInfo})";
                }
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

            if (MessageBox.Show("Are you sure you want to delete the selected song info from Azure SQL DB via REST API?", "Confirmation",
                           MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                string error = await _azureService!.DeleteSongInfoAsync(SelectedSongInfoFromAzure.SongUrl, IsRestApiViaAuth);
                if (error.IsBlank())
                {
                    SongInfoListFromAzure = new ObservableCollection<SongInfo>(await _azureService!.GetAllSongInfoListAsync(IsRestApiViaAuth));
                    UpdateAzureAddDeleteButtonStates();
                    StatusMessage = $"Song info deleted from Azure SQL DB ({RestApiAuthInfo})";
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
        private async Task RefreshAzure()
        {
            await LoadSongInfoListOnAzureTabAsync(isFromRefresh: true);
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
            UpdateAzureAddDeleteButtonStates();
        }

        partial void OnIsRestApiViaAuthChanged(bool value)
        {
            _appService.AppSettings.IsRestApiViaAuth = value;
            if (value)
            {
                RestApiAuthOptionDescription = $"REST API with auth ({_azureService?.RestApiUrlAuth})";
            }
            else
            {
                RestApiAuthOptionDescription = $"REST API with no-auth ({_azureService?.RestApiUrlNoAuth})";
            }
        }
    }
}
