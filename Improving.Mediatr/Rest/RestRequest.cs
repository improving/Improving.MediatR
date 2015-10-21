namespace Improving.MediatR.Rest
{
    using System;
    using SmartFormat;

    public abstract class RestRequest<TMethod, TResponse>
        : Request.WithResponse<TResponse>, IResource<TMethod>
    {
        protected RestRequest()
        {
        }

        protected RestRequest(TMethod resource)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));
            Resource = resource;
        }

        public string BaseAddress { get; set; }

        public string ResourceUri { get; set; }

        public TMethod Resource { get; protected set; }

        public string SetResourceUri(string resourceUriformat, params object[] args)
        {
            if (resourceUriformat == null)
                throw new ArgumentNullException(nameof(resourceUriformat));
            if (args.Length == 0)
            {
                return ResourceUri = (Resource != null)
                     ? Smart.Format(resourceUriformat, Resource)
                     : resourceUriformat;
            }
            return ResourceUri = Smart.Format(resourceUriformat, args);
        }
    }
}
