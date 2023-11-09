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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Note: without this, table name would be SongInfos
            modelBuilder.Entity<SongInfo>().ToTable("SongInfo");
        }

        // SongInfo table
        public DbSet<SongInfo> SongInfo { get; set; }
    }
}
