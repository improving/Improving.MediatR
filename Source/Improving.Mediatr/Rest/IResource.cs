namespace Improving.MediatR.Rest
{
    /// <summary>
    /// Represents a rest resource.
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// The resource identifier.
        /// </summary>
        string ResourceUri { get; set; }
    }

    /// <summary>
    /// Represents a rest resource with content.
    /// </summary>
    /// <typeparam name="TContent">The content type</typeparam>
    public interface IResource<out TContent> : IResource
    {
        /// <summary>
        /// The resource content.
        /// </summary>
        TContent Resource { get; }
    }
}
