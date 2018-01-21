namespace Improving.MediatR
{
    using System;
    using global::MediatR;

    public interface INotificationDecorator : IAsyncNotification
    {
        IAsyncNotification Notification { get; }
    }

    public abstract class NotificationDecorator : INotificationDecorator
    {
        protected NotificationDecorator()
        {   
        }

        protected NotificationDecorator(IAsyncNotification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            Notification = notification;
        }

        public IAsyncNotification Notification { get; set; }
    }
}
