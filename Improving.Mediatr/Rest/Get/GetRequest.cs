namespace Improving.MediatR.Rest.Get
{
    using System.Text;
    using global::MediatR;

    /// <summary>
    /// Represents the resource to get.
    /// </summary>
    /// <typeparam name="TGet">Get content</typeparam>
    /// <typeparam name="TResource">Response content</typeparam>
    public class GetRequest<TGet, TResource>
        : RestRequest<TGet, GetResponse<TResource>>
    {
        public GetRequest()
        {
        }

        public GetRequest(TGet resource) : base(resource)
        {
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine().AppendFormat("Get {0} at", typeof(TResource));
            if (ResourceUri != null)
                builder.AppendFormat(" {0}", ResourceUri);
            if (BaseAddress != null)
                builder.AppendFormat(" ({0})", BaseAddress);
            if (Resource != null)
                builder.AppendLine().Append(Resource);
            return builder.ToString();
        }
    }

    public class GetRequest<TReponse> : GetRequest<Unit, TReponse>
    {
    }

    /// <summary>
    /// Represents the received resource.
    /// </summary>
    /// <typeparam name="TResource">Response content</typeparam>
    public class GetResponse<TResource> : IResource<TResource>
    {
        public GetResponse(TResource resource)
        {
            Resource = resource;
        }

        public string ResourceUri { get; set; }

        public TResource Resource { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine().AppendFormat("Get got {0}", typeof(TResource));
            if (ResourceUri != null)
                builder.AppendFormat(" at {0}", ResourceUri);
            if (Resource != null)
                builder.AppendLine().Append(Resource);
            return builder.ToString();
        }
    }
}
