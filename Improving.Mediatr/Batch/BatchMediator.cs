namespace Improving.MediatR.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::MediatR;

    /// <summary>
    /// Batches requests for delivery.
    /// </summary>
    public class BatchMediator : MediatorDecorator
    {
        private readonly IDictionary<Type, object> _batches;

        public BatchMediator(IMediator mediator, int batchSize = 1)
            : base(mediator)
        {
            if (batchSize <= 0)
                throw new ArgumentException("BatchSize must be greater than zero.");

            BatchSize = batchSize;
            _batches  = new Dictionary<Type, object>();
        }

        public int BatchSize { get; }

        public BatchResponse<TResponse> Batch<TPayload, TResponse>(params TPayload[] payload)
            where TPayload : Request.WithResponse<TResponse>
            where TResponse : class
        {
            object batch;
            if (!_batches.TryGetValue(typeof (TPayload), out batch))
            {
                batch = new BatchOf<TPayload, TResponse>();
                _batches.Add(typeof(TPayload), batch);
            }
            var payloads = (BatchOf<TPayload, TResponse>)batch;
            if (payloads.AddPayload(payload) < BatchSize)
                return null;
            _batches.Remove(typeof (TPayload));
            return Send(payloads);
        }

        public Task<BatchResponse<TResponse>>
            BatchAsync<TPayload, TResponse>(params TPayload[] payload)
            where TPayload : Request.WithResponse<TResponse>
            where TResponse : class
        {
            object batch;
            if (!_batches.TryGetValue(typeof(TPayload), out batch))
            {
                batch = new BatchOf<TPayload, TResponse>();
                _batches.Add(typeof(TPayload), batch);
            }
            var payloads = (BatchOf<TPayload, TResponse>)batch;
            if (payloads.AddPayload(payload) >= BatchSize)
            {
                _batches.Remove(typeof (TPayload));
                return SendAsync(payloads);
            }
            return Task.FromResult(null as BatchResponse<TResponse>);
        }

        public BatchResponse<TResponse> Flush<TPayload, TResponse>()
            where TPayload : Request.WithResponse<TResponse>
            where TResponse : class
        {
            object batch;
            if (_batches.TryGetValue(typeof (TPayload), out batch))
            {
                _batches.Remove(typeof(TPayload));
                return Send(((BatchOf<TPayload, TResponse>)batch));
            }
            return null;
        }

        public Task<BatchResponse<TResponse>> FlushAsync<TPayload, TResponse>()
            where TPayload : Request.WithResponse<TResponse>
            where TResponse : class
        {
            object batch;
            if (_batches.TryGetValue(typeof(TPayload), out batch))
            {
                _batches.Remove(typeof(TPayload));
                return SendAsync(((BatchOf<TPayload, TResponse>)batch));
            }
            return Task.FromResult(null as BatchResponse<TResponse>);
        }
    }
}
