namespace Improving.MediatR.Route
{
    using System.Threading.Tasks;
    using global::MediatR;

    public interface IRouter
    {
        bool CanRoute(Routed route, object message);

        Task<object> Route(Routed route, object message, IMediator mediator);
    }
}
