using System.Threading;
using MediatR;

namespace Improving.MediatR
{
    using System;
    using System.Threading.Tasks;

    public class MediatorDecorator : IMediator
    {
        private readonly IMediator _mediator;

        public MediatorDecorator(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException("mediator");
            _mediator = mediator;
        }

        protected IMediator Mediator
        {
            get { return _mediator; }
        }

        public virtual TResponse Send<TResponse>(IRequest<TResponse> request)
        {
            return _mediator.Send(request);
        }

        public virtual Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request)
        {
            return _mediator.SendAsync(request);
        }

        public virtual Task<TResponse> SendAsync<TResponse>(ICancellableAsyncRequest<TResponse> request, CancellationToken cancellationToken)
        {
            return _mediator.SendAsync(request, cancellationToken);
        }

        public virtual void Publish(INotification notification) 
        {
           _mediator.Publish(notification);
        }

        public virtual Task PublishAsync(IAsyncNotification notification)
        {
            return _mediator.PublishAsync(notification);
        }

        public virtual Task PublishAsync(ICancellableAsyncNotification notification, CancellationToken cancellationToken)
        {
            return _mediator.PublishAsync(notification, cancellationToken);
        }
    }
}
