using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using CSharpWpfShazam.Helpers;

namespace CSharpWpfShazam.Models
{
    // SongInfo table in MySQL DB (see MySQLContext)
    // [Index] makes SongUrl unique.
    [Index(nameof(SongUrl), IsUnique = true)]
    public class SongInfo
    {
        // [Key] is optional for special name Id
        [Key]
        public int Id { get; set; }

        // From Shazam       
        [StringLength(maximumLength: 30, MinimumLength = 1)]
        public string Artist { get; set; } = string.Empty;
        // Usually song title
        public string Description { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }

        // From LyricsService
        public string? Lyrics { get; set; }

        // From YouTube (could be a search url, if not navigated to a video)
        [Required(ErrorMessage = "SongUrl is a YouTube url and required")]
        public string SongUrl { get; set; } = string.Empty;

        public DateTime? ModifiedDateTime { get; set; }

        public override string ToString() => Artist.IsNotBlank() ? $"{Artist} - {Description}" : Description;
    }
}
