using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using User.Models;
using App.Models;

namespace create.db
{
    public class DatabaseContext : DbContext
    {
        public DbSet<users> Users { get; set; }
        public DbSet<app> App { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(@"Data Source=database.db");
            // => options.UseSqlite(@"Data Source=/Users/lucie/Desktop/ETNA/TIC/WIN1/WIN/database.db");

        }
    
}
