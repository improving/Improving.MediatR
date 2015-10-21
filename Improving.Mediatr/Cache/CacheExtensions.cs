namespace Improving.MediatR.Cache
{
    using System;

    public static class CacheExtensions
    {
        public static Cached<TResponse> Cached<TResponse>(this Request.WithResponse<TResponse> request)
            where TResponse : class
        {
            return new Cached<TResponse>(request);
        }

        public static Cached<TResponse> Cached<TResponse>(
            this Request.WithResponse<TResponse> request, TimeSpan timeToLive)
            where TResponse : class
        {
            return new Cached<TResponse>(request)
            {
                TimeToLive = timeToLive
            };
        }
    }
}
