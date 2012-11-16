[assembly: WebActivator.PreApplicationStartMethod(typeof(Website.App_Start.NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(Website.App_Start.NinjectMVC3), "Stop")]

namespace Website.App_Start
{
	using System.Reflection;
	using Microsoft.Web.Infrastructure.DynamicModuleHelper;
	using Ninject;
	using Ninject.Web.Mvc;

	public static class NinjectMVC3
	{
		/// <summary>
		/// Load your modules or register your services here!
		/// </summary>
		/// <param name="kernel">The kernel.</param>
		private static void RegisterServices(IKernel kernel)
		{
			kernel.Bind<MusicHub.IMetadataService>().To<MusicHub.TagLibSharp.TagLibSharpMetadataService>().InSingletonScope();
			kernel.Bind<MusicHub.IMusicRepository>().To<Website.MusicRepository>().InSingletonScope();
			kernel.Bind<MusicHub.IUserRepository>().To<MusicHub.ActiveDirectory.ActiveDirectoryUserRepository>().InSingletonScope();
			kernel.Bind<MusicHub.IMediaServer>().To<MusicHub.FMod.FModMediaServer>().InSingletonScope();
			kernel.Bind<MusicHub.ISongSelector>().To<MusicHub.Implementation.DefaultSongSelector>().InSingletonScope();
		}

		private static readonly Bootstrapper bootstrapper = new Bootstrapper();

		/// <summary>
		/// Starts the application
		/// </summary>
		public static void Start()
		{
			DynamicModuleUtility.RegisterModule(typeof(OnePerRequestModule));
			DynamicModuleUtility.RegisterModule(typeof(HttpApplicationInitializationModule));

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
			RegisterServices(kernel);
			SignalR.Hosting.AspNet.AspNetHost.SetResolver(
				new SignalR.Ninject.NinjectDependencyResolver(kernel));
			return kernel;
		}
	}
}
