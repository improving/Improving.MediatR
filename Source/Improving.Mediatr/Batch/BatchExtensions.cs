namespace Improving.MediatR.Batch
{
    using global::MediatR;

    public static class BatchExtensions
    {
        public static BatchMediator Batch(this IMediator mediator, int batchSize = 1)
        {
            return new BatchMediator(mediator, batchSize);
        }
    }
}
