using Akka.Actor;
using Akka.Routing;
using CircuitBreaker.Actors;

namespace CircuitBreaker.BackgroundTasks
{
    public class ApiPollingBackgroundTask : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ActorSystem _actorSystem;

        public ApiPollingBackgroundTask(IServiceProvider serviceProvider, ActorSystem actorSystem)
        {
            _serviceProvider = serviceProvider;
            _actorSystem = actorSystem;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pollingActorRouter = _actorSystem.ActorOf(PollingActor.Props(_serviceProvider).WithRouter(new BroadcastPool(1)));

            pollingActorRouter.Tell(new PollingActor.StartPolling());

            return Task.CompletedTask;
        }
    }
}
