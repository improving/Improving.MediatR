namespace Improving.MediatR.Route
{
    using global::MediatR;

    public static class RouteExtensions
    {
        public static Routed<TResponse> RouteTo<TResponse>(
            this IAsyncRequest<TResponse> request, string route, string tag = null)
        {
            return new Routed<TResponse>(request)
            {
                Route = route,
                Tag   = tag
            };
        }

        public static RoutedNotification RouteTo(
            this IAsyncNotification notification, string route, string tag = null)
        {
            return new RoutedNotification(notification)
            {
                Route = route,
                Tag   = tag
            };
        }
    }
}