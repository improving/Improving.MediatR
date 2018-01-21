namespace Improving.MediatR.Rest
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Castle.Core.Logging;
    using Castle.MicroKernel;
    using global::MediatR;

    public abstract class RestHandler<TRestRequest, TRestResponse, TContent, TResource>
        : IAsyncRequestHandler<TRestRequest, TRestResponse>,
          IRequireGenericMatching<HandlerGenericCloser>
        where TRestRequest : RestRequest<TContent, TRestResponse>
    {
        public Uri BaseAddress { get; set; }

        public IResourceUriBuilder<TContent> ResourceUriBuilder { get; set; }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public abstract Task<TRestResponse> Handle(TRestRequest message);

        protected virtual HttpContent GetContent(TRestRequest request)
        {
            var stringContent = request.Resource as string;
            if (stringContent != null)
                return new StringContent(stringContent);
            var streamContent = request.Resource as Stream;
            if (streamContent != null)
                return new StreamContent(streamContent);
            var bytes = request.Resource as byte[];
            if (bytes != null)
                return new ByteArrayContent(bytes);
            var jsonFormatter = request.TypeNameHandling
                              ? RestFormatters.JsonTyped
                              : RestFormatters.Json;
            return new ObjectContent<TContent>(request.Resource, jsonFormatter);
        }

        protected virtual Task<TResource> ExtractResource(
             RestRequest<TContent, TRestResponse> request, HttpResponseMessage response)
        {
            if (typeof (TResource) == typeof (HttpResponseMessage))
                return Task.FromResult(response) as Task<TResource>;
            response.EnsureSuccessStatusCode();
            if (typeof(TResource) == typeof(string))
                return response.Content.ReadAsStringAsync() as Task<TResource>;
            if (typeof (TResource) == typeof (Stream))
                return response.Content.ReadAsStreamAsync() as Task<TResource>;
            if (typeof (TResource) == typeof (byte[]))
                return response.Content.ReadAsByteArrayAsync() as Task<TResource>;
            var jsonFormatter = request.TypeNameHandling
                   ? RestFormatters.JsonTyped
                   : RestFormatters.Json;
            return response.Content.ReadAsAsync<TResource>(new [] { jsonFormatter });
        }

        protected string GetResourceUri(TRestRequest request)
        {
            var resourceUri = request.ResourceUri;
            if (ResourceUriBuilder != null && resourceUri != null && request.Resource != null)
                resourceUri = ResourceUriBuilder.BuildResourceUri(request.Resource, resourceUri);
            if (resourceUri != null)
                resourceUri = Uri.EscapeUriString(resourceUri);
            return resourceUri;
        }

        protected void SetBaseAddress(HttpClient httpClient, TRestRequest request)
        {
            httpClient.BaseAddress = request.BaseAddress != null
                ? new Uri(request.BaseAddress)
                : BaseAddress;
        }
    }

    public abstract class RestHandler<TRestRequest, TRestResponse, TResource>
        : RestHandler<TRestRequest, TRestResponse, Unit, TResource>
        where TRestRequest : RestRequest<Unit, TRestResponse>
    {
    }

    #region RestHandlerFilter

    class RestHandlerFilter : IHandlersFilter
    {
        public bool HasOpinionAbout(Type service)
        {
            if (!service.IsGenericType)
                return false;

            var arguments = service.GetGenericArguments();
            return arguments.Length == 2 &&
                   typeof (IResource).IsAssignableFrom(arguments[0]);
        }

        public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
        {
            var requestType = service.GetGenericArguments()[0];
            foreach (var handler in
                from handler in handlers
                let  implementation = handler.ComponentModel.Implementation
                let  baseType       = implementation.BaseType
                where baseType != null &&  baseType.IsGenericType &&
                     baseType.GetGenericTypeDefinition() == typeof (RestHandler<,,,>)
                let implRequestType = baseType.GetGenericArguments()[0]
                where requestType.GetGenericTypeDefinition() ==
                    implRequestType.GetGenericTypeDefinition() select handler)
            {
                return new[] { handler };
            }
            return handlers;
        }
    }

    #endregion
}
