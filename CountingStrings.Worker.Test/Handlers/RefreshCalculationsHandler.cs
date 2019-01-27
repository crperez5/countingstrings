using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CountingStrings.API.Contract;
using CountingStrings.Service.Data;
using CountingStrings.Service.Data.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CountingStrings.Worker.Test.Handlers
{
    public class RefreshCalculationsHandler : IDisposable
    {
        private CountingStringsContext _context;
        private readonly IMapper _mapper;
        private readonly string _connectionString;
        private Session _existingSession;
        private SessionWord _existingSessionWord;
        private SessionWordCount _existingSessionWordCount;
        private WordDateCount _existingWordDateCount;

        public RefreshCalculationsHandler()
        {
            _mapper = Common.GetMapper();

            _connectionString =
                $"Server=(localdb)\\mssqllocaldb;Database=CountingStrings_{Guid.NewGuid()};Trusted_Connection=True;MultipleActiveResultSets=true";

            _context = Common.GetDbContext(_connectionString);

            PopulateDatabase();
        }

        [Fact]
        public async Task CalculateWordsPerSession()
        {
            // Arrange
            var handler = new Service.Handlers.SubmitWordsHandler(_context, _mapper);
            var message = new SubmitWords
            {
                SessionId = _existingSession.Id,
                Words = new List<string>
                {
                    "chocolate",
                    "vanilla",
                },
                DateModified = DateTime.UtcNow
            };
            await handler.Handle(message, new TestsMessageHandlerContext());


            // Act
            await new Worker.Handlers.RefreshCalculationsHandler(Common.GetDbContext(_connectionString), Common.GetMapper())
                .Handle(new RefreshCalculations(), new TestsMessageHandlerContext());

            // Assert
            _context = Common.GetDbContext(_connectionString);
            var vanillaCounts = await _context.SessionWordCounts.SingleAsync(s => s.Word == "vanilla");
            var chocolateCounts = await _context.SessionWordCounts.SingleAsync(s => s.Word == "chocolate");
            Assert.Equal(2, vanillaCounts.Count);
            Assert.Equal(1, chocolateCounts.Count);
        }

        [Fact]
        public async Task CalculateWordFrequency()
        {
            // Arrange
            var handler = new Service.Handlers.SubmitWordsHandler(Common.GetDbContext(_connectionString), Common.GetMapper());
            var worker =
                new Worker.Handlers.RefreshCalculationsHandler(Common.GetDbContext(_connectionString),
                    Common.GetMapper());

            var messageOld = new SubmitWords
            {
                SessionId = _existingSession.Id,
                Words = new List<string>
                {
                    "chocolate",    // new
                    "vanilla",      // existing
                },
                DateModified = DateTime.UtcNow
            };

            await handler.Handle(messageOld, new TestsMessageHandlerContext());

            // Act
            await worker.Handle(new RefreshCalculations(), new TestsMessageHandlerContext());

            //Assert
            var refreshedContext = Common.GetDbContext(_connectionString);
            var vanillaData = await refreshedContext.WordDateCounts.Where(s => s.Word == "vanilla").ToListAsync();
            var chocolateData = await _context.WordDateCounts.Where(s => s.Word == "chocolate").ToListAsync();

            Assert.True(vanillaData.Count == 2);
            Assert.Contains(vanillaData, d => d.Date.Date == DateTime.UtcNow.Date);
            Assert.Contains(vanillaData, d => d.Date.Date == DateTime.UtcNow.AddDays(-1).Date);
            Assert.True(vanillaData.All(d => d.Count == 1));
            Assert.True(chocolateData.Count == 1);
            Assert.Contains(chocolateData, d => d.Date.Date == DateTime.UtcNow.Date);
            Assert.True(vanillaData.All(d => d.Count == 1));

            // Submit new words and recalculate counts.

            // Arrange
            handler = new Service.Handlers.SubmitWordsHandler(Common.GetDbContext(_connectionString), Common.GetMapper());
            var messageNew = new SubmitWords
            {
                SessionId = _existingSession.Id,
                Words = new List<string>
                {
                    "chocolate",    // existing
                    "vanilla",      // existing
                },
                DateModified = DateTime.UtcNow
            };

            await handler.Handle(messageNew, new TestsMessageHandlerContext());

            // Act
            await new Worker.Handlers.RefreshCalculationsHandler(Common.GetDbContext(_connectionString), Common.GetMapper())
                .Handle(new RefreshCalculations(), new TestsMessageHandlerContext());

            //Assert
            _context = Common.GetDbContext(_connectionString);
            vanillaData = await _context.WordDateCounts.Where(s => s.Word == "vanilla").ToListAsync();
            chocolateData = await _context.WordDateCounts.Where(s => s.Word == "chocolate").ToListAsync();

            Assert.True(vanillaData.Count == 2);
            Assert.NotNull(vanillaData.SingleOrDefault(v => v.Date.Date == DateTime.UtcNow.Date && v.Count == 2));
            Assert.NotNull(vanillaData.SingleOrDefault(v => v.Date.Date == DateTime.UtcNow.AddDays(-1).Date && v.Count == 1));
            Assert.True(chocolateData.Count == 1);
            Assert.NotNull(chocolateData.SingleOrDefault(v => v.Date.Date == DateTime.UtcNow.Date && v.Count == 2));
        }

        private void PopulateDatabase()
        {
            _context.Database.Migrate();

            _context.SessionCounts.Add(new SessionCount
            {
                Id = Guid.Parse("A9EF096F-9FB5-4408-B3FB-FF7F577D7C80"),
                NumOpen = 1,
                NumClose = 0
            });

            _existingSession = new Session()
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow,
                Status = 1
            };

            _context.Sessions.Add(_existingSession);

            _existingSessionWord = new SessionWord()
            {
                SessionId = _existingSession.Id,
                Word = "vanilla",
                DateCreated = DateTime.UtcNow.AddDays(-1)
            };

            _context.SessionWords.Add(_existingSessionWord);

            _existingSessionWordCount = new SessionWordCount()
            {
                SessionId = _existingSession.Id,
                Word = "vanilla",
                Count = 1,
                DateCreated = DateTime.UtcNow.AddDays(-1)
            };

            _context.SessionWordCounts.Add(_existingSessionWordCount);

            _existingWordDateCount = new WordDateCount()
            {
                Word = "vanilla",
                Date = DateTime.UtcNow.AddDays(-1),
                Count = 1,
                DateCreated = DateTime.UtcNow.AddDays(-1)
            };

            _context.WordDateCounts.Add(_existingWordDateCount);

            _context.WorkerJobs.Add(new WorkerJob
            {
                ProcessId = 1,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(-1)
            });
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
