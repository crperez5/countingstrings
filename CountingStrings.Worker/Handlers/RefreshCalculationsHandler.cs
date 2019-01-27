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

            await _db.WorkerJobs.AddAsync(currentJob);

            await _db.SaveChangesAsync();

            var lastJob = await _db.WorkerJobs.OrderByDescending(o => o.EndDate).FirstOrDefaultAsync();

            var baselineDate = lastJob?.EndDate ?? DateTime.MinValue;

            var unprocessedSessionWords = await _db.SessionWords.Where(w => w.DateCreated > baselineDate).ToListAsync();

            await CalculateWordsPerSession(unprocessedSessionWords);

            await CalculateWordFrequency(unprocessedSessionWords);

            currentJob.EndDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        private async Task CalculateWordsPerSession(List<SessionWord> unprocessedSessionWords)
        {
            var toBePurgedSessionWordCounts = new List<SessionWordCount>();

            foreach (var unprocessedSessionWord in unprocessedSessionWords.GroupBy(g => new { g.SessionId, g.Word }))
            {
                var sessionWordCounts = await _db.SessionWordCounts.Where(c =>
                        c.SessionId == unprocessedSessionWord.Key.SessionId && c.Word == unprocessedSessionWord.Key.Word)
                    .ToListAsync();

                toBePurgedSessionWordCounts.AddRange(sessionWordCounts);

                if (!sessionWordCounts.Any())
                {
                    await _db.SessionWordCounts.AddAsync(new SessionWordCount
                    {
                        SessionId = unprocessedSessionWord.Key.SessionId,
                        Word = unprocessedSessionWord.Key.Word,
                        Count = unprocessedSessionWord.Count()
                    });
                }
                else
                {
                    var current = sessionWordCounts.OrderByDescending(o => o.DateCreated).First();
                    await _db.SessionWordCounts.AddAsync(new SessionWordCount
                    {
                        SessionId = unprocessedSessionWord.Key.SessionId,
                        Word = unprocessedSessionWord.Key.Word,
                        Count = current.Count + unprocessedSessionWord.Count()
                    });
                }
            }

            _db.SessionWordCounts.RemoveRange(toBePurgedSessionWordCounts);
        }

        private async Task CalculateWordFrequency(List<SessionWord> unprocessedSessionWords)
        {
            var tobePurgedWordDateCounts = new List<WordDateCount>();

            foreach (var unprocessedSessionWord in unprocessedSessionWords.GroupBy(g => new { g.Word, g.DateCreated.Date }))
            {
                var wordDateCounts = await _db.WordDateCounts.Where(c =>
                        c.Word == unprocessedSessionWord.Key.Word && c.Date.Date == unprocessedSessionWord.Key.Date)
                    .ToListAsync();

                tobePurgedWordDateCounts.AddRange(wordDateCounts);

                if (!wordDateCounts.Any())
                {
                    await _db.WordDateCounts.AddAsync(new WordDateCount
                    {
                        Word = unprocessedSessionWord.Key.Word,
                        Date = unprocessedSessionWord.Key.Date,
                        Count = unprocessedSessionWord.Count()
                    });
                }
                else
                {
                    var current = wordDateCounts.OrderByDescending(o => o.DateCreated).First();
                    await _db.WordDateCounts.AddAsync(new WordDateCount
                    {
                        Word = unprocessedSessionWord.Key.Word,
                        Date = unprocessedSessionWord.Key.Date,
                        Count = current.Count + unprocessedSessionWord.Count()
                    });
                }
            }
            _db.WordDateCounts.RemoveRange(tobePurgedWordDateCounts);

        }
    }
}
