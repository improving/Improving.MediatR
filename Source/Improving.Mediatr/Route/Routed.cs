namespace Improving.MediatR.Route
{
    using System;
    using global::MediatR;

    public abstract class Routed : DTO
    {
        public string Route { get; set; }

        public string Tag   { get; set; }
    }

    public class Routed<TResponse> : Routed, 
        IAsyncRequest<TResponse>, IRequestDecorator<TResponse>
    {
        public Routed()
        {
        }

        public Routed(IAsyncRequest<TResponse> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Request = request;
        }

        public IAsyncRequest<TResponse> Request { get; set; }

        object IRequestDecorator.Request => Request;
    }

    public class RoutedNotification : Routed, INotificationDecorator
    {
        public RoutedNotification()
        {
        }

        public RoutedNotification(IAsyncNotification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            Notification = notification;
        }

        public IAsyncNotification Notification { get; set; }
    }
}