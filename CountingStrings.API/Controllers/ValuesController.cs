using CountingStrings.API.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

namespace CountingStrings.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IInventoryRepository _inventoryRepository;

        private readonly IMessageSession _messageSession;

        public ValuesController(IInventoryRepository inventoryRepository, IMessageSession messageSession)
        {
            _inventoryRepository = inventoryRepository;
            _messageSession = messageSession;
        }

        //// GET api/values
        //[HttpGet]
        //public ActionResult<IEnumerable<string>> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}
        
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Inventory>> Get(int id)
        //{
        //    var message = new MyMessage();

        //    await this._messageSession.Send(message).ConfigureAwait(false);

        //    return await this._inventoryRepository.GetById(id);
        //}

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
