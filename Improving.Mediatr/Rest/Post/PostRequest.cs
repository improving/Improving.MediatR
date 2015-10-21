namespace Improving.MediatR.Rest.Post
{
    using System.Text;

    /// <summary>
    /// Represents the resource to get.
    /// </summary>
    /// <typeparam name="TPost">>Post content</typeparam>
    /// <typeparam name="TResource">Response content</typeparam>
    public class PostRequest<TPost, TResource>
        : RestRequest<TPost, PostResponse<TResource>>
    {
        public PostRequest(TPost resource) : base(resource)
        {
            Resource = resource;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine().AppendFormat("Post {0} at", typeof(TResource));
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
    public class PostResponse<TResource> : IResource<TResource>
    {
        public PostResponse(TResource resource)
        {
            Resource = resource;
        }

        public string ResourceUri { get; set; }

        public TResource Resource { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine().AppendFormat("Post got {0}", typeof(TResource));
            if (ResourceUri != null)
                builder.AppendFormat(" at {0}", ResourceUri);
            if (Resource != null)
                builder.AppendLine().Append(Resource);
            return builder.ToString();
        }
    }
}
