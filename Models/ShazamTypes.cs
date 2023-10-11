using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CSharpWpfShazam.Models
{
    #region Shazam Request

    internal class ShazamRequest
    {
        [JsonPropertyName("signature")]
        public ShazamSignature? Signature { get; set; }

    }

    internal class ShazamSignature
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;

        [JsonPropertyName("samplems")]
        public int SampleMs { get; set; }

    }

    #endregion

    #region Shazam Response

    internal class ShazamResponse
    {
        [JsonPropertyName("track")]
        public ShazamTrack? Track { get; set; }

        [JsonPropertyName("retryms")]
        public int? RetryMs { get; set; }
    }

    internal class ShazamTrack
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public ShazamImages? Images { get; set; }

        [JsonPropertyName("share")]
        public ShazamShare? Share { get; set; }
        
        // Instead, use LyricsService class because this only find some English song lyrics
        [JsonPropertyName("sections")]
        public List<Section> SectionList { get; set; } = new List<Section>();
    }
  
    internal class Section
    {
        [JsonPropertyName("type")]
        public string SectionType { get; set; } = string.Empty;

        // When SectionType is "SONG"
        [JsonPropertyName("metadata")]
        public List<Metadata> MetadataList { get; set; } = new List<Metadata>();
    }

    internal class Metadata
    {
        // Pair of "Released" and "2007", etc.
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    internal class ShazamImages
    {
        [JsonPropertyName("coverart")]
        public string Cover { get; set; } = string.Empty;

        [JsonPropertyName("coverarthq")]
        public string CoverHQ { get; set; } = string.Empty;

        [JsonPropertyName("background")]
        public string Background { get; set; } = string.Empty;
    }

    internal class ShazamShare
    {
        [JsonPropertyName("href")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

    }

    #endregion

}
