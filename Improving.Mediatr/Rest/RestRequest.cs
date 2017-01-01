namespace Improving.MediatR.Rest
{
    using System;
    using SmartFormat;

    public abstract class RestRequest<TMethod, TResponse>
        : DTO, Request.WithResponse<TResponse>, IResource<TMethod>
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

        public TMethod Resource { get; set; }

        public bool TypeNameHandling { get; set; }

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

        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other?.GetType() != GetType())
                return false;

            var otherRestRequest = other as RestRequest<TMethod, TResponse>;
            return otherRestRequest != null
                   && Equals(BaseAddress, otherRestRequest.BaseAddress)
                   && Equals(ResourceUri, otherRestRequest.ResourceUri)
                   && Equals(Resource, otherRestRequest.Resource)
                   && TypeNameHandling == otherRestRequest.TypeNameHandling;
        }

        public override int GetHashCode()
        {
            return (Resource?.GetHashCode() ?? 0) * 31 ^
                   (BaseAddress?.GetHashCode() ?? 0) ^
                   (ResourceUri?.GetHashCode() ?? 0) ^
                   TypeNameHandling.GetHashCode();
        }
    }
}
