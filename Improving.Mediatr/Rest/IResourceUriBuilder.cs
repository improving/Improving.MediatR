namespace Improving.MediatR.Rest
{
    public interface IResourceUriBuilder<in TResource>
    {
        string BuildResourceUri(TResource resource, string format);
    }
}
