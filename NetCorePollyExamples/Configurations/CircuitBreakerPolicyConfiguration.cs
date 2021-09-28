namespace NetCorePollyExamples.Configurations
{
    public class CircuitBreakerPolicyConfiguration
    {
        public bool Enabled { get; set; }
        public BasicCircuitBreakerPolicyConfiguration Basic { get; set; }
        public AdvancedCircuitBreakerPolicyConfiguration Advanced { get; set; }
    }

    public class BasicCircuitBreakerPolicyConfiguration
    {
        public int NumberOfExceptionAllowedBeforeOpenCircuit { get; set; }
        public int DurationOfBreakInSeconds { get; set; }
    }

    public class AdvancedCircuitBreakerPolicyConfiguration
    {
        public double FailureThreshold { get; set; }
        public int SamplingDurationInSeconds { get; set; }
        public int MinimumThroughput { get; set; } = 2; //Valor minimo aceitavel pelo framework
        public int DurationOfBreakInSeconds { get; set; }
    }
}
