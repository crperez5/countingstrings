using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CountingStrings.API.Contract;
using CountingStrings.Service.Data;
using CountingStrings.Service.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CountingStrings.Service.Test.Handlers
{
    [Collection("SessionTests")]
    public class OpenSessionHandler : IDisposable
    {
        private readonly CountingStringsContext _context;
        private readonly IMapper _mapper;
        private readonly string _connectionString;

        public OpenSessionHandler()
        {
            _mapper = Common.GetMapper();

            _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            _context = Common.GetDbContext(_connectionString);

            PopulateDatabase();
        }

        [Fact]
        public async Task OpenSession()
        {

            // Arrange
            var handler = new Service.Handlers.OpenSessionHandler(_context, _mapper);
            var message = new OpenSession { SessionId = Guid.NewGuid(), DateCreated = DateTime.UtcNow };

            // Act
            await handler.Handle(message, new TestsMessageHandlerContext());

            // Assert
            var sessions = await _context.Sessions.ToListAsync();
            var sessionCounts = await _context.SessionCounts.ToListAsync();

            Assert.Single(sessions);
            var session = sessions.First();
            Assert.Equal(message.SessionId, session.Id);
            Assert.Equal(message.DateCreated, session.DateCreated);

            Assert.Single(sessionCounts);
            var sessionCount = sessionCounts.First();
            Assert.Equal(1, sessionCount.NumOpen);
            Assert.Equal(0, sessionCount.NumClose);
        }

        [Fact]
        public async Task OpenSessionConcurrency()
        {
            // Arrange
            var tasks = new List<Func<Task>>();
            var eventsFired = 10;
            for (var i = 0; i < eventsFired; i++)
            {
                var context = Common.GetDbContext(_connectionString);
                var mapper = Common.GetMapper();
                var handler = new Service.Handlers.OpenSessionHandler(context, mapper);
                var message = new OpenSession { SessionId = Guid.NewGuid(), DateCreated = DateTime.UtcNow };

                var coldTask = new Func<Task>(async () =>
                {
                    await handler.Handle(message, new TestsMessageHandlerContext());
                });
                tasks.Add(coldTask);
            }

            // Act
            await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));


            // Assert
            var refreshedContext = Common.GetDbContext(_connectionString);

            var sessions = await refreshedContext.Sessions.ToListAsync();
            var sessionCounts = await refreshedContext.SessionCounts.ToListAsync();

            Assert.Equal(eventsFired, sessions.Count);

            Assert.Single(sessionCounts);

            var sessionCount = sessionCounts.First();
            Assert.Equal(eventsFired, sessionCount.NumOpen);
            Assert.Equal(0, sessionCount.NumClose);
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
