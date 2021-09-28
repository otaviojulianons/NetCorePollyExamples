using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCorePollyExamples.HttpClients;
using System;
using System.Threading.Tasks;

namespace NetCorePollyExamples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {

        private readonly ILogger<TestController> _logger;
        private readonly IExternalService _externalService;

        public TestController(ILogger<TestController> logger, IExternalService externalService)
        {
            _logger = logger;
            _externalService = externalService;
        }

        [HttpGet("slow")]
        public async Task<string> Slow()
        {
            try
            {
                await _externalService.Slow();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet("unstable")]
        public async Task<string> Unstable()
        {
            try
            {
                await _externalService.Unstable();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
