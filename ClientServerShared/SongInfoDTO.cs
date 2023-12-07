namespace ClientServerShared
{
    // Similar to SongInfo class
    public class SongInfoDTO
    {
        // DB Id to be used for Delete from client
        public int SongInfoId { get; set; }
        public string Artist { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public string? Lyrics { get; set; }
        public string SongUrl { get; set; } = string.Empty;        
    }
}
