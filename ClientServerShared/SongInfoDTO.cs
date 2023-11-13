﻿namespace ClientServerShared
{
    // Similar to SongInfo class
    public class SongInfoDTO
    {
        public string Artist { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public string? Lyrics { get; set; }
        public string SongUrl { get; set; } = string.Empty;
        // This is a flag for use in the future between client and server
        public bool IsDeleted { get; set; }
    }
}