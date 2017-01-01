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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Cache;
    using Castle.Core;
    using Castle.Windsor.Installer;
    using Inspect;
    using Route;

    public class MediatRInstaller : IWindsorInstaller
    {
        private readonly FromAssemblyDescriptor[] _fromAssemblies;

        private static readonly List<RequestMetadata> _supportedRequests
            = new List<RequestMetadata>();
        private static readonly List<RequestMetadata> _supportedNotifications
            = new List<RequestMetadata>();

        public MediatRInstaller(params FromAssemblyDescriptor[] fromAssemblies)
        {
            _fromAssemblies = fromAssemblies;
        }

        public static ICollection<RequestMetadata> SupportedRequests      => _supportedRequests;
        public static ICollection<RequestMetadata> SupportedNotifications => _supportedNotifications;

        public static event Action<RequestMetadata[]> RequestsSupported;
        public static event Action<RequestMetadata[]> NotificationsSupported;

        public static void Reset()
        {
            _supportedRequests.Clear();
            _supportedNotifications.Clear();

            RequestsSupported      = null;
            NotificationsSupported = null;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var childContainer = new WindsorContainer(
                "Improving.MediatR", new DefaultKernel(), new DefaultComponentInstaller());
            childContainer.Kernel.Resolver.AddSubResolver(new EnvironmentResolver());
            childContainer.Kernel.Resolver.AddSubResolver(
                new ArrayResolver(childContainer.Kernel, true));
            childContainer.Kernel.AddHandlersFilter(new RestHandlerFilter());
            childContainer.Kernel.AddHandlersFilter(
                new OpenGenericHandlersFilter(typeof(BatchOf<,>), typeof(BatchHandler<,>)));
            childContainer.Kernel.AddHandlersFilter(
                new OpenGenericHandlersFilter(typeof(Cached<>), typeof(CacheHandler<>)));
            childContainer.Kernel.AddHandlersFilter(
                new OpenGenericHandlersFilter(typeof(Routed<>), typeof(RouteHandler<>)));
            childContainer.Kernel.AddHandlersFilter(new ContravariantFilter());
            childContainer.Kernel.AddHandlerSelector(new PipelineSelector());

            childContainer.Register(
                Component.For<IMediator>().ImplementedBy<ScopedMediator>(),
                Component.For<EnvironmentInterceptor>().LifestyleTransient()
            );

            childContainer.Kernel.ComponentModelCreated += Kernel_ComponentModelCreated;

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
                    .OrBasedOn(typeof (IRouter))
                    .WithServiceBase()
                    .Configure(c =>
                    {
                        if (c.Implementation != null &&
                            typeof (IRouter).IsAssignableFrom(c.Implementation))
                            return;
                        c.LifestyleScoped();
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

        private static void Kernel_ComponentModelCreated(ComponentModel model)
        {
            var impl = model.Implementation;
            if (impl.IsGenericType &&
                impl.GetGenericTypeDefinition() == typeof(MediatorPipeline<,>))
                return;

            var newRequests = model.Services
                .Select(svc => svc.IsGenericType && !svc.IsGenericTypeDefinition &&
                               svc.GetGenericTypeDefinition() == typeof(IAsyncRequestHandler<,>)
                             ? svc : null)
                .Where(handler => handler != null)
                .Select(handler =>
                {
                    var args    = handler.GetGenericArguments();
                    var request = args[0];
                    return request.IsGenericType
                        || typeof(IRequestDecorator).IsAssignableFrom(request) ? null 
                         : new RequestMetadata
                           {
                               RequestType  = request,
                               ResponseType = args[1],
                               HandlerType  = impl
                           };
                })
                .Where(metadata => metadata != null)
                .ToArray();
            _supportedRequests.AddRange(newRequests);

            var newNotifications = model.Services
                .Select(svc => svc.IsGenericType && !svc.IsGenericTypeDefinition &&
                               svc.GetGenericTypeDefinition() == typeof(IAsyncNotificationHandler<>)
                             ? svc : null)
                .Where(handler => handler != null)
                .Select(handler =>
                {
                    var args         = handler.GetGenericArguments();
                    var notification = args[0];
                    return notification.IsGenericType 
                        || typeof(INotificationDecorator).IsAssignableFrom(notification) ? null 
                         : new RequestMetadata
                           {
                               RequestType  = notification,
                               ResponseType = null,
                               HandlerType  = impl
                           };
                })
                .Where(metadata => metadata != null)
                .ToArray();
            _supportedNotifications.AddRange(newNotifications);

            if (newRequests.Length > 0)
                RequestsSupported?.Invoke(newRequests);

            if (newNotifications.Length > 0)
                NotificationsSupported?.Invoke(newNotifications);
        }

        /// <summary>
        /// Allows handlers to be resolved contravariantly (in).
        /// </summary>
        private class ContravariantFilter : IHandlersFilter
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
                return handlers  // prefer concrete handlers
                    .Where(h => !h.ComponentModel.Implementation.IsGenericTypeDefinition)
                    .ToArray();
            }
        }

        /// <summary>
        /// Ensures the MediatorPipeline is the default IRequestHandler.
        /// </summary>
        private class PipelineSelector : IHandlerSelector
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
