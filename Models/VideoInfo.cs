using System.Net;
using CSharpWpfShazam.Helpers;

namespace CSharpWpfShazam.Models
{    
    public class VideoInfo
    {                        
        // Shazam listen result, so it's a song (when used), not a video
        public string Song { get; set; } = string.Empty;

        public string Artist { get; set; } = string.Empty;

        // Served as a key for VideoMatch
        // https://www.shazam.com/track/229189268/the-way-you-were
        // https://www.youtube.com/watch?v=d_l-st8Q1S0
        public string? Link { get; set; }

        // Leave it as null (not string.Empty) as default because string.Empty causes an exception as invalid Image's Source in XAML
        // https://is3-ssl.mzstatic.com/image/thumb/Music5/v4/d5/88/85/d5888577-c197-e689-23ac-fc34c3fed0d3/634158401541.jpg/400x400cc.jpg
        public string? CoverUrl { get; set; }        
        public string? Released { get; set; }        
        
        public string YouTubeWebSiteSearch => $"https://youtube.com/results?search_query={WebUtility.UrlEncode(Artist + " - " + Song)}";
        
        public override string ToString() => Artist.IsNotBlank() ? $"{Artist} - {Song}" : Song;
    }
}
