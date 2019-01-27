using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CountingStrings.API.Data.Models;
using CountingStrings.API.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

namespace CountingStrings.API.Controllers
{
    [ApiController]
    public class WordsController : ControllerBase
    {
        private readonly IMessageSession _messageSession;
        private readonly ICountingStringsRepository _countingStringsRepository;

        public WordsController(IMessageSession messageSession, ICountingStringsRepository countingStringsRepository)
        {
            _messageSession = messageSession;
            _countingStringsRepository = countingStringsRepository;
        }

        [HttpGet]
        [Route("api/words/{word}/frequency")]
        [ProducesResponseType(typeof(List<WordFrequency>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetWordFrequency(string word, [FromQuery] DateTime? rangeStartDate, [FromQuery] DateTime? rangeEndDate)
        {
            if (word == string.Empty)
            {
                return BadRequest();
            }

            var wordFrequency = 
                await this._countingStringsRepository.GetWordFrequency(word, rangeStartDate, rangeEndDate);

            if (wordFrequency == null)
            {
                return NotFound(new { Detail = "Word not recognized" });
            }

            return Ok(wordFrequency);
        }
    }
}