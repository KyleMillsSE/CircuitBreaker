using CircuitBreaker.Common;

namespace CircuitBreaker.Integration
{
    public class HttpClientProxy : IHttpClientProxy
    {

        private HttpProxyCircuitState _state;
        private int _failures;
        private long _failureTimeoutTicks;

        private readonly ILogger<HttpClientProxy> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClientProxyConfiguration _configuration;

        private readonly object _lock = new();

        public HttpClientProxy(ILogger<HttpClientProxy> logger, IHttpClientFactory httpClientFactory, HttpClientProxyConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _state = HttpProxyCircuitState.Closed;
            _failures = 0;
            _failureTimeoutTicks = 0;
        }

        public async Task<HttpProxyResult> SendAsync(HttpRequestMessage httpRequestMsg)
        {
            if (_state == HttpProxyCircuitState.Open)
            {
                if (DateTime.UtcNow.Ticks < _failureTimeoutTicks)
                {
                    _logger.LogWarning($"Circuit open cannot send request to '{httpRequestMsg.RequestUri}'.");

                    return HttpProxyResult.Failed();
                }

                using (DisposableLock.Lock(_lock))
                {
                    if (_state == HttpProxyCircuitState.Open)
                    {
                        _state = HttpProxyCircuitState.CloseTest;
                    }
                    else
                    {
                        _logger.LogWarning($"Circuit close test cannot send request to '{httpRequestMsg.RequestUri}'.");

                        return HttpProxyResult.Failed();
                    }
                }
            }

            var client = _httpClientFactory.CreateClient();

            client.Timeout = TimeSpan.FromSeconds(5);

            try
            {
                var httpResponseMsg = await client.SendAsync(httpRequestMsg);

                using (DisposableLock.Lock(_lock))
                {
                    if (_state == HttpProxyCircuitState.CloseTest)
                    {
                        _state = HttpProxyCircuitState.Closed;
                        _failures = 0;
                    }
                    else if (_state == HttpProxyCircuitState.Closed)
                    {
                        _failures = 0;
                    }
                }

                return HttpProxyResult.Success(httpResponseMsg);
            }
            catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
            {
                _logger.LogError(ex, $"Error occurred sending a request to '{httpRequestMsg.RequestUri}'.");

                using (DisposableLock.Lock(_lock))
                {
                    _failures++;

                    if (_failures >= _configuration.FailureTrip)
                    {
                        _state = HttpProxyCircuitState.Open;
                        _failureTimeoutTicks = DateTime.UtcNow.AddSeconds(_configuration.FailureTripTimeoutInSeconds).Ticks;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred sending a request to '{httpRequestMsg.RequestUri}'.");
            }

            return HttpProxyResult.Failed();
        }
    }
}
