using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NetCorePollyExamples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExternalController : ControllerBase
    {
        private readonly ILogger<ExternalController> _logger;

        public ExternalController(ILogger<ExternalController> logger)
        {
            _logger = logger;
        }

        [HttpGet("slow")]
        public async Task<string> Slow()
        {
            var timeStart = 1000;
            var timeEnd = 3000;
            await Task.Delay(new Random().Next(timeStart, timeEnd));
            return $"OK in ({timeStart} ~ {timeEnd})ms";
        }

        [HttpGet("unstable")]
        public async Task<string> Unstable()
        {
            if (DateTime.Now.Minute % 2 == 0)
            {
                return $"OK";
            }
            else
            {
                await Task.Delay(3000);
                return $"OK in 3000ms";
            }
        }
    }
}
