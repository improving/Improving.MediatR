namespace Improving.MediatR.Pipeline
{
    using System;

    public class PipelineContext
    {
        public PipelineContext(Type handlerType)
        {
            HandlerType = handlerType;
        }

        public Type HandlerType { get; }
    }
}
