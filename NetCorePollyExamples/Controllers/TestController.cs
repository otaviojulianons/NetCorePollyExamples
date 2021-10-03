using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCorePollyExamples.Services;
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
        private readonly ITokenService _tokenService;

        public TestController(
            ILogger<TestController> logger,
            IExternalService externalService,
            ITokenService tokenService)
        {
            _logger = logger;
            _externalService = externalService;
            _tokenService = tokenService;
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


        [HttpPost("token-validate")]
        public async Task<string> TokenValidade([FromQuery]string token)
        {
            try
            {
                var result = await _tokenService.Validate(token);
                return result.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
