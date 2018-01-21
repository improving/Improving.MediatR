namespace Improving.MediatR
{
    using System;
    using global::MediatR;

    public static class ScopeExtensions
    {
        public static IDisposable NewScope(this IMediator mediator)
        {
            var scopedMediator = mediator as ScopedMediator;
            if (scopedMediator == null)
                throw new InvalidOperationException("The mediator does not support scoping");
            return scopedMediator.StartScope();
        }
    }
}
