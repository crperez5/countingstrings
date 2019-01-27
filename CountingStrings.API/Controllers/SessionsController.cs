using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CountingStrings.API.Contract;
using CountingStrings.API.Data.Models;
using CountingStrings.API.Data.Repositories;
using CountingStrings.API.SwaggerProviders;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Swashbuckle.AspNetCore.Examples;

namespace CountingStrings.API.Controllers
{
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly IMessageSession _messageSession;
        private readonly ICountingStringsRepository _countingStringsRepository;

        public SessionsController(IMessageSession messageSession, ICountingStringsRepository countingStringsRepository)
        {
            _messageSession = messageSession;
            _countingStringsRepository = countingStringsRepository;
        }

        [HttpGet]
        [Route("api/sessions")]
        [ProducesResponseType(typeof(List<Session>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var sessions = await this._countingStringsRepository.GetAllSessionsAsync();
            return Ok(sessions);
        }

        [HttpGet]
        [Route("api/sessions/{id}/words")]
        [ProducesResponseType(typeof(List<WordRepetition>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetWordRepetitions(Guid id)
        {
            var session = await this._countingStringsRepository.GetByIdAsync(id);

            if (session == null)
            {
                return NotFound(new { Detail = "Session not recognized" });
            }

            var wordRepetitions = await this._countingStringsRepository.GetWordRepetitions(id);
            return Ok(wordRepetitions);
        }

        [HttpPost]
        [Route("api/sessions/open")]
        [ProducesResponseType(201, Type = typeof(Session))]
        public async Task<IActionResult> Open()
        {
            var session = new
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow,
                Status = 1
            };

            var openSessionCommand = new OpenSession
            {
                SessionId = session.Id,
                DateCreated = session.DateCreated
            };

            await this._messageSession.Send(openSessionCommand);

            return this.Created($"api/sessions/{session.Id}", session);
        }

        [HttpPut]
        [Route("api/sessions/{id}/close")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Close(Guid id)
        {
            var session = await this._countingStringsRepository.GetByIdAsync(id);

            if (session == null)
            {
                return BadRequest(new { Detail = "Session not recognized" });
            }

            if (session.Status == 0)
            {
                return BadRequest(new { Detail = "Session is already closed" });
            }

            var closeSessionCommand = new CloseSession
            {
                SessionId = session.Id,
            };

            await this._messageSession.Send(closeSessionCommand);

            return NoContent();
        }

        [HttpPost]
        [Route("api/sessions/{id}/submit")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [SwaggerRequestExample(typeof(List<string>), typeof(SubmitWordsSwaggerProvider))]

        public async Task<IActionResult> SubmitWords(Guid id, [FromBody]List<string> words)
        {
            if (!words.Any())
            {
                return BadRequest(new { Detail = "Cannot submit empty list of words" });
            }

            var session = await this._countingStringsRepository.GetByIdAsync(id);

            if (session == null)
            {
                return BadRequest(new { Detail = "Session not recognized" });
            }

            if (session.Status == 0)
            {
                return BadRequest(new { Detail = "Session is already closed" });
            }

            var submitWordsCommand = new SubmitWords
            {
                SessionId = session.Id,
                Words = words,
                DateModified = DateTime.UtcNow
            };

            await this._messageSession.Send(submitWordsCommand);

            return NoContent();
        }
    }
}