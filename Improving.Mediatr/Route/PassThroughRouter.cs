namespace Improving.MediatR.Route
{
    using System.Threading.Tasks;
    using global::MediatR;

    public class PassThroughRouter : IRouter
    {
        public bool CanRoute(Routed route, object message)
        {
            return route.Route == "pass-through";
        }

        public Task<object> Route(Routed route, object message, IMediator mediator)
        {
            return DynamicDispatch.Dispatch(mediator, message);
        }
    }
}
