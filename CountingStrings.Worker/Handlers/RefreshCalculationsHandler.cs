using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CountingStrings.Service.Data;
using CountingStrings.Service.Data.Models;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using NServiceBus.Logging;

namespace CountingStrings.Worker.Handlers
{
    public class RefreshCalculationsHandler : IHandleMessages<RefreshCalculations>
    {
        private readonly CountingStringsContext _db;
        private readonly IMapper _mapper;
        private static readonly ILog Log = LogManager.GetLogger<RefreshCalculationsHandler>();

        public RefreshCalculationsHandler(CountingStringsContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task Handle(RefreshCalculations message, IMessageHandlerContext context)
        {
            Log.Info($"RefreshCalculations. ProcessId: '{Process.GetCurrentProcess().Id}'");

            var currentJob = new WorkerJob() { ProcessId = Process.GetCurrentProcess().Id, StartDate = DateTime.UtcNow };

            var lastJob = await _db.WorkerJobs.OrderByDescending(o => o.EndDate).FirstOrDefaultAsync();

            var lastJobEndDate = lastJob?.EndDate ?? DateTime.MinValue;

            var checkpoint = DateTime.UtcNow;

            var unprocessedSessionWords = await _db.SessionWords.Where(w => w.DateCreated > lastJobEndDate).ToListAsync();

            Log.Info($"{unprocessedSessionWords.Count} new words found.");

            await CalculateWordsPerSession(unprocessedSessionWords);

            await CalculateWordFrequency(unprocessedSessionWords);

            Log.Info($"Saving checkpoint. Next time will process words from date '{checkpoint:s}'.");
            currentJob.EndDate = DateTime.UtcNow;
            await _db.WorkerJobs.AddAsync(currentJob);
            await _db.SaveChangesAsync();
        }

        private async Task CalculateWordsPerSession(List<SessionWord> unprocessedSessionWords)
        {
            foreach (var unprocessedWord in unprocessedSessionWords.GroupBy(g => new { g.SessionId, g.Word }))
            {
                var currentCounts = await _db.SessionWordCounts.Where(s =>
                    s.SessionId == unprocessedWord.Key.SessionId && s.Word == unprocessedWord.Key.Word).ToListAsync();
                _db.SessionWordCounts.RemoveRange(currentCounts);

                var ocurrences = await _db.SessionWords.Where(s =>
                    s.SessionId == unprocessedWord.Key.SessionId && s.Word == unprocessedWord.Key.Word).ToListAsync();

                await _db.SessionWordCounts.AddAsync(new SessionWordCount
                {
                    SessionId = unprocessedWord.Key.SessionId,
                    Word = unprocessedWord.Key.Word,
                    Count = ocurrences.Count
                });
            }
            await _db.SaveChangesAsync();
        }

        private async Task CalculateWordFrequency(List<SessionWord> unprocessedSessionWords)
        {
            foreach (var unprocessedSessionWord in unprocessedSessionWords.GroupBy(g => new { g.Word, g.DateCreated.Date }))
            {
                var currentCounts = await _db.WordDateCounts.Where(s =>
                    s.Word == unprocessedSessionWord.Key.Word && s.DateCreated.Date == unprocessedSessionWord.Key.Date.Date).ToListAsync();
                _db.WordDateCounts.RemoveRange(currentCounts);

                var ocurrences = await _db.SessionWords.Where(s =>
                    s.Word == unprocessedSessionWord.Key.Word && s.DateCreated == unprocessedSessionWord.Key.Date.Date).ToListAsync();

                await _db.WordDateCounts.AddAsync(new WordDateCount
                {
                    Word = unprocessedSessionWord.Key.Word,
                    Date = unprocessedSessionWord.Key.Date,
                    Count = ocurrences.Count
                });
            }
            await _db.SaveChangesAsync();
        }
    }
}
