using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Configuration;

namespace Website
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new System.Web.Mvc.AuthorizeAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);
		}

		protected void Application_Start()
		{
            SignalR.GlobalHost.DependencyResolver = new Website.App_Start.NinjectWebCommon.NinjectDependencyResolver(Website.App_Start.NinjectWebCommon.bootstrapper.Kernel);
            SignalR.RouteExtensions.MapHubs(RouteTable.Routes);

			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);

			BundleTable.Bundles.RegisterTemplateBundles();

			if (ConfigurationManager.AppSettings["Autostart playback"] == "true")
				PlayRandomSong();
		}

		private static void PlayRandomSong()
		{
			var songSelector = DependencyResolver.Current.GetService<MusicHub.ISongSelector>();
			var randomSong = songSelector.GetRandomSong();

			var server = DependencyResolver.Current.GetService<MusicHub.IMediaServer>();
			server.PlaySong(randomSong);
		}
	}
}