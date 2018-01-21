namespace Improving.MediatR.Rest.Post
{
    /// <summary>
    /// Represents the resource to get.
    /// </summary>
    /// <typeparam name="TPost">>Post content</typeparam>
    /// <typeparam name="TResource">Response content</typeparam>
    public class PostRequest<TPost, TResource>
        : RestRequest<TPost, PostResponse<TResource>>
    {
        public PostRequest()
        {    
        }

        public PostRequest(TPost resource) : base(resource)
        {
            Resource = resource;
        }
    }

    /// <summary>
    /// Represents the received resource.
    /// </summary>
    /// <typeparam name="TResource">Response content</typeparam>
    public class PostResponse<TResource> : DTO, IResource<TResource>
    {
        public PostResponse(TResource resource)
        {
            Resource = resource;
        }

        public string ResourceUri { get; set; }

        public TResource Resource { get; set; }
    }
}
