namespace Improving.MediatR
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Threading.Tasks;
    using global::MediatR;

    public static class DynamicDispatch
    {
        private static readonly MethodInfo DispatchMethod =
             typeof(DynamicDispatch).GetMethod("DispatchInternal",
                BindingFlags.Static | BindingFlags.NonPublic);

        private delegate Task<object> DispatchDelegate(IMediator mediator, object request);

        private static readonly ConcurrentDictionary<Type, DispatchDelegate> Cache
            = new ConcurrentDictionary<Type, DispatchDelegate>();

        public static Task<object> Dispatch(IMediator mediator, object request, 
            Func<object, object> invalid = null)
        {
            var responseType = InferResponseType(request);
            if (responseType == null)
            {
                if (invalid != null)
                    return Task.FromResult(invalid(request));
                throw new NotSupportedException("Request is not an IAsyncRequest<>");
            }

            var dispatch = Cache.GetOrAdd(responseType, type =>
                (DispatchDelegate)Delegate.CreateDelegate(typeof(DispatchDelegate),
                    DispatchMethod.MakeGenericMethod(type)));

            return dispatch(mediator, request);
        }

        private static async Task<object> DispatchInternal<TResponse>(
            IMediator mediator, object request)
        {
            return await mediator.SendAsync((IAsyncRequest<TResponse>)request);
        }

        private static Type InferResponseType(object request)
        {
            return request?.GetType()
                .GetInterface(typeof(IAsyncRequest<>).FullName)
                ?.GetGenericArguments()[0];
        }
    }
}
