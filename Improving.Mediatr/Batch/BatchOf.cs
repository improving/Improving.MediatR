namespace Improving.MediatR.Batch
{
    using System.Collections.Generic;
    using global::MediatR;

    /// <summary>
    /// Represents a batch of requests.
    /// </summary>
    /// <typeparam name="TPayload">The payload type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class BatchOf<TPayload, TResponse> 
        : DTO, Request.WithResponse<BatchResponse<TResponse>>
        where TPayload : Request.WithResponse<TResponse>
        where TResponse : class
    {
        private readonly List<TPayload> _payloads = new List<TPayload>();
 
        public BatchOf()
        {
        }

        public BatchOf(params TPayload[] batch)
        {
            Batch = batch;
        }

        public int PayloadCount => _payloads.Count;

        public TPayload[] Batch
        {
            get { return _payloads.ToArray(); }
            set
            {
                _payloads.Clear();
                if (value != null)
                    _payloads.AddRange(value);
            }
        }

        public int AddPayload(params TPayload[] payload)
        {
            _payloads.AddRange(payload);
            return _payloads.Count;
        }
    }

    /// <summary>
    /// Represents a batch of responses.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class BatchResponse<TResponse> : DTO, INotification, IAsyncNotification
        where TResponse : class
    {
        private TResponse[] _responses;

        private static readonly TResponse[] Empty = new TResponse[0];

        public BatchResponse()
        {
        }

        public BatchResponse(TResponse[] batch)
        {
            Batch = batch;
        }

        public TResponse[] Batch
        {
            get{ return _responses ?? Empty; }
            set { _responses = value; }
        }
    }
}
