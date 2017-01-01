namespace Improving.MediatR.Tests.Route
{
    using System;
    using System.Threading.Tasks;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;
    using global::MediatR;
    using MediatR.Route;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RouteHandlerTests
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
        public async Task Should_Route_Requests()
        {
            var result = await _mediator.SendAsync(new Ping().RouteTo("Trash"));
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Should_Fail_For_Unrecognized_Routes()
        {
            try
            {
                await _mediator.SendAsync(new Ping().RouteTo("NoWhere"));
            }
            catch (NotSupportedException ex)
            {
                Assert.AreEqual(ex.Message, "Unrecognized request route 'NoWhere'");
            }
        }
    }

    public class TrashRouter : IRouter
    {
        public bool CanRoute(Routed route, object message)
        {
            return route.Route == "Trash";
        }

        public Task<object> Route(Routed route, object message, IMediator mediator)
        {
            return Task.FromResult(null as object);
        }
    }
}
