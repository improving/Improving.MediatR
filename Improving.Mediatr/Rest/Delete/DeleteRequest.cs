namespace Improving.MediatR.Rest.Delete
{
    using System.Text;

    /// <summary>
    /// Represents the resource to get.
    /// </summary>
    /// <typeparam name="TDelete">Delete content</typeparam>
    /// <typeparam name="TResource">Response content</typeparam>
    public class DeleteRequest<TDelete, TResource>
        : RestRequest<TDelete, DeleteResponse<TResource>>
    {
        public DeleteRequest()
        {
        }

        public DeleteRequest(TDelete resource) : base(resource)
        {
            Resource = resource;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine().AppendFormat("Delete {0} at", typeof(TResource));
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
    public class DeleteResponse<TResource> : IResource<TResource>
    {
        public DeleteResponse(TResource resource)
        {
            Resource = resource;
        }

        public string ResourceUri { get; set; }

        public TResource Resource { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine().AppendFormat("Delete got {0}", typeof(TResource));
            if (ResourceUri != null)
                builder.AppendFormat(" at {0}", ResourceUri);
            if (Resource != null)
                builder.AppendLine().Append(Resource);
            return builder.ToString();
        }
    }
}
