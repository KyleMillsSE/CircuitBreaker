namespace CircuitBreaker.Integration
{
    public class HttpClientProxyConfiguration
    {
        public int FailureTrip { get; }
        public int FailureTripTimeoutInSeconds { get; }

        public HttpClientProxyConfiguration(int failureTrip, int failureTripTimeoutInSeconds)
        {
            FailureTrip = failureTrip;
            FailureTripTimeoutInSeconds = failureTripTimeoutInSeconds;
        }
    }
}
