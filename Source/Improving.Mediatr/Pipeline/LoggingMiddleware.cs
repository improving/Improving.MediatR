namespace Improving.MediatR.Pipeline
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Castle.Core.Logging;
    using FluentValidation;
    using global::MediatR;

    [RelativeOrder(Stage.Logging)]
    public class LoggingMiddleware<TRequest, TResponse>
        : RequestMiddleware<TRequest, TResponse>
        where TRequest : IAsyncRequest<TResponse>
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public ILoggerFactory LoggerFactory { get; set; }

        public PipelineContext PipelineContext { get; set; }

        public override async Task<TResponse> Apply(TRequest request,
            Func<TRequest, Task<TResponse>> next)
        {
            var logger = GetEffectiveLogger();

            Stopwatch stopwatch = null;
            if (logger.IsDebugEnabled || logger.IsErrorEnabled)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }

            if (logger.IsDebugEnabled)
                logger.DebugFormat("Handling {0}", GetDescription(request));

            try
            {
                var response = await next(request);

                if (logger.IsDebugEnabled)
                {
                    stopwatch?.Stop();
                    logger.DebugFormat("Completed {0}{1} with {2}",
                        DTO.PrettyName(request.GetType()),
                        FormatDuration(stopwatch), GetDescription(response));
                }

                return response;
            }
            catch (Exception ex)
            {
                if (WarningExceptions.Any(wex => wex.IsInstanceOfType(ex)))
                {
                    if (logger.IsWarnEnabled)
                    {
                        stopwatch?.Stop();
                        logger.WarnFormat(ex, "Failed {0}{1}", GetDescription(request),
                            FormatDuration(stopwatch));
                    }
                }
                else if (logger.IsErrorEnabled)
                {
                    stopwatch?.Stop();
                    logger.ErrorFormat(ex, "Failed {0}{1}", GetDescription(request),
                        FormatDuration(stopwatch));
                }
                ex.Data[Stage.Logging] = true;
                throw;
            }
        }

        private ILogger GetEffectiveLogger()
        {
            return PipelineContext != null && LoggerFactory != null
                ? LoggerFactory.Create(PipelineContext.HandlerType)
                : Logger;
        }

        private static string GetDescription(object instance)
        {
            var dto = instance as DTO;
            return dto?.ToString() ?? DTO.ToString(instance);
        }

        private static string FormatDuration(Stopwatch stopwatch)
        {
            if (stopwatch == null)
                return string.Empty;
            var elapsed = stopwatch.Elapsed;
            var totalMillis = elapsed.TotalMilliseconds;
            if (totalMillis > 60000)
                return $" in {(totalMillis/60000):0.00} min";
            return totalMillis > 1000
                ? $" in {(totalMillis/1000):0.00} sec"
                : $" in {totalMillis:0.00} ms";
        }

        private static readonly Type[] WarningExceptions =
        {
            typeof(ArgumentException),
            typeof(InvalidOperationException),
            typeof(ValidationException)
        };
    }
}