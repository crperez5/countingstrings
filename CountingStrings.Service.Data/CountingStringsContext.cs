using CountingStrings.Service.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CountingStrings.Service.Data
{
    public class CountingStringsContext : DbContext
    {
        public DbSet<SessionWord> SessionWords { get; set; }
        public DbSet<SessionWordCount> SessionWordCounts { get; set; }

        public DbSet<WordDateCount> WordDateCounts { get; set; }

        public DbSet<SessionCount> SessionCounts { get; set; }

        public DbSet<RequestCount> RequestCounts { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<WorkerJob> WorkerJobs { get; set; }

        public CountingStringsContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildSessionsModel(modelBuilder);
        }

        private static void BuildSessionsModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SessionWordCount>()
                .HasKey(h => h.Id);

            modelBuilder.Entity<SessionWordCount>()
                .Property(s => s.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<SessionWordCount>()
                .Property(s => s.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<SessionWordCount>()
                .ToTable("SessionWordCounts");

            modelBuilder.Entity<WordDateCount>()
                .HasKey(h => h.Id);

            modelBuilder.Entity<WordDateCount>()
                .Property(s => s.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<WordDateCount>()
                .Property(s => s.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<WordDateCount>()
                .ToTable("WordDateCounts");

            modelBuilder.Entity<SessionWord>()
                .HasKey(h => h.Id);

            modelBuilder.Entity<SessionWord>()
                .Property(s => s.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<SessionWord>()
                .Property(s => s.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<SessionWord>()
                .ToTable("SessionWords");

            modelBuilder.Entity<SessionCount>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<SessionCount>()
                .Property(s => s.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<SessionCount>()
                .Property(s => s.NumOpen)
                .IsConcurrencyToken();

            modelBuilder.Entity<SessionCount>()
                .Property(s => s.NumClose)
                .IsConcurrencyToken();

            modelBuilder.Entity<SessionCount>()
                .ToTable("SessionCounts");

            modelBuilder.Entity<Session>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Session>()
                .Property(s => s.Status)
                .HasDefaultValue(1);

            modelBuilder.Entity<Session>()
                .Property(s => s.Status)
                .IsConcurrencyToken();

            modelBuilder.Entity<Session>()
                .ToTable("Sessions");

            modelBuilder.Entity<WorkerJob>()
                .HasKey(h => h.Id);

            modelBuilder.Entity<WorkerJob>()
                .Property(s => s.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<WorkerJob>()
                .Property(h => h.EndDate)
                .IsRequired(false);

            modelBuilder.Entity<WorkerJob>()
                .ToTable("WorkerJobs");

            modelBuilder.Entity<RequestCount>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<RequestCount>()
                .Property(s => s.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<RequestCount>()
                .Property(s => s.Count)
                .IsConcurrencyToken();

            modelBuilder.Entity<RequestCount>()
                .ToTable("RequestCount");
        }
    }
}
