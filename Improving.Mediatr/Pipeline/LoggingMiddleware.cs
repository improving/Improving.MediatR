namespace Improving.MediatR.Pipeline
{
    using System;
    using System.Threading.Tasks;
    using Castle.Core.Logging;
    using global::MediatR;

    [RelativeOrder(Stage.Logging)]
    public class LoggingMiddleware<TRequest, TResponse>
            : RequestMiddleware<TRequest, TResponse>
            where TRequest : IAsyncRequest<TResponse>
    {
        public LoggingMiddleware()
        {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override async Task<TResponse> Apply(TRequest request,
            Func<TRequest, Task<TResponse>> next)
        {
            Logger.DebugFormat("Handling request {0}", request);
            try
            {
                var response = await next(request);
                Logger.DebugFormat("Handled request {0} with response {1}", request, response);
                return response;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(ex, "Failed to handle request {0}", request);
                throw;
            }
        }
    }
}