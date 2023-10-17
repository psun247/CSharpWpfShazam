using Microsoft.EntityFrameworkCore;
using CSharpWpfShazam.Models;

namespace CSharpWpfShazam.Data
{
    public class MySQLContext : DbContext
    {
        // Note: ensure to match your local MySQL installation and configuration
        //       Use PM> Update-Database to create CSharpWpfShazamDB with data in Migrations folder
        private const string _MySQLConnectionString = "Server=localhost;Database=CSharpWpfShazamDB;User=root;Password=pass1234";

        // This ctor is needed for PM> Update-Database
        public MySQLContext()
        {
        }        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {           
            optionsBuilder.UseMySql(_MySQLConnectionString, ServerVersion.AutoDetect(_MySQLConnectionString));
        }

        // SongInfos table
        public DbSet<SongInfo> SongInfos { get; set; }
    }
}
