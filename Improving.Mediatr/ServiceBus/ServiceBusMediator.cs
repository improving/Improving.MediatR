namespace Improving.MediatR.ServiceBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MediatR;
    using Route;

    /// <summary>
    /// Manages remote IMediator operations.
    /// </summary>
    public class ServiceBusMediator : IMediator
    {
        private readonly IMediator _mediator;
        private readonly string _baseAddress;

        public ServiceBusMediator(IMediator mediator, string baseAddress)
        {
            _mediator    = mediator;
            _baseAddress = baseAddress;
        }

        public TResponse Send<TResponse>(IRequest<TResponse> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var asyncRequest = request as IAsyncRequest<TResponse>;
            if (asyncRequest == null)
                throw new NotSupportedException("Only async requests are supported");

            return SendAsync(asyncRequest).Result;    
        }

        public Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return _mediator.SendAsync(request.RouteTo(_baseAddress));
        }

        public void Publish(INotification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            var asyncNotification = notification as IAsyncNotification;
            if (asyncNotification == null)
                throw new NotSupportedException("Only async notifications are supported");

            PublishAsync(asyncNotification).Wait();
        }

        public Task PublishAsync(IAsyncNotification notification)
        {
            return _mediator.PublishAsync(notification.RouteTo(_baseAddress));
        }

        #region Not Supported

        public Task<TResponse> SendAsync<TResponse>(
            ICancellableAsyncRequest<TResponse> request, 
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Cancellation not supported yet");
        }

        public Task PublishAsync(
            ICancellableAsyncNotification notification,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Cancellation not supported yet");
        }

        #endregion
    }
}
