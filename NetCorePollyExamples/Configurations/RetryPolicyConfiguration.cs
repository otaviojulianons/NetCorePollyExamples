namespace NetCorePollyExamples.Configurations
{
    public class RetryPolicyConfiguration
    {
        public int RetryCount { get; set; }

        public int RetryDelayMilliseconds { get; set; }

        public int TimeoutMilliseconds { get; set; }
    }
}
