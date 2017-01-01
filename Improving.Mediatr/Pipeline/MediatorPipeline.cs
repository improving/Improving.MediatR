namespace Improving.MediatR.Pipeline
{
    using System;
    using System.Threading.Tasks;
    using Castle.DynamicProxy;
    using global::MediatR;

    /// <summary>
    /// Defines pre/post processing activities.
    /// </summary>
    /// <typeparam name="TRequest">Incoming request</typeparam>
    /// <typeparam name="TResponse">Outgoing response</typeparam>
    public interface IRequestMiddleware<TRequest, TResponse>
         where TRequest : IAsyncRequest<TResponse>
    {
        Task<TResponse> Apply(TRequest request, Func<TRequest, Task<TResponse>> next);
    }

    public abstract class RequestMiddleware<TRequest, TResponse>
        : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IAsyncRequest<TResponse>
    {
        public abstract Task<TResponse> Apply(TRequest request, Func<TRequest, Task<TResponse>> next);
    }

    /// <summary>
    /// Coordinates the execution of a request and associated middleware.
    /// </summary>
    /// <typeparam name="TRequest">The incoming request</typeparam>
    /// <typeparam name="TResponse">The outgoing resoinse</typeparam>
    public class MediatorPipeline<TRequest, TResponse>
        : IAsyncRequestHandler<TRequest, TResponse>
        where TRequest : IAsyncRequest<TResponse>
    {
        private readonly IAsyncRequestHandler<TRequest, TResponse> _inner;
        private readonly IRequestMiddleware<TRequest, TResponse>[] _middleware;

        public MediatorPipeline(IAsyncRequestHandler<TRequest, TResponse> inner,
                                IRequestMiddleware<TRequest, TResponse>[] middleware)
        {
            _inner      = inner;
            _middleware = middleware;
            Array.Sort(_middleware, RelativeOrderAttribute.Compare);
        }


        public Task<TResponse> Handle(TRequest request)
        {
            var index = -1;
            Func<TRequest, Task<TResponse>> next = null;
            next = req =>
            {
                ++index;
                return index < _middleware.Length
                     ? _middleware[index].Apply(request, next)
                     : _inner.Handle(request);
            };

            Env.Use(new PipelineContext(ProxyUtil.GetUnproxiedType(_inner)));

            return next(request);
        }
    }
}