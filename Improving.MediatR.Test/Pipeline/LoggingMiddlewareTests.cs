using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Improving.MediatR.Tests.Pipeline
{
    [TestClass]
    public class LoggingMiddlewareTests : LoggingTestBase
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
        public async Task Should_Log_Request_And_Response()
        {
            var ping = new Ping();
            var pong = await _mediator.SendAsync(ping);
            Assert.IsNotNull(pong);

            var events = _MemoryTarget.Logs;
            Assert.IsTrue(events.Any(x => Regex.Match(x,
                $"DEBUG.*Improving.MediatR.Tests.PingHandler.*Handling {ping}").Success));
            Assert.IsTrue(events.Any(x => Regex.Match(x,
                $"DEBUG.*Improving.MediatR.Tests.PingHandler.*Completed Ping .* with {pong}").Success));
        }

        [TestMethod]
        public async Task Should_Log_Exceptions_To_Error_Level()
        {
            try
            {
                await _mediator.SendAsync(new Ping
                {
                    ThrowException = new Exception("Something bad")
                });
                Assert.Fail("Should have raised an exception");
            }
            catch (Exception)
            {
                var events = _MemoryTarget.Logs;
                Assert.IsTrue(events.Any(x => Regex.Match(x, 
                    "ERROR.*Improving.MediatR.Tests.PingHandler.*Failed Ping").Success));
            }
        }

        [TestMethod]
        public async Task Should_Log_InvalidOperationExceptions_To_Warn_Level()
        {
            try
            {
                await _mediator.SendAsync(new Ping
                {
                    ThrowException = new InvalidOperationException()
                });
                Assert.Fail("Should have raised an exception");
            }
            catch (Exception)
            {
                var events = _MemoryTarget.Logs;
                Assert.IsTrue(events.Any(x => Regex.Match(x,
                    "WARN.*Improving.MediatR.Tests.PingHandler.*Failed Ping").Success));
            }
        }

        [TestMethod]
        public async Task Should_Log_Validation_Exceptions_To_Warn_Level()
        {
            try
            {
                await _mediator.SendAsync(new Ping { Timestamp = null });
                Assert.Fail("Should have raised an exception");
            }
            catch (Exception)
            {
                var events = _MemoryTarget.Logs;
                Assert.IsTrue(events.Any(x => Regex.Match(x,
                    "WARN.*Improving.MediatR.Tests.PingHandler.*Failed Ping").Success));
            }
        }
    }
}
