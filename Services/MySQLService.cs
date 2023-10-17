using System.Collections.Generic;
using System.Linq;
using CSharpWpfShazam.Data;
using CSharpWpfShazam.Models;

namespace CSharpWpfShazam.Services
{
    public class MySQLService
    {        
        public MySQLService()
        {
        }

        public List<SongInfo> GetAllSongInfos()
        {
            using var context = new MySQLContext();
            return context.SongInfos.ToList();
        }

        public bool AddSongInfo(SongInfo songInfo, out string error)
        {
            error = string.Empty;
            using var context = new MySQLContext();
            if (context.SongInfos.Any(x => x.SongUrl == songInfo.SongUrl))
            {
                error = $"Song url '{songInfo.SongUrl}' already exists in the database.";
                return false;
            }

            context.SongInfos.Add(songInfo);
            context.SaveChanges();
            return true;
        }

        public bool DeleteSongInfo(string songUrl)
        {
            using var context = new MySQLContext();
            var songInfo = context.SongInfos.FirstOrDefault(x => x.SongUrl == songUrl);
            if (songInfo != null)
            {
                context.Entry(songInfo).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                context.SaveChanges();
                return true;
            }
            return false;
        }       
    }
}
