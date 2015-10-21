namespace Improving.MediatR
{
    using Castle.Core.Internal;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using FluentValidation;
    using global::MediatR;
    using Environment;
    using Batch;
    using Rest;
    using Pipeline;
    using System;
    using System.Linq;
    using System.Reflection;
    using Cache;

    public class MediatRInstaller : IWindsorInstaller
    {
        private readonly FromAssemblyDescriptor[] _fromAssemblies;

        public MediatRInstaller(params FromAssemblyDescriptor[] fromAssemblies)
        {
            _fromAssemblies = fromAssemblies;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var childContainer = new WindsorContainer();
            childContainer.Kernel.Resolver.AddSubResolver(new EnvironmentResolver());
            childContainer.Kernel.Resolver.AddSubResolver(
                new ArrayResolver(childContainer.Kernel, true));
            childContainer.Kernel.AddHandlersFilter(new RestHandlerFilter());
            childContainer.Kernel.AddHandlersFilter(new BatchHandlerFilter());
            childContainer.Kernel.AddHandlersFilter(new CacheHandlerFilter());
            childContainer.Kernel.AddHandlersFilter(new ContravariantFilter());
            childContainer.Kernel.AddHandlerSelector(new PipelineSelector());

            childContainer.Register(
                Component.For<IMediator>().ImplementedBy<ScopedMediator>(),
                Component.For<EnvironmentInterceptor>().LifestyleTransient()
            );

            foreach (var assembly in _fromAssemblies
                .Concat(new[] {Classes.FromThisAssembly()}))
            {
                childContainer.Register(assembly
                    .BasedOn(typeof (IRequestHandler<,>))
                    .OrBasedOn(typeof (IRequestMiddleware<,>))
                    .OrBasedOn(typeof (IAsyncRequestHandler<,>))
                    .OrBasedOn(typeof (INotificationHandler<>))
                    .OrBasedOn(typeof (IAsyncNotificationHandler<>))
                    .OrBasedOn(typeof (IValidator<>))
                    .WithServiceBase()
                    .LifestyleScoped()
                    .Configure(c =>
                    {
                        c.Proxy.Hook(new EnvironmentProxyGenerationHook())
                             .Interceptors<EnvironmentInterceptor>();
                        if (c.Implementation != null)
                        {
                            var requiresMatching = c.Implementation.GetInterface(
                                typeof (IRequireGenericMatching<>).FullName);
                            if (requiresMatching == null) return;
                            var matcher = requiresMatching.GetGenericArguments()[0];
                            c.ExtendedProperties(
                                Property.ForKey(Constants.GenericImplementationMatchingStrategy)
                                    .Eq(Activator.CreateInstance(matcher)));
                        }
                    }));
            }

            container.Register(Component.For<IValidatorFactory>()
                     .ImplementedBy<WindsorValidatorFactory>()
                     .OnlyNewServices());

            container.AddChildContainer(childContainer);
            container.Register(Component.For<IMediator>()
                     .Instance(childContainer.Resolve<IMediator>()));
        }
   
        /// <summary>
        /// Allows handlers to be resolved contravariantly (in).
        /// </summary>
        class ContravariantFilter : IHandlersFilter
        {
            public bool HasOpinionAbout(Type service)
            {
                if (!service.IsGenericType)
                    return false;

                var genericType = service.GetGenericTypeDefinition();
                var genericArguments = genericType.GetGenericArguments();
                return genericArguments.Count(arg => arg.GenericParameterAttributes
                    .HasFlag(GenericParameterAttributes.Contravariant)) == 1;
            }

            public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
            {
                return handlers;
            }
        }

        /// <summary>
        /// Ensures the MediatorPipeline is the default IRequestHandler.
        /// </summary>
        class PipelineSelector : IHandlerSelector
        {
            public bool HasOpinionAbout(string key, Type service)
            {
                if (!service.IsGenericType) return false;
                var genericDef = service.GetGenericTypeDefinition();
                return (genericDef == typeof (IRequestHandler<,>)
                    ||  genericDef == typeof (IAsyncRequestHandler<,>));
            }

            public IHandler SelectHandler(string key, Type service, IHandler[] handlers)
            {
                return (from h in handlers
                        let i = h.ComponentModel.Implementation
                        where i.IsGenericType &&
                              i.GetGenericTypeDefinition() == typeof(MediatorPipeline<,>)
                        select h).FirstOrDefault();
            }
        }
    }
}
