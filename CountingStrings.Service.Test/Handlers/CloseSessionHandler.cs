using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CountingStrings.API.Contract;
using CountingStrings.Service.Data;
using CountingStrings.Service.Data.Extensions;
using CountingStrings.Service.Data.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CountingStrings.Service.Test.Handlers
{
    [Collection("SessionTests")]
    public class CloseSessionHandler : IDisposable
    {
        private readonly CountingStringsContext _context;
        private readonly IMapper _mapper;
        private readonly string _connectionString;
        private Session _existingSession;

        public CloseSessionHandler()
        {
            _mapper = Common.GetMapper();

            _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

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
            var sessionCount = await _context.SessionCounts.SingleAsync();
            sessionCount.NumClose = 0;
            sessionCount.NumOpen = 1;
            await _context.Sessions.AddAsync(_existingSession);
            await _context.SaveChangesAsync();
            var handler = new Service.Handlers.CloseSessionHandler(_context, _mapper);
            var message = new CloseSession { SessionId = _existingSession.Id };

            // Act
            await handler.Handle(message, new TestsMessageHandlerContext());

            // Assert
            var refreshedContext = Common.GetDbContext(_connectionString);
            var session = await refreshedContext.Sessions.SingleAsync();
            sessionCount = await refreshedContext.SessionCounts.SingleAsync();
            Assert.Equal(message.SessionId, session.Id);
            Assert.Equal(0, session.Status);
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
            using (var context = Common.GetDbContext(_connectionString))
            {
                context.Sessions.Clear();
                context.SaveChanges();
            }
        }

        private void PopulateDatabase()
        {
            using (var context = Common.GetDbContext(_connectionString))
            {
                var sessionCount = context.SessionCounts.Single();
                sessionCount.NumOpen = 0;
                sessionCount.NumClose = 0;
                context.SaveChanges();
            }
        }
    }
}
