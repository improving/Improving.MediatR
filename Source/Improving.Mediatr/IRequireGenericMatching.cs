namespace Improving.MediatR
{
    using Castle.MicroKernel.Handlers;

    public interface IRequireGenericMatching<T>
        where T : IGenericImplementationMatchingStrategy, new()
    {
    }
}
