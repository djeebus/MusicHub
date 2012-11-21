using SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

using Ninject;

namespace Website.App_Start
{
    public static class SignalRStart
    {
        public static void Start()
        {
            GlobalHost.DependencyResolver = new MvcDependencyResolver();
            RouteExtensions.MapHubs(RouteTable.Routes);
        }
    }

    public class NinjectHubActivator : SignalR.Hubs.IHubActivator
    {
        private readonly IKernel _kernel;

        public NinjectHubActivator(IKernel kernel)
        {
            if (kernel == null)
                throw new ArgumentNullException("kernel");

            this._kernel = kernel;
        }

        public SignalR.Hubs.IHub Create(SignalR.Hubs.HubDescriptor descriptor)
        {
            if (descriptor.Type == null)
                return null;

            return (SignalR.Hubs.IHub)_kernel.Get(descriptor.Type);
        }
    }

    public class MvcDependencyResolver : DefaultDependencyResolver
    {
        public override object GetService(Type serviceType)
        {
            try
            {
                var ob = System.Web.Mvc.DependencyResolver.Current.GetService(serviceType);
                if (ob != null)
                    return ob;
            }
            catch
            {
            }

            return base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                var ob = System.Web.Mvc.DependencyResolver.Current.GetServices(serviceType);
                if (ob != null && ob.Count() != 0)
                    return ob;
            }
            catch
            {
            }

            return base.GetServices(serviceType);
        }
    }
}