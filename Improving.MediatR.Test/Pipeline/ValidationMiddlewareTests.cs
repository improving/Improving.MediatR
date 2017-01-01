using System.Linq;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using FluentValidation;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Improving.MediatR.Tests.Pipeline
{
    [TestClass]
    public class ValidationMiddlewareTests
    {
        private IWindsorContainer _container;
        private IMediator _mediator;

        [TestInitialize]
        public void TestInitialize()
        {
            _container = new WindsorContainer().Install(
                FromAssembly.This(),
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
        public async Task When_Validation_Fails_Should_Throw_Exception()
        {
            try
            {
                await _mediator.SendAsync(new Ping { Timestamp = null });
                Assert.Fail("Should have failed validation");
            }
            catch (ValidationException vex)
            {
                Assert.IsTrue(vex.Errors.Any(vf => vf.PropertyName == "Timestamp" &&
                    vf.ErrorMessage.Equals("'Timestamp' must not be empty.")));
            }
        }
    }
}
