namespace Improving.MediatR.Rest.Delete
{
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
    }

    /// <summary>
    /// Represents the received resource.
    /// </summary>
    /// <typeparam name="TResource">Response content</typeparam>
    public class DeleteResponse<TResource> : DTO, IResource<TResource>
    {
        public DeleteResponse()
        {    
        }

        public DeleteResponse(TResource resource)
        {
            Resource = resource;
        }

        public string ResourceUri { get; set; }

        public TResource Resource { get; set; }
    }
}
