namespace Improving.MediatR.Rest.Put
{
    /// <summary>
    /// Represents the resource to get.
    /// </summary>
    /// <typeparam name="TPut">>Put content</typeparam>
    /// <typeparam name="TResource">Response content</typeparam>
    public class PutRequest<TPut, TResource>
        : RestRequest<TPut, PutResponse<TResource>>
    {
        public PutRequest()
        {    
        }

        public PutRequest(TPut resource) : base(resource)
        {
            Resource = resource;
        }
    }

    /// <summary>
    /// Represents the received resource.
    /// </summary>
    /// <typeparam name="TResource">Response content</typeparam>
    public class PutResponse<TResource> : DTO, IResource<TResource>
    {
        public PutResponse()
        {    
        }

        public PutResponse(TResource resource)
        {
            Resource = resource;
        }

        public string ResourceUri { get; set; }

        public TResource Resource { get; set; }
    }
}
