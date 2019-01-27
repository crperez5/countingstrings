using System.Threading.Tasks;
using CountingStrings.API.Data.Models;
using CountingStrings.API.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CountingStrings.API.Controllers
{
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly ICountingStringsRepository _countingStringsRepository;

        public StatsController(ICountingStringsRepository countingStringsRepository)
        {
            _countingStringsRepository = countingStringsRepository;
        }

        [HttpGet]
        [Route("api/stats")]
        [ProducesResponseType(typeof(Stats), 200)]
        public async Task<IActionResult> GetStats()
        {
            var sessions = await this._countingStringsRepository.GetStats();
            return Ok(sessions);
        }
    }
}