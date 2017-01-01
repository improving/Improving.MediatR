namespace Improving.MediatR.ServiceBus
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Castle.Core.Logging;
    using FluentValidation;
    using FluentValidation.Results;
    using global::MediatR;
    using Rest;
    using Route;

    public class ServiceBusRouter : IRouter
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public bool CanRoute(Routed route, object message)
        {
            Uri uri;
            return route != null && Uri.TryCreate(route.Route, UriKind.Absolute, out uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public async Task<object> Route(Routed route, object message, IMediator mediator)
        {
            var response = await mediator
                .PostAsync<Message, HttpResponseMessage>(
                new Message(message), post =>
                {
                    post.BaseAddress      = route.Route;
                    post.ResourceUri      = GetResourceUri(route, message);
                    post.TypeNameHandling = true;
                });

            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content
                    .ReadAsAsync<Message>(RestFormatters.TypedJsonList);
                return payload.Payload;
            }

            if (response.StatusCode == HttpStatusCodeExtensions.UnprocessableEntity &&
                response.ReasonPhrase == "Validation")
            {
                var errors = (await response.Content
                    .ReadAsAsync<ValidationFailureShim[]>(RestFormatters.JsonList))
                    .Select(e => (ValidationFailure)e);
                throw new ValidationException(errors);
            }

            if (response.ReasonPhrase == "HttpError")
            {
                var error = await response.Content.ReadAsAsync<HttpError>();
                LogExceptionDetails(error);

                object exceptionType;
                if (error.TryGetValue("ExceptionType", out exceptionType) &&
                    Equals(exceptionType, typeof(InvalidOperationException).FullName))
                    throw new InvalidOperationException(error.ExceptionMessage ?? error.Message);

                throw new Exception(error.ExceptionMessage ?? error.Message);
            }

            response.EnsureSuccessStatusCode();
            return null;
        }

        public static string GetRequestPath(Type requestType)
        {
            if (requestType.IsGenericTypeDefinition)
                return null;
            var path = requestType.FullName;
            if (typeof(IRequestDecorator).IsAssignableFrom(requestType))
            {
                var name  = requestType.Name;
                var index = name.IndexOf('`');
                path = index < 0 ? name : name.Substring(0, index);
            }
            else if (requestType.IsGenericType ||
                    (!typeof(IAsyncNotification).IsAssignableFrom(requestType) &&
                     requestType.GetInterface(typeof(IAsyncRequest<>).FullName) == null))
            {
                return null;
            }
            var parts = path.Split('.');
            return string.Join("/", parts.Select(part =>
                char.ToLower(part[0]) + part.Substring(1))
                .ToArray());
        }

        public static string GetRequestPath(object request)
        {
            var decorators = "";
            while (request is IRequestDecorator)
            {
                var decorator = GetRequestPath(request.GetType());
                decorators = $"{decorators}/{decorator}";
                request = ((IRequestDecorator)request).Request;
            }
            var basePath = GetRequestPath(request.GetType());
            return basePath == null ? null : $"{basePath}{decorators}";
        }

        private void LogExceptionDetails(HttpError error)
        {
            if (Logger.IsDebugEnabled)
            {
                object detail;
                if (error.TryGetValue("ExceptionType", out detail))
                    Logger.DebugFormat("Remote Exception Type: {0}", detail);

                Logger.Debug(error.ExceptionMessage ?? error.Message);

                if (error.TryGetValue("StackTrace", out detail))
                    Logger.DebugFormat("Remote Stack Trace: {0}", detail);
            }
        }

        private static string GetResourceUri(Routed route, object message)
        {
            if (!string.IsNullOrEmpty(route.Tag))
                return $"tag/{GetApplicationName()}/{route.Tag}";
            var prefix = message is IAsyncNotification ? "publish" : "process";
            var path   = GetRequestPath(message);
            return path != null ? $"{prefix}/{path}" : prefix;
        }

        private static string GetApplicationName()
        {
            var entry = Assembly.GetEntryAssembly();
            return entry != null
                 ? entry.GetName().Name
                 : AppDomain.CurrentDomain.FriendlyName;
        }
    }
}
