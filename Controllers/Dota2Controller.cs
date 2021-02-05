using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JWT.Serializers;
using JWT.Algorithms;
using JWT;

namespace Dota2ExtensionEBS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dota2Controller : ControllerBase
    {
        private readonly ILogger<Dota2Controller> _logger;
        private IPubSubService _pubSubService;

        public Dota2Controller(ILogger<Dota2Controller> logger, IPubSubService pubSubService)
        {
            _logger = logger;
            _pubSubService = pubSubService;
        }

        [HttpPost]
        [Route("submitGameData/{channelId}")]
        public IActionResult submitGameData([FromRoute] int channelId, [FromBody] Dota2Data data) {
            validateDota2Data(data);
            _pubSubService.SendPubSubMessage(channelId.ToString(), data);
            return Ok();
        }

        public void validateDota2Data(Dota2Data data)
        {
            return;
        }
    }
}
