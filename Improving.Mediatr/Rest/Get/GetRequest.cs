namespace Improving.MediatR.Rest.Get
{
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
    }

    /// <summary>
    /// Represents the received resource.
    /// </summary>
    /// <typeparam name="TResource">Response content</typeparam>
    public class GetResponse<TResource> : DTO, IResource<TResource>
    {
        public GetResponse()
        {    
        }

        public GetResponse(TResource resource)
        {
            Resource = resource;
        }

        public string ResourceUri { get; set; }

        public TResource Resource { get; set; }
    }
}
