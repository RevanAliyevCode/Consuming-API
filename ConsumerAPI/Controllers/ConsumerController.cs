using ConsumerAPI.Services.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsumerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumerController : ControllerBase
    {
        readonly IConsumerService _consumerService;

        public ConsumerController(IConsumerService consumerService)
        {
            _consumerService = consumerService;
        }

        [HttpGet]
        public async Task<IActionResult> ConsumeAsync()
        {
            await _consumerService.ConsumeAsync();
            return Ok();
        }
    }
}
