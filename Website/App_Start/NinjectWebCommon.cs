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

            // MusicHub.Core
            kernel.Bind<MusicHub.IJukebox>().To<MusicHub.Implementation.DefaultJukebox>().InSingletonScope();
            kernel.Bind<MusicHub.SongSpider>().ToSelf().InSingletonScope();

            // TagLibSharp
            kernel.Bind<MusicHub.IMetadataService>().To<MusicHub.TagLibSharp.TagLibSharpMetadataService>().InSingletonScope();

            // ActiveDirectory authentication
            kernel.Bind<MusicHub.IAuthenticationService>().To<MusicHub.ActiveDirectory.ActiveDirectoryAuthenticationService>().InSingletonScope();

            // BassNet
            kernel.Bind<MusicHub.IMediaPlayer>().To<MusicHub.BassNet.BassNetMediaPlayer>().InSingletonScope()
                .OnActivation(SubscribeToStatusChanges);

            // EntityFramework
            kernel.Bind<MusicHub.IConnectionRepository>().To<MusicHub.EntityFramework.ConnectionRepository>().InRequestScope();
            kernel.Bind<MusicHub.ILibraryRepository>().To<MusicHub.EntityFramework.LibraryRepository>().InRequestScope();
            kernel.Bind<MusicHub.EntityFramework.DbContext>().ToSelf().InRequestScope();
            kernel.Bind<MusicHub.ISongRepository>().To<MusicHub.EntityFramework.SongRepository>().InRequestScope();
            kernel.Bind<MusicHub.IUserRepository>().To<MusicHub.EntityFramework.UserRepository>().InRequestScope();
            kernel.Bind<MusicHub.IMusicLibraryFactory>().To<Models.MediaLibraryFactory>().InRequestScope();

            // for HubDispatcher.ProcessRequestAsync()
            kernel.Bind<SignalR.Hubs.IJavaScriptProxyGenerator>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Hubs.IHubManager>().ToNull().InSingletonScope();
            kernel.Bind<SignalR.Hubs.IHubActivator>().To<NinjectHubActivator>().InSingletonScope();
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

        private static void SubscribeToStatusChanges(Ninject.Activation.IContext arg1, MusicHub.BassNet.BassNetMediaPlayer mediaServer)
        {
            mediaServer.StatusChanged += (s,e) => {
                var hub = SignalR.GlobalHost.ConnectionManager.GetHubContext<Website.Hubs.MusicControl>();

                var clientProxy = new Website.Hubs.ClientProxy(hub.Clients, mediaServer);

                clientProxy.updateStatus(e.Status);
            };
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
