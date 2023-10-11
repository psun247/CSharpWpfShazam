using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CSharpWpfShazam.Models
{
    public class LyricsSearchResponse
    {
        [JsonPropertyName("meta")]
        public Meta? Meta { get; set; }

        [JsonPropertyName("response")]
        public SearchData? Response { get; set; }
    }

    public class Meta
    {
        [JsonPropertyName("status")]
        public ulong Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class SearchData
    {
        // Hits[0].Result.Url:"https://genius.com/Shayne-ward-breathless-lyrics"
        [JsonPropertyName("hits")]
        public List<SearchHit> Hits { get; set; } = new List<SearchHit>();
    }

    // Only a subset of SearchHit in Genius.NET\ExampleApplication
    public class SearchHit
    {
        [JsonPropertyName("result")]
        public Song? Result { get; set; }
    }

    public class Song
    {
        [JsonPropertyName("full_title")]
        public string FullTitle { get; set; } = string.Empty;
        
        [JsonPropertyName("lyrics_state")]
        public string LyricsState { get; set; } = string.Empty;

        [JsonPropertyName("song_art_image_thumbnail_url")]
        public string SongArtImageThumbnailUrl { get; set; } = string.Empty;

        [JsonPropertyName("song_art_image_url")]
        public string SongArtImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
