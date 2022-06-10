using Akka.Actor;
using CircuitBreaker.Integration;

namespace CircuitBreaker.Actors
{
    public class PollingActor : ReceiveActor
    {
        #region Messages

        public class StartPolling
        { }

        #endregion

        public static Props Props(IServiceProvider serviceProvider) => Akka.Actor.Props.Create<PollingActor>(() => new PollingActor(serviceProvider));

        private readonly IServiceProvider _serviceProvider;

        public PollingActor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            Receives();
        }

        private void Receives()
        {
            Receive<StartPolling>(_ =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5254/weatherforecast");

                var httpClientProxy = _serviceProvider.GetService<IHttpClientProxy>();

                httpClientProxy.SendAsync(request).PipeTo(Self);
            });

            Receive<HttpProxyResult>(res =>
            {
                var self = Self;
                Task.Delay(2000).ContinueWith(x =>
                {
                    self.Tell(new StartPolling());

                }, TaskContinuationOptions.ExecuteSynchronously);
            });
        }
    }

}
