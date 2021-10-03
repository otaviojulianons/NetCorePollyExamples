using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Threading.Tasks;

namespace NetCorePollyExamples.Services
{
    public class ResilientTokenService : ITokenService
    {
        private readonly ITokenService _userRepository;
        private static AsyncPolicyWrap<bool> _policyWrap;

        public ResilientTokenService(
            ITokenService userRepository,
            ILoggerFactory loggerFactory)
        {
            _userRepository = userRepository;
            CreateResilientPolicyIfNotExist(loggerFactory);
        }

        private void CreateResilientPolicyIfNotExist(ILoggerFactory loggerFactory)
        {
            if (_policyWrap == null)
            {
                var logger = loggerFactory.CreateLogger<ResilientTokenService>();
                var fallbackPolicy = CreateFallbackPolicy(logger);
                var circuitPolicy = CreateCircuitPolicy(logger);
                var timeoutPolicy = CreateTimeoutPolicy(logger);
                _policyWrap = Policy.WrapAsync(fallbackPolicy, circuitPolicy, timeoutPolicy);
            }
        }

        private AsyncFallbackPolicy<bool> CreateFallbackPolicy(ILogger<ResilientTokenService> logger)
        {
            return Policy<bool>.Handle<TimeoutException>().Or<BrokenCircuitException>()
                .FallbackAsync(
                    true,
                    onFallbackAsync: (result, context) =>
                    {
                        logger.LogInformation($"###-FALLBACK: fallback value substituted, due to: {result.Exception}.");
                        return Task.FromResult(result.Result);
                    });
        }

        private AsyncCircuitBreakerPolicy<bool> CreateCircuitPolicy(ILogger<ResilientTokenService> logger)
        {
            int exceptionsAllowedBeforeBreaking = 3;
            TimeSpan dureationOfBreak = TimeSpan.FromSeconds(30);
            return Policy<bool>.Handle<Exception>().CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking,
                dureationOfBreak,
                onBreak: (result, timespan, context) =>
                {
                    logger.LogError($"###-CIRCUIT_BREAKER -> ResilentTokenService: ON-BREAK");
                },
                onReset: (context) =>
                {
                    logger.LogError($"###-CIRCUIT_BREAKER -> ResilentTokenService: ON-RESET");
                },
                onHalfOpen: () =>
                {
                    logger.LogError($"###-CIRCUIT_BREAKER -> ResilentTokenService: ON-HALF-OPEN");
                });
        }

        private AsyncTimeoutPolicy<bool> CreateTimeoutPolicy(ILogger<ResilientTokenService> logger)
        {
            int timeoutInSeconds = 1;
            return Policy.TimeoutAsync<bool>(
                timeoutInSeconds,
                TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timespan, task, exception) =>
                {
                    logger.LogError($"###-TIMEOUT: execution timed out after {timespan.TotalSeconds} seconds, eventually terminated with: {exception}.");
                    return task;
                });
        }

        public async Task<bool> Validate(string token)
        {
            return await _policyWrap.ExecuteAsync(() => _userRepository.Validate(token));
        }
    }
}
