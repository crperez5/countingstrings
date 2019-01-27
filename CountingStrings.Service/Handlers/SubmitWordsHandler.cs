using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CountingStrings.API.Contract;
using CountingStrings.Service.Data;
using CountingStrings.Service.Data.Models;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using NServiceBus.Logging;

namespace CountingStrings.Service.Handlers
{
    public class SubmitWordsHandler : IHandleMessages<SubmitWords>
    {
        private readonly CountingStringsContext _db;
        private readonly IMapper _mapper;
        private static readonly ILog Log = LogManager.GetLogger<SubmitWordsHandler>();

        public SubmitWordsHandler(CountingStringsContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task Handle(SubmitWords message, IMessageHandlerContext context)
        {
            Log.Info($"SubmitWords. SessionId '{message.SessionId}', ProcessId: '{Process.GetCurrentProcess().Id}'");

            var session = await _db.Sessions.SingleOrDefaultAsync(s => s.Id == message.SessionId);

            if (session == null)
            {
                return;
            }

            if (session.Status == 0)
            {
                return;
            }

            var sessionWords = new List<SessionWord>();
            sessionWords.AddRange(message.Words.Select(word => new SessionWord
            {
                SessionId = message.SessionId,
                Word = word
            }));

            await _db.SessionWords.AddRangeAsync(sessionWords);

            await _db.SaveChangesAsync();
        }
    }
}
