namespace Improving.MediatR.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::MediatR;

    public class BatchHandler<TPayload, TResponse>
        : IAsyncRequestHandler<BatchOf<TPayload, TResponse>, BatchResponse<TResponse>>,
          IRequireGenericMatching<HandlerGenericCloser>
        where TPayload  : Request.WithResponse<TResponse>
        where TResponse : class
    {
        private readonly IMediator _mediator;

        public BatchHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<BatchResponse<TResponse>> Handle(BatchOf<TPayload, TResponse> batchOf)
        {
            return HandleBatch(batchOf, payload => _mediator.SendAsync(payload));
        }

        protected virtual async Task<BatchResponse<TResponse>> HandleBatch(
            BatchOf<TPayload, TResponse> batchOf,
            Func<TPayload, Task<TResponse>> sendPayload)
        {
            var responses = new List<TResponse>();
            if (batchOf.Batch != null)
            {
                foreach (var payload in batchOf.Batch)
                {
                    var response = await sendPayload(payload);
                    if (response != null)
                        responses.Add(response);
                }
            }
            return new BatchResponse<TResponse>(responses.ToArray());
        }
    }
}
