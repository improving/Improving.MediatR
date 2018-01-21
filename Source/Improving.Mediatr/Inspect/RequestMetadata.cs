namespace Improving.MediatR.Inspect
{
    using System;

    public class RequestMetadata
    {
        public Type RequestType  { get; set; }

        public Type ResponseType { get; set; }

        public Type HandlerType  { get; set; }
    }
}
