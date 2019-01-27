using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CountingStrings.API.Contract;
using CountingStrings.Service.Data;
using CountingStrings.Service.Extensions;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using NServiceBus.Logging;

namespace CountingStrings.Service.Handlers
{
    public class CloseSessionHandler : IHandleMessages<CloseSession>
    {
        private readonly CountingStringsContext _db;
        private readonly IMapper _mapper;
        private static readonly ILog Log = LogManager.GetLogger<CloseSessionHandler>();
        private const int Closed = 0;
        public CloseSessionHandler(CountingStringsContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task Handle(CloseSession message, IMessageHandlerContext context)
        {
            Log.Info($"CloseSession. SessionId '{message.SessionId}', ProcessId: '{Process.GetCurrentProcess().Id}'");

            var session = await _db.Sessions.SingleOrDefaultAsync(s => s.Id == message.SessionId);

            if (session == null || session.Status == Closed)
            {
                return;
            }

            session.Status = Closed;

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    await CloseSession();

                    await UpdateCounters();

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private async Task UpdateCounters()
        {
            var countersSaved = false;
            var sessionCounters = await this._db.SessionCounts.SingleAsync();
            sessionCounters.NumOpen -= 1;
            sessionCounters.NumClose += 1;

            while (!countersSaved)
            {
                try
                {
                    await _db.SaveChangesAsync();
                    countersSaved = true;
                }
                catch (Exception ex) when (ex.IsConcurrencyException())
                {
                    ResolveCounterConflicts(ex);
                }
            }
        }

        private static void ResolveCounterConflicts(Exception ex)
        {
            var dbUpdateException = (DbUpdateException)ex;
            var entry = dbUpdateException.Entries.First();
            var proposedValues = entry.CurrentValues;
            var databaseValues = entry.GetDatabaseValues();
            proposedValues["NumOpen"] = (int)databaseValues["NumOpen"] - 1;
            proposedValues["NumClose"] = (int)databaseValues["NumClose"] + 1;
            entry.OriginalValues.SetValues(databaseValues);
        }

        private async Task CloseSession()
        {
            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception ex) when (ex.IsConcurrencyException())
            {
                // Another process already closed the session. No more actions needed.
            }
        }
    }
}
