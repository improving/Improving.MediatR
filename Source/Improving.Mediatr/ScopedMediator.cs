using System.Threading;

namespace Improving.MediatR
{
    using System;
    using System.Threading.Tasks;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Lifestyle;
    using Environment;
    using global::MediatR;

    /// <summary>
    /// Manages the lifestyle scopes for IMediator operations.
    /// </summary>
    public class ScopedMediator : IMediator
    {
        private readonly IKernel _kernel;
        private readonly IMediator _mediator;

        public ScopedMediator(IKernel kernel)
        {
            _kernel   = kernel;
            _mediator = new Mediator(
                t => _kernel.Resolve(t), 
                t => (object[])_kernel.ResolveAll(t)
                );
        }

        public TResponse Send<TResponse>(IRequest<TResponse> request)
        {
            using (RequiredScope())
            {
                var asyncRequest = request as IAsyncRequest<TResponse>;
                return asyncRequest != null
                    ? _mediator.SendAsync(asyncRequest).Result
                    : _mediator.Send(request);
            }
        }

        public async Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request)
        {
            using (RequiredScope())
                return await _mediator.SendAsync(request);
        }

        public async Task<TResponse> SendAsync<TResponse>(ICancellableAsyncRequest<TResponse> request, CancellationToken cancellationToken)
        {
            using (RequiredScope())
                return await _mediator.SendAsync(request, cancellationToken);
        }

        public void Publish(INotification notification)
        {
            using (RequiredScope())
                _mediator.Publish(notification);
        }

        public async Task PublishAsync(IAsyncNotification notification)
        {
            using (RequiredScope())
                await _mediator.PublishAsync(notification);
        }

        public async Task PublishAsync(ICancellableAsyncNotification notification, CancellationToken cancellationToken)
        {
            using (RequiredScope())
                await _mediator.PublishAsync(notification, cancellationToken);
        }

        protected internal IDisposable StartScope()
        {
            return _kernel.BeginScope();
        }

        private IDisposable RequiredScope()
        {
            var scope    = _kernel.RequireScope();
            var envScope = new EnvironmentScope();
            return new DisposableAction(() =>
            {
                envScope.Dispose();
                scope?.Dispose();
            });
        }
    }
}
