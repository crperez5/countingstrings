using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CountingStrings.API.Contract;
using CountingStrings.Service.Data;
using CountingStrings.Service.Data.Models;
using CountingStrings.Service.Extensions;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using NServiceBus.Logging;

namespace CountingStrings.Service.Handlers
{
    public class OpenSessionHandler : IHandleMessages<OpenSession>
    {
        private readonly CountingStringsContext _db;
        private readonly IMapper _mapper;
        private static readonly ILog Log = LogManager.GetLogger<OpenSessionHandler>();

        public OpenSessionHandler(CountingStringsContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task Handle(OpenSession message, IMessageHandlerContext context)
        {
            Log.Info($"OpenSession. SessionId '{message.SessionId}', ProcessId: '{Process.GetCurrentProcess().Id}'");

            var session = _mapper.Map<Session>(message);

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    await AddSession(session);

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
            var counterSaved = false;
            var sessionCounters = await this._db.SessionCounts.SingleAsync();
            sessionCounters.NumOpen += 1;

            while (!counterSaved)
            {
                try
                {
                    await _db.SaveChangesAsync();
                    counterSaved = true;
                }
                catch (Exception ex) when (ex.IsConcurrencyException())
                {
                    ResolveCounterConflicts(ex);
                }
            }
        }

        private async Task AddSession(Session session)
        {
            await this._db.Sessions.AddAsync(session);
            await this._db.SaveChangesAsync();
        }

        private static void ResolveCounterConflicts(Exception ex)
        {
            var dbUpdateException = (DbUpdateException)ex;
            var entry = dbUpdateException.Entries.First();
            var proposedValues = entry.CurrentValues;
            var databaseValues = entry.GetDatabaseValues();
            proposedValues["NumOpen"] = (int)databaseValues["NumOpen"] + 1;
            entry.OriginalValues.SetValues(databaseValues);
        }
    }
}