namespace Improving.MediatR.Concurrency
{
    public class Sequential : DTO, Request.WithResponse<ConcurrencyResult>
    {
        public object[] Requests { get; set; }
    }
}
