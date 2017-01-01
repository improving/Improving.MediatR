namespace Improving.MediatR.ServiceBus
{
    using global::MediatR;

    public static class ServiceBusExtensions
    {
        public static ServiceBusMediator At(
            this IMediator mediator, string baseAddress)
        {
            return new ServiceBusMediator(mediator, baseAddress);
        }
    }
}
