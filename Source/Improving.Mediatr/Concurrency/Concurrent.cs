namespace Improving.MediatR.Concurrency
{
    public class Concurrent : DTO, Request.WithResponse<ConcurrencyResult>
    {
        public object[] Requests { get; set; }
    }
}
