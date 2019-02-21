using AutoMapper;
using CountingStrings.Service.Data;
using CountingStrings.Service.Data.Models;
using System;

namespace CountingStrings.Worker.Test
{
    public class TestFixture : IDisposable
    {
        public CountingStringsContext Context { get; set; }

        public IMapper Mapper { get; set; }

        public Session ExistingSession { get; set; }

        public string ConnectionString { get; set; }

        public SessionWord ExistingSessionWord { get; set; }

        public SessionWordCount ExistingSessionWordCount { get; set; }

        public WordDateCount ExistingWordDateCount { get; set; }

        public TestFixture()
        {
            Mapper = Common.GetMapper();

            ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            Context = Common.GetDbContext(ConnectionString);

            PopulateDatabase();
        }

        private void PopulateDatabase()
        {
            Context.SessionCounts.Add(new SessionCount
            {
                Id = Guid.Parse("A9EF096F-9FB5-4408-B3FB-FF7F577D7C80"),
                NumOpen = 1,
                NumClose = 0
            });

            ExistingSession = new Session()
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow,
                Status = 1
            };

            Context.Sessions.Add(ExistingSession);

            ExistingSessionWord = new SessionWord()
            {
                SessionId = ExistingSession.Id,
                Word = "vanilla",
                DateCreated = DateTime.UtcNow.AddDays(-1)
            };

            Context.SessionWords.Add(ExistingSessionWord);

            ExistingSessionWordCount = new SessionWordCount()
            {
                SessionId = ExistingSession.Id,
                Word = "vanilla",
                Count = 1,
                DateCreated = DateTime.UtcNow.AddDays(-1)
            };

            Context.SessionWordCounts.Add(ExistingSessionWordCount);

            ExistingWordDateCount = new WordDateCount()
            {
                Word = "vanilla",
                Date = DateTime.UtcNow.AddDays(-1),
                Count = 1,
                DateCreated = DateTime.UtcNow.AddDays(-1)
            };

            Context.WordDateCounts.Add(ExistingWordDateCount);

            Context.WorkerJobs.Add(new WorkerJob
            {
                ProcessId = 1,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(-1)
            });
            Context.SaveChanges();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
        }
    }
}
