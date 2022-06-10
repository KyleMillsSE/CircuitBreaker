namespace CircuitBreaker.Integration
{
    public class HttpProxyResult
    {
        public bool Succeeded { get; private set; }

        private readonly HttpResponseMessage _httpResponseMsg;
        public HttpResponseMessage HttpResponseMsg
        {
            get
            {
                if (!Succeeded)
                {
                    throw new InvalidOperationException("result is not successful cannot return response");
                }
                return _httpResponseMsg;
            }
            private init => _httpResponseMsg = value;
        }

        private HttpProxyResult(bool succeeded, HttpResponseMessage httpResponseMsg = null)
        {
            Succeeded = succeeded;
            HttpResponseMsg = httpResponseMsg;
        }

        public static HttpProxyResult Success(HttpResponseMessage httpResponseMsg) => new(true, httpResponseMsg);
        public static HttpProxyResult Failed() => new(false);
    }
}
