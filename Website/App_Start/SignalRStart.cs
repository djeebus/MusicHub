using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Website.App_Start
{
    public static class SignalRStart
    {
        public static void Start()
        {
            global::SignalR.GlobalHost.DependencyResolver = new NinjectWebCommon.NinjectDependencyResolver(NinjectWebCommon.bootstrapper.Kernel);
            global::SignalR.RouteExtensions.MapHubs(RouteTable.Routes);
        }
    }
}