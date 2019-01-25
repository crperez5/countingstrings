using System;
using System.Threading.Tasks;
using CountingStrings.API.Contract;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

namespace CountingStrings.API.Controllers
{
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly IMessageSession _messageSession;

        public SessionsController(IMessageSession messageSession)
        {
            this._messageSession = messageSession;
        }

        [HttpPost]
        [Route("api/session/create")]
        public async Task Create()
        {
            var session = new
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow
            };

            var openSessionCommand = new OpenSession
            {
                SessionId = session.Id,
                DateCreated = session.DateCreated
            };

            await this._messageSession.Send(openSessionCommand);

            this.Created($"api/sessions/{session.Id}", session);
        }
    }
}