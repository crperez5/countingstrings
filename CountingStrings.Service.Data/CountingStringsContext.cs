using CountingStrings.Service.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CountingStrings.Service.Data.Contexts
{
    public class CountingStringsContext : DbContext
    {
        public DbSet<SessionCounts> SessionCounts { get; set; }

        public DbSet<Session> Sessions{ get; set; }

        public CountingStringsContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildSessionsModel(modelBuilder);
        }

        private static void BuildSessionsModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Session>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<SessionCounts>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<SessionCounts>()
                .Property(p => p.NumOpen)
                .IsConcurrencyToken();

            modelBuilder.Entity<SessionCounts>()
                .Property(p => p.NumClose)
                .IsConcurrencyToken();
        }
    }
}
