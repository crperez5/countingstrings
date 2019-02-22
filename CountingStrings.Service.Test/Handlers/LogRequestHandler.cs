using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CountingStrings.API.Contract;
using CountingStrings.Service.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CountingStrings.Service.Test.Handlers
{    
    public class LogRequestHandler : IDisposable
    {
        private readonly CountingStringsContext _context;
        private readonly string _connectionString;

        public LogRequestHandler()
        {
            _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            _context = Common.GetDbContext(_connectionString);

            PopulateDatabase();
        }


        [Fact]
        public async Task LogRequestConcurrency()
        {
            // Arrange
            var requestIds = new List<Guid>()
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

            var tasks = (from requestId in requestIds
                         let context = Common.GetDbContext(_connectionString)
                         let handler = new Service.Handlers.LogRequestHandler(context)
                         let message = new LogRequest { Id = requestId, RequestDate = DateTime.UtcNow }
                         select new Func<Task>(async () =>
                         {
                             await handler.Handle(message, new TestsMessageHandlerContext());
                         }))
                .ToList();

            // Act
            await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));

            // Assert
            var freshContext = Common.GetDbContext(_connectionString);

            var requestCount = await freshContext.RequestCounts.SingleAsync();

            Assert.Equal(10, requestCount.Count);
        }

        private void PopulateDatabase()
        {
            using (var context = Common.GetDbContext(_connectionString))
            {
                var requestCount = context.RequestCounts.Single();
                requestCount.Count = 0;
                context.SaveChanges();
            }
        }

        public void Dispose()
        {            
        }
    }
}
