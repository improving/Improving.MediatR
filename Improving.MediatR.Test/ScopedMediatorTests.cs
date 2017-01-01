using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using MediatR;

namespace Improving.MediatR.Tests
{
    [TestClass]
    public class ScopedMediatorTests : LoggingTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            InitializeContainer();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _container.Dispose();
        }

        [TestMethod]
        public void Should_Resolve_Mediator()
        {
            var mediator = _container.Resolve<IMediator>();
            Assert.AreSame(_mediator, mediator);
        }

        [TestMethod]
        public void Should_Resolve_Scoping_Mediator()
        {
            Assert.IsInstanceOfType(_mediator, typeof(ScopedMediator));
        }

        [TestMethod]
        public async Task Should_Begin_Scope_For_Each_Call()
        {
            var pong = await _mediator.SendAsync(new Ping());
            Assert.IsNotNull(pong);
        }

        [TestMethod]
        public void Should_Promote_To_Async_When_Possible()
        {
            var ping = new Ping();
            var pong = _mediator.Send(ping);
            Assert.IsNotNull(pong);
            var events = _MemoryTarget.Logs;
            Assert.IsTrue(events.Any(x => Regex.Match(x, "DEBUG").Success &&
                Regex.Match(x, $"Handling {ping}").Success));
        }

        [TestMethod]
        public void Should_Override_Handler()
        {
            var pong = _mediator
                .Use(new FastPingHandler())
                .Send(new Ping());
            Assert.IsNotNull(pong);
        }

        [TestMethod]
        public void Should_Handle_Contravariantly()
        {
            var pong = _mediator
                .Send(new FastPing());
            Assert.IsNotNull(pong);
        }

        [TestMethod]
        public async Task Should_Propgate_Environment_Info()
        {
            var history = new PingHistory();
            var pong = await _mediator.Use(history).SendAsync(new Ping());
            Assert.IsNotNull(pong);
            Assert.AreSame(history, pong.History);
            Assert.AreEqual("FooBar", history.Identity.Name);

            Console.WriteLine(pong);
        }

        [TestMethod]
        public async Task Should_Clear_Environment_Info()
        {
            using (_mediator.NewScope())
            {
                await _mediator.Use(new PingHistory()).SendAsync(new Ping());
                var pong = await _mediator.Use(null as PingHistory).SendAsync(new Ping());
                Assert.IsNotNull(pong);
                Assert.IsNull(pong.History);
            }
        }

        [TestMethod]
        public void Should_Dispose_Implied_Environment_Scopes()
        {
            _mediator.Use(new PingHistory());
            GC.Collect();
        }

        [TestMethod]
        public async Task Should_Use_New_Scopes()
        {
            var pong1 = await _mediator.SendAsync(new Ping());
            using (_mediator.NewScope())
            {
                var pong2 = await _mediator.SendAsync(new Ping());
                Assert.AreNotEqual(pong1.Code, pong2.Code);
                var pong3 = await _mediator.SendAsync(new Ping());
                Assert.AreEqual(pong2.Code, pong3.Code);
                using (_mediator.NewScope())
                {
                    var pong4 = await _mediator.SendAsync(new Ping());
                    Assert.AreNotEqual(pong2.Code, pong4.Code);
                }
            }
            var pong5 = await _mediator.SendAsync(new Ping());
            Assert.AreNotEqual(pong1.Code, pong5.Code);
        }

        public class FastPing : Ping
        {

        }

        class FastPingHandler : IAsyncRequestHandler<Ping, Pong>
        {
            public Task<Pong> Handle(Ping message)
            {
                return Task.FromResult(new Pong { Code = 1 });
            }
        }
    }
}
