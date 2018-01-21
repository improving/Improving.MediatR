namespace Improving.MediatR.Route
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::MediatR;

    public class RouteHandler<TResponse>
         : IAsyncRequestHandler<Routed<TResponse>, TResponse>,
           IRequireGenericMatching<HandlerGenericCloser>
    {
        private readonly IRouter[] _routers;
        private readonly IMediator _mediator;

        public RouteHandler(IRouter[] routers, IMediator mediator)
        {
            _routers  = routers;
            _mediator = mediator;
        }

        public async Task<TResponse> Handle(Routed<TResponse> message)
        {
            var route   = message.Route;
            var request = message.Request;

            var router = _routers.FirstOrDefault(r => r.CanRoute(message, request));
            if (router == null)
                throw new NotSupportedException($"Unrecognized request route '{route}'");

            var response = await router.Route(message, request, _mediator);
            return (TResponse) response;
        }
    }

    public class RouteNotificationHandler 
         : IAsyncNotificationHandler<RoutedNotification>
    {
        private readonly IRouter[] _routers;
        private readonly IMediator _mediator;

        public RouteNotificationHandler(IRouter[] routers, IMediator mediator)
        {
            _routers  = routers;
            _mediator = mediator;
        }

        public async Task Handle(RoutedNotification notification)
        {
            var route = notification.Route;
            var notif = notification.Notification;

            var router = _routers.FirstOrDefault(r => r.CanRoute(notification, notif));
            if (router == null)
                throw new NotSupportedException($"Unrecognized notification route '{route}'");

            await router.Route(notification, notif, _mediator);
        }
    }
}
