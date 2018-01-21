namespace Improving.MediatR
{
    public class KeysResult<TId> : DTO
    {
        public Key<TId>[] Keys { get; set; }
    }
}
