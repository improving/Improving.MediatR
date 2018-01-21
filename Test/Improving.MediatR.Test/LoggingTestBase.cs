namespace Improving.MediatR.Tests
{
    using Castle.Facilities.Logging;
    using Castle.MicroKernel.Registration;
    using Castle.Services.Logging.NLogIntegration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;
    using global::MediatR;
    using NLog;
    using NLog.Config;
    using NLog.Targets;

    public class LoggingTestBase
    {
        protected MemoryTarget _MemoryTarget;
        protected IWindsorContainer _container;
        protected IMediator _mediator;

        protected void InitializeContainer()
        {
            var config = new LoggingConfiguration();
            _MemoryTarget = new MemoryTarget();
            config.AddTarget("InMemoryTarget", _MemoryTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, _MemoryTarget));
            LogManager.Configuration = config;
            _container = new WindsorContainer()
                .AddFacility<LoggingFacility>(f => f.LogUsing(new NLogFactory(config)))
                .Install(FromAssembly.This(),
                    new MediatRInstaller(Classes.FromThisAssembly())
                );
            _mediator = _container.Resolve<IMediator>();
        }
    }
}