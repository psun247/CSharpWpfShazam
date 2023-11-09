using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpWpfShazam.Helpers;
using CSharpWpfShazam.Models;
using CSharpWpfShazam.Services;

namespace CSharpWpfShazam.ViewModelsViews
{
    public partial class MainViewModel
    {
        private const string _ReadyToListen = "Ready to 'Listen to'. When a song is identified, its lyrics is queried and displayed (if found) below.";
        private const string _SongLyricsNotFound = "Not found";

        // If lyricsApiKey not working, get a new one at: https://genius.com/developers
        private LyricsService _lyricsService = new LyricsService("0LoCEwmokQ1E865SpcLGyVUJvcVYtWfAAsTndJPcz7vWWFxTbqflNXNvRyt5vJzZ");

        [ObservableProperty]
        string? _songCoverUrl;
        [ObservableProperty]
        Visibility _songCoverVisibility = Visibility.Collapsed;
        [ObservableProperty]
        string _songInfoText = _ReadyToListen;
        [ObservableProperty]
        string? _songLyrics;

        [RelayCommand]
        private void CopySongInfo()
        {
            var sb = new StringBuilder();
            if (SongInfoText != _ReadyToListen)
            {
                sb.AppendLine(SongInfoText);
                sb.AppendLine(string.Empty);

                if (!string.IsNullOrWhiteSpace(SongCoverUrl))
                {
                    sb.AppendLine(SongCoverUrl);
                    sb.AppendLine(string.Empty);
                }
            }
            if (!string.IsNullOrWhiteSpace(SongLyrics) && SongLyrics != _SongLyricsNotFound)
            {
                sb.Append(SongLyrics);
            }

            string result = sb.ToString();
            if (result.IsNotBlank())
            {
                Clipboard.SetText(result);
                StatusMessage = "Song info copied to clipboard";
            }
        }

        private async Task<bool> UpdateSongInfoSectionAsync(VideoInfo videoInfo)
        {
            bool updated = true;
            try
            {
                SongCoverUrl = videoInfo.CoverUrl;
                SongInfoText = videoInfo.ToString();

                string lyrics = await _lyricsService.GetLyricsAsync(videoInfo.Song, videoInfo.Artist);
                if (lyrics.IsNotBlank())
                {
                    SongLyrics = lyrics;
                }
                else
                {
                    SongLyrics = _SongLyricsNotFound;
                }
            }
            catch (Exception ex)
            {
                ErrorStatusMessage = ex.Message;
                updated = false;
            }
            return updated;
        }

        partial void OnSongCoverUrlChanged(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                SongInfoText = _ReadyToListen;
                SongCoverVisibility = Visibility.Collapsed;
            }
            else
            {
                SongCoverVisibility = Visibility.Visible;
            }
        }
    }
}