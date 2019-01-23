using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CountingStrings.API.DataAccess.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace CountingStrings.API.DataAccess.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly IConfiguration _config;

        public InventoryRepository(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<Inventory> GetById(int id)
        {
            using (var conn = Connection)
            {
                const string sQuery = "SELECT id, name, quantity FROM Inventory WHERE id = @ID";
                conn.Open();
                var result = await conn.QueryAsync<Inventory>(sQuery, new { ID = id });
                return result.FirstOrDefault();
            }
        }
    }
}
