using MediatR;

namespace Improving.MediatR
{
    public abstract class Request
    {
        /// <summary>
        /// Every request/response can be asynchronous.
        /// </summary>
        /// <typeparam name="TResponse">The response type</typeparam>
        public interface WithResponse<out TResponse>
            : IRequest<TResponse>, IAsyncRequest<TResponse>
        {
        }

        /// <summary>
        /// Every request can be asynchronous.
        /// </summary>
        public interface WithNoResponse : IRequest, IAsyncRequest
        {
        }
    }
}