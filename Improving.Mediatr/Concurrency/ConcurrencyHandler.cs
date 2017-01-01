namespace Improving.MediatR.Concurrency
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::MediatR;

    public class ConcurrencyHandler
        : IAsyncRequestHandler<Concurrent, ConcurrencyResult>,
          IAsyncRequestHandler<Sequential, ConcurrencyResult>
    {
        private readonly IMediator _mediator;

        public ConcurrencyHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ConcurrencyResult> Handle(Concurrent request)
        {
            var responses = request.Requests?.Length > 0
                ? await Task.WhenAll(request.Requests.Select(Process))
                : new object[0];
            return new ConcurrencyResult
            {
                Responses = responses
            };
        }

        public async Task<ConcurrencyResult> Handle(Sequential request)
        {
            var responses = new List<object>();
            if (request.Requests?.Length > 0)
            {
                foreach (var req in request.Requests)
                    responses.Add(await Process(req));
            }
            return new ConcurrencyResult
            {
                Responses = responses.ToArray()
            };
        }

        private Task<object> Process(object request)
        {
            return DynamicDispatch.Dispatch(_mediator, request);
        }
    }
}
