using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Configuration;
using System.Threading;
using System.Security.Principal;

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

		protected void Application_Start()
		{
            App_Start.SignalRStart.Start(); 
            
            AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);

			BundleTable.Bundles.RegisterTemplateBundles();

            App_Start.WebApi.Start(RouteTable.Routes);
            App_Start.Routes.Register(RouteTable.Routes);

            App_Start.MusicHubStart.Start(this);
		}

        protected void Application_End()
        {
            var player = DependencyResolver.Current.GetService<MusicHub.IMediaPlayer>();

            player.Stop();
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var currentPrincipal = Thread.CurrentPrincipal;

            if (!currentPrincipal.Identity.IsAuthenticated)
                return;

            if (!(currentPrincipal.Identity is WindowsIdentity))
                throw new ArgumentOutOfRangeException("currentPrincipal.Identity", currentPrincipal.Identity.GetType().ToString(), "Unknown identity");

            var userRepository = DependencyResolver.Current.GetService<MusicHub.IUserRepository>();
            var user = userRepository.EnsureUser(currentPrincipal.Identity.Name, currentPrincipal.Identity.Name);

            Thread.CurrentPrincipal = new Models.MusicHubPrincipal(user);
        }
	}
}