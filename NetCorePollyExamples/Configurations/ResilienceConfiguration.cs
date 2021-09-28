namespace NetCorePollyExamples.Configurations
{
    public class ResilienceConfiguration
    {
        public RetryPolicyConfiguration Retry { get; set; }
        public CircuitBreakerPolicyConfiguration CircuitBreaker { get; set; }
    }
}
