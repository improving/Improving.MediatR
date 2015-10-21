namespace Improving.MediatR.Rest.Put
{
    using System.Text;

    /// <summary>
    /// Represents the resource to get.
    /// </summary>
    /// <typeparam name="TPut">>Put content</typeparam>
    /// <typeparam name="TResource">Response content</typeparam>
    public class PutRequest<TPut, TResource>
        : RestRequest<TPut, PutResponse<TResource>>
    {
        public PutRequest(TPut resource) : base(resource)
        {
            Resource = resource;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine().AppendFormat("Put {0} at", typeof(TResource));
            if (ResourceUri != null)
                builder.AppendFormat(" {0}", ResourceUri);
            if (BaseAddress != null)
                builder.AppendFormat(" ({0})", BaseAddress);
            if (Resource != null)
                builder.AppendLine().Append(Resource);
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents the received resource.
    /// </summary>
    /// <typeparam name="TResource">Response content</typeparam>
    public class PutResponse<TResource> : IResource<TResource>
    {
        public PutResponse(TResource resource)
        {
            Resource = resource;
        }

        public string ResourceUri { get; set; }

        public TResource Resource { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine().AppendFormat("Put got {0}", typeof(TResource));
            if (ResourceUri != null)
                builder.AppendFormat(" at {0}", ResourceUri);
            if (Resource != null)
                builder.AppendLine().Append(Resource);
            return builder.ToString();
        }
    }
}
