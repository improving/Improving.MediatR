using System;
using System.Security.Principal;
using System.Threading.Tasks;
using MediatR;
using Improving.MediatR.Pipeline;

namespace Improving.MediatR.Tests
{
    using System.Threading;

    [RelativeOrder(Stage.Validation - 1)]
    public class PingHandler :
        IAsyncRequestHandler<Ping, Pong>,
        IRequestMiddleware<Ping, Pong>
    {
        public IIdentity Identity { get; set; }

        public PingHistory History { get; set; }

        public Task<Pong> Apply(Ping ping, Func<Ping, Task<Pong>> next)
        {
            return next(ping);
        }

        public async Task<Pong> Handle(Ping ping)
        {
            if (ping.ThrowException != null)
                throw ping.ThrowException;

            if (History != null)
              History.Identity = Identity;

            if (ping.DelayMs.HasValue)
                await Task.Delay(ping.DelayMs.Value);

            return new Pong
            {
                Code    = GetHashCode(),
                History = History,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };
        }
    }
}