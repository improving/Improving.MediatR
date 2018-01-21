namespace Improving.MediatR
{
    using System;
    using global::MediatR;

    public interface IRequestDecorator
    {
        object Request { get; }
    }

    public interface IRequestDecorator<out TResponse> : IRequestDecorator
    {
        new IAsyncRequest<TResponse> Request { get; }
    }

    public abstract class RequestDecorator<TResponse>
        : DTO, IRequestDecorator<TResponse>, Request.WithResponse<TResponse>
    {
        protected RequestDecorator()
        {         
        }

        protected RequestDecorator(IAsyncRequest<TResponse> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Request = request;
        }

        public IAsyncRequest<TResponse> Request { get; set; }

        object IRequestDecorator.Request => Request;
    }
}
