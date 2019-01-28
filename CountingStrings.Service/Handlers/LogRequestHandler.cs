using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CountingStrings.API.Contract;
using CountingStrings.Service.Data;
using CountingStrings.Service.Extensions;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using NServiceBus.Logging;

namespace CountingStrings.Service.Handlers
{
    public class LogRequestHandler : IHandleMessages<LogRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger<LogRequestHandler>();

        private readonly CountingStringsContext _db;

        public LogRequestHandler(CountingStringsContext db)
        {
            _db = db;
        }

        public async Task Handle(LogRequest message, IMessageHandlerContext context)
        {
            Log.Info($"LogRequest. RequestId: '{message.Id}', ProcessId: '{Process.GetCurrentProcess().Id}'");

            var requestSaved = false;

            var sessionCounters = await this._db.RequestCounts.SingleAsync();

            sessionCounters.Count += 1;

            while (!requestSaved)
            {
                try
                {
                    await _db.SaveChangesAsync();
                    requestSaved = true;
                }
                catch (Exception ex) when (ex.IsConcurrencyException())
                {
                    var dbUpdateException = (DbUpdateException)ex;
                    var entry = dbUpdateException.Entries.First();
                    var proposedValues = entry.CurrentValues;
                    var databaseValues = entry.GetDatabaseValues();
                    proposedValues["Count"] = (int) databaseValues["Count"] + 1;
                    entry.OriginalValues.SetValues(databaseValues);
                }
            }
        }
    }
}
