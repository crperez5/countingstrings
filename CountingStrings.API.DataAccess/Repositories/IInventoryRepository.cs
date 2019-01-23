using System.Threading.Tasks;
using CountingStrings.API.DataAccess.Entities;

namespace CountingStrings.API.DataAccess.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory> GetById(int id);
    }
}
