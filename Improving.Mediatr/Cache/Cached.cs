namespace Improving.MediatR.Cache
{
    using System;
    using global::MediatR;

    /// <summary>
    /// Represents a cached request.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class Cached<TResponse> : RequestDecorator<TResponse>
    {
        public Cached()
        {    
        }

        public Cached(IAsyncRequest<TResponse> request)
            : base(request)
        {
            TimeToLive = TimeSpan.FromDays(1);
        }

        public TimeSpan TimeToLive { get; set; }

        public bool     Invalidate { get; set; }
    }
}
