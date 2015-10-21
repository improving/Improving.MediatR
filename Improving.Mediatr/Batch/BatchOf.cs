namespace Improving.MediatR.Batch
{
    using System.Collections.Generic;
    using System.Text;
    using global::MediatR;

    /// <summary>
    /// Represents a batch of requests.
    /// </summary>
    /// <typeparam name="TPayload">The payload type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class BatchOf<TPayload, TResponse> 
        : Request.WithResponse<BatchResponse<TResponse>>
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

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Batch of {0} {1}",
                _payloads.Count, typeof(TPayload));
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a batch of responses.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class BatchResponse<TResponse> : INotification, IAsyncNotification
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

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Batch response of {0} {1}",
                Batch.Length, typeof(TResponse));
            return builder.ToString();
        }
    }
}
