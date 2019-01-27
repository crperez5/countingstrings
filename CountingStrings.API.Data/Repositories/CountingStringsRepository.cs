using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CountingStrings.API.Data.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace CountingStrings.API.Data.Repositories
{
    public class CountingStringsRepository : ICountingStringsRepository
    {
        private readonly IConfiguration _config;

        public CountingStringsRepository(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<Session> GetByIdAsync(Guid id)
        {
            using (var conn = Connection)
            {
                const string sQuery = "SELECT Id, Status, DateCreated FROM Sessions WHERE id = @ID";
                conn.Open();
                var result = await conn.QueryAsync<Session>(sQuery, new { ID = id });
                return result.FirstOrDefault();
            }
        }

        public async Task<List<Session>> GetAllSessionsAsync()
        {
            using (var conn = Connection)
            {
                const string sQuery = "SELECT Id, Status, DateCreated FROM Sessions";
                conn.Open();
                var result = await conn.QueryAsync<Session>(sQuery);
                return result.ToList();
            }
        }

        public async Task<List<WordRepetition>> GetWordRepetitions(Guid sessionId)
        {
            using (var conn = Connection)
            {
                const string sQuery = "SELECT Word, Count FROM SessionWordCounts WHERE SessionId = @ID";
                conn.Open();
                var result = await conn.QueryAsync<WordRepetition>(sQuery, new { ID = sessionId });
                return result.ToList();
            }
        }

        public async Task<List<WordFrequency>> GetWordFrequency(string word, DateTime? rangeStartDate, DateTime? rangeEndDate)
        {
            const string sqlTemplate = @"SELECT TOP 1 Word, Date, Count FROM WordDateCounts /**where**/ ORDER BY DateCreated DESC";

            var sqlBuilder = new SqlBuilder();
            var template = sqlBuilder.AddTemplate(sqlTemplate);

            sqlBuilder.Where("Word = @Word", new { Word = word });

            if (rangeStartDate != null)
            {
                sqlBuilder.Where("Date >= @StartDate", new { StartDate = rangeStartDate.Value.Date.ToString("s") });
            }
            if (rangeEndDate != null)
            {
                sqlBuilder.Where("Date <= @EndDate", new { EndDate = rangeEndDate.Value.Date.ToString("s") });
            }

            using (var conn = Connection)
            {
                conn.Open();
                var result = await conn.QueryAsync<WordFrequency>(template.RawSql, template.Parameters);
                return result.ToList();
            }
        }

        public async Task<Stats> GetStats()
        {
            using (var conn = Connection)
            {
                conn.Open();

                var taskSessionStats = conn.QuerySingleAsync("SELECT NumOpen, NumClose FROM SessionCounts");
                var taskRequestStats = conn.QuerySingleAsync("SELECT Count FROM RequestCount");

                await Task.WhenAll(taskSessionStats, taskRequestStats);

                return new Stats
                {
                    OpenSessions = taskSessionStats.Result.NumOpen,
                    CloseSessions = taskSessionStats.Result.NumClose,
                    TotalRequests = taskRequestStats.Result.Count
                };
            }
        }
    }
}
