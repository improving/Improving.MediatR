namespace Improving.MediatR.Tests.Concurrency
{
    using System;
    using System.Threading.Tasks;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;
    using global::MediatR;
    using MediatR.Concurrency;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SequentialTests
    {
        private IWindsorContainer _container;
        private IMediator _mediator;

        [TestInitialize]
        public void TestInitialize()
        {
            _container = new WindsorContainer()
                .Install(FromAssembly.This(),
                         new MediatRInstaller(Classes.FromThisAssembly())
                         );
            _mediator = _container.Resolve<IMediator>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _container.Dispose();
        }

        [TestMethod]
        public async Task Should_Execute_Sequentially()
        {
            var result = await _mediator.SendAsync(new Concurrent
            {
                Requests = new[]
                {
                    new Ping { DelayMs = 5  }, new Ping()
                }
            });

            Assert.AreEqual(2, result.Responses.Length);
        }

        [TestMethod]
        public async Task Should_Propogate_Exception()
        {
            try
            {
                await _mediator.SendAsync(new Concurrent
                {
                    Requests = new[]
                    {
                        new Ping(),
                        new Ping { ThrowException = new InvalidOperationException() }
                    }
                });
                Assert.Fail("Expected an exception");
            }
            catch (InvalidOperationException)
            {            
            }
        }
    }
}
