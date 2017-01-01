using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Improving.MediatR.Tests.Route
{
    using System.Threading.Tasks;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;
    using global::MediatR;
    using MediatR.Route;

    [TestClass]
    public class PassThroughRouterTests
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
        public async Task Should_Pass_Through_Requests()
        {
            var pong = await _mediator.SendAsync(new Ping().RouteTo("pass-through"));
            Assert.IsNotNull(pong);
        }
    }
}
