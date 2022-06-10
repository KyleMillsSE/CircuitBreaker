namespace CircuitBreaker.Integration
{
    public interface IHttpClientProxy
    {
        Task<HttpProxyResult> SendAsync(HttpRequestMessage httpRequestMsg);
    }
}
