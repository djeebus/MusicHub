[assembly: WebActivator.PreApplicationStartMethod(typeof(Website.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(Website.App_Start.NinjectWebCommon), "Stop")]

namespace Website.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon
    {
        public static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Settings.AllowNullInjection = true;

            kernel.Bind<MusicHub.IMetadataService>().To<MusicHub.TagLibSharp.TagLibSharpMetadataService>().InSingletonScope();
            kernel.Bind<MusicHub.IMusicRepository>().To<Website.MusicRepository>().InSingletonScope();
            kernel.Bind<MusicHub.IUserRepository>().To<MusicHub.ActiveDirectory.ActiveDirectoryUserRepository>().InSingletonScope();
            kernel.Bind<MusicHub.IMediaServer>().To<MusicHub.FMod.FModMediaServer>().InSingletonScope();
            kernel.Bind<MusicHub.ISongSelector>().To<MusicHub.Implementation.DefaultSongSelector>().InSingletonScope();

            // for HubDispatcher.ProcessRequestAsync()
            kernel.Bind<SignalR.Hubs.IJavaScriptProxyGenerator>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Hubs.IHubManager>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Hubs.IHubActivator>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.IDependencyResolver>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Hubs.IJavaScriptMinifier>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Hubs.IParameterResolver>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Hubs.IHubRequestParser>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.IMessageBus>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.IConnectionIdGenerator>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.IJsonSerializer>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Transports.ITransportManager>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Infrastructure.ITraceManager>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Infrastructure.IServerCommandHandler>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Transports.ITransportHeartBeat>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.IConfigurationManager>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Infrastructure.IServerIdManager>().ToNull().InSingletonScope();
        }

        public class NinjectDependencyResolver : SignalR.DefaultDependencyResolver
        {
            private readonly IKernel _kernel;

            public NinjectDependencyResolver(IKernel kernel)
            {
                this._kernel = kernel;
            }

            public override object GetService(Type serviceType)
            {
                try 
                { 
                    var ob = this._kernel.Get(serviceType);
                    if (ob != null)
                        return ob;
                }
                catch
                {
                }

                return base.GetService(serviceType);
            }
        }
    }

    static class NinjectExtensions
    {
        public static Ninject.Syntax.IBindingWhenInNamedWithOrOnSyntax<T> ToNull<T>(this Ninject.Syntax.IBindingToSyntax<T> syntax)
            where T : class
        {
            return syntax.ToConstant((T)null);
        }
    }
}
