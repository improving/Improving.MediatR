using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Improving.MediatR.Pipeline;

namespace Improving.MediatR.Tests
{
    [RelativeOrder(Stage.Authorization)]
    public class PingAuthorization : IRequestMiddleware<Ping, Pong>
    {
        public Task<Pong> Apply(Ping request, Func<Ping, Task<Pong>> next)
        {
            Env.Use(new GenericIdentity("FooBar"));
            return next(request);
        }
    }
}
