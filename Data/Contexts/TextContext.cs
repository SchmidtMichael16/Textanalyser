using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Contexts
{
    public class TextContext : DbContext
    {
        public DbSet<Text> Texts { get; set; }
        public TextContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Text>();
            //base.OnModelCreating(modelBuilder);
        }
    }
}
