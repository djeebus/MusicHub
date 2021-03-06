﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    public class DbContext : global::System.Data.Entity.DbContext
    {
        static DbContext()
        {
            Database.SetInitializer(new DbContextInitializer());
        }

        public DbSet<DbUser> Users { get; set; }
        public DbSet<DbConnection> Connections { get; set; }
        public DbSet<DbLibrary> Libraries { get; set; }
        public DbSet<DbSong> Songs { get; set; }
        public DbSet<DbSongAffinity> SongAffinities { get; set; }

        public bool AutoDetectChangesEnabled
        {
            get { return this.Configuration.AutoDetectChangesEnabled; }
            set { this.Configuration.AutoDetectChangesEnabled = value; }
        }
    }
}
