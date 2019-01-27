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

namespace CountingStrings.Service.Test.Handlers
{
    public class CloseSessionHandler : IDisposable
    {
        private readonly CountingStringsContext _context;
        private readonly IMapper _mapper;
        private readonly string _connectionString;
        private Session _existingSession;

        public CloseSessionHandler()
        {
            _mapper = Common.GetMapper();

            _connectionString =
                $"Server=(localdb)\\mssqllocaldb;Database=CountingStrings_{Guid.NewGuid()};Trusted_Connection=True;MultipleActiveResultSets=true";

            _context = Common.GetDbContext(_connectionString);

            _existingSession = new Session()
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow,
                Status = 1
            };

            PopulateDatabase();
        }

        [Fact]
        public async Task CloseSession()
        {

            // Arrange
            await _context.Sessions.AddAsync(_existingSession);
            await _context.SaveChangesAsync();
            var handler = new Service.Handlers.CloseSessionHandler(_context, _mapper);
            var message = new CloseSession { SessionId = _existingSession.Id };

            // Act
            await handler.Handle(message, new TestsMessageHandlerContext());

            // Assert
            var sessions = await _context.Sessions.ToListAsync();
            var sessionCounts = await _context.SessionCounts.ToListAsync();

            Assert.Single(sessions);
            var session = sessions.First();
            Assert.Equal(message.SessionId, session.Id);
            Assert.Equal(0, session.Status);

            Assert.Single(sessionCounts);
            var sessionCount = sessionCounts.First();
            Assert.Equal(0, sessionCount.NumOpen);
            Assert.Equal(1, sessionCount.NumClose);
        }

        [Fact]
        public async Task CloseSessionConcurrency()
        {
            // Arrange
            var existingSessionsGuids = new List<Guid>()
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };
            existingSessionsGuids.ForEach(async sessionId =>
                await _context.Sessions.AddAsync(new Session { Id = sessionId, DateCreated = DateTime.UtcNow }));
            var sessionCountsBeforeTest = await _context.SessionCounts.FirstAsync();
            sessionCountsBeforeTest.NumOpen = existingSessionsGuids.Count;
            await _context.SaveChangesAsync();

            var tasks = (from sessionId in existingSessionsGuids
                         let context = Common.GetDbContext(_connectionString)
                         let mapper = Common.GetMapper()
                         let handler = new Service.Handlers.CloseSessionHandler(context, mapper)
                         let message = new CloseSession { SessionId = sessionId }
                         select new Func<Task>(async () =>
                             {
                                 await handler.Handle(message, new TestsMessageHandlerContext());
                             }))
                .ToList();

            // Act
            await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));


            // Assert
            var refreshedContext = Common.GetDbContext(_connectionString);

            var sessions = await refreshedContext.Sessions.ToListAsync();
            var sessionCounts = await refreshedContext.SessionCounts.ToListAsync();

            Assert.All(sessions, session => session.Status = 0);

            var sessionCount = sessionCounts.First();

            Assert.Equal(0, sessionCount.NumOpen);
            Assert.Equal(existingSessionsGuids.Count, sessionCount.NumClose);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }

        private void PopulateDatabase()
        {
            _context.Database.Migrate();

            _context.SessionCounts.Add(new SessionCount
            {
                NumOpen = 1,
                NumClose = 0
            });

            _context.SaveChanges();
        }
    }
}
