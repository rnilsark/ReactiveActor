using System;
using System.Threading;
using System.Threading.Tasks;
using Actor1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace Web1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountController : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(Guid id)
        {
            var count = await ActorProxy.Create<IActor1>(new ActorId(id)).GetCountAsync(CancellationToken.None);
            return Ok(count);
        }

        [HttpPost("{id}")]
        public async Task Post(Guid id, [FromBody] int count)
        {
            await ActorProxy.Create<IActor1>(new ActorId(id)).SetCountAsync(count, CancellationToken.None);
        }
    }
}
