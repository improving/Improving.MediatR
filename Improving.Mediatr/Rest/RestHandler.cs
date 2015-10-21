namespace Improving.MediatR.Rest
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;
    using Castle.Core;
    using Castle.Core.Logging;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Context;
    using Castle.MicroKernel.Handlers;
    using global::MediatR;

    public abstract class RestHandler<TRestRequest, TRestResponse, TContent, TResource>
        : IAsyncRequestHandler<TRestRequest, TRestResponse>,
          IRequireGenericMatching<RestHandlerGenericCloser>
        where TRestRequest : RestRequest<TContent, TRestResponse>
        where TResource : class
    {
        protected RestHandler()
        {
            Logger = NullLogger.Instance;
        }

        public Uri BaseAddress { get; set; }

        public IResourceUriBuilder<TContent> ResourceUriBuilder { get; set; }

        public ILogger Logger { get; set; }

        public abstract Task<TRestResponse> Handle(TRestRequest message);

        protected virtual HttpContent GetContent(TRestRequest request)
        {
            if (typeof(TContent) == typeof(string))
                return new StringContent(request.Resource as string);
            if (typeof(TResource) == typeof(Stream))
                return new StreamContent(request.Resource as Stream);
            return new ObjectContent<TContent>(request.Resource, new JsonMediaTypeFormatter());
        }

        protected async virtual Task<TResource> ExtractResource(HttpResponseMessage response)
        {
            if (typeof(TResource) == typeof(HttpResponseMessage))
                return response as TResource;
            response.EnsureSuccessStatusCode();
            if (typeof(TResource) == typeof(string))
                return (await response.Content.ReadAsStringAsync()) as TResource;
            if (typeof(TResource) == typeof(Stream))
                return (await response.Content.ReadAsStreamAsync()) as TResource;
            if (typeof(TResource) == typeof(byte[]))
                return (await response.Content.ReadAsByteArrayAsync()) as TResource;
            return await response.Content.ReadAsAsync<TResource>();
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
        where TResource : class
    {
    }

    class RestHandlerGenericCloser : IGenericImplementationMatchingStrategy
    {
        public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
        {
            var requestArgs = context.RequestedType.GetGenericArguments()[0];
            return requestArgs.GetGenericArguments();
        }
    }

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
}
