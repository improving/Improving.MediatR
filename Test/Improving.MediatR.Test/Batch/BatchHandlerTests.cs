using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Improving.MediatR.Batch;

namespace Improving.MediatR.Tests.Batch
{
    [TestClass]
    public class BatchHandlerTests
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
        public async Task Should_Send_Batch_Request()
        {
            var batch = new BatchOf<Ping, Pong>(new Ping(), new Ping());
            var results = await _mediator.SendAsync(batch);
            Assert.AreEqual(2, results.Batch.Length);
            CollectionAssert.AllItemsAreInstancesOfType(results.Batch, typeof(Pong));
        }

        [TestMethod]
        public async Task Should_Batch_Requests()
        {
            var batcher = _mediator.Batch(2);
            var results = await batcher.BatchAsync<Ping, Pong>(new Ping());
            Assert.IsNull(results);
            results = await batcher.BatchAsync<Ping, Pong>(new Ping());
            Assert.AreEqual(2, results.Batch.Length);
            CollectionAssert.AllItemsAreInstancesOfType(results.Batch, typeof(Pong));
        }

        [TestMethod]
        public async Task Should_Overflow_Batch()
        {
            var batcher = _mediator.Batch(2);
            var results = await batcher.BatchAsync<Ping, Pong>(new Ping());
            Assert.IsNull(results);
            results = await batcher.BatchAsync<Ping, Pong>(new Ping(), new Ping());
            Assert.AreEqual(3, results.Batch.Length);
            CollectionAssert.AllItemsAreInstancesOfType(results.Batch, typeof(Pong));
        }

        [TestMethod]
        public async Task Should_Send_Remaining_When_Batch_Ends()
        {
            var batcher = _mediator.Batch(2);
            await batcher.BatchAsync<Ping, Pong>(new Ping());
            var results = await batcher.FlushAsync<Ping, Pong>();
            Assert.AreEqual(1, results.Batch.Length);
            CollectionAssert.AllItemsAreInstancesOfType(results.Batch, typeof(Pong));
        }
    }
}
