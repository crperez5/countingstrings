using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CountingStrings.API.Data.Models;

namespace CountingStrings.API.Data.Repositories
{
    public interface ICountingStringsRepository
    {
        Task<Session> GetByIdAsync(Guid sessionId);

        Task<List<Session>> GetAllSessionsAsync();

        Task<List<WordRepetition>> GetWordRepetitions(Guid sessionId);

        Task<List<WordFrequency>> GetWordFrequency(string word, DateTime? rangeStartDate, DateTime? rangeEndDate);

        Task<Stats> GetStats();
    }
}
