namespace Improving.MediatR.Cache
{
    using System;
    using global::MediatR;

    public static class CacheExtensions
    {
        public static Cached<TResponse> Cached<TResponse>(
            this IAsyncRequest<TResponse> request)
        {
            return new Cached<TResponse>(request);
        }

        public static Cached<TResponse> Cached<TResponse>(
            this IAsyncRequest<TResponse> request, TimeSpan timeToLive)
        {
            return new Cached<TResponse>(request)
            {
                TimeToLive = timeToLive
            };
        }

        public static Cached<TResponse> InvalidateCache<TResponse>(
            this IAsyncRequest<TResponse> request)
        {
            return new Cached<TResponse>(request)
            {
                Invalidate = true
            };
        }
    }
}
