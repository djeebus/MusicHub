using MusicHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace Website.Models
{
    public class MusicHubPrincipal : IPrincipal
    {
        public static MusicHubPrincipal CurrentPrincipal
        {
            get
            {
                return Thread.CurrentPrincipal as MusicHubPrincipal;
            }
        }

        private readonly MusicHubIdentity _identity;

        public MusicHubPrincipal(User user)
        {
            this._identity = new MusicHubIdentity(user);
        }

        public IIdentity Identity
        {
            get { return this._identity; }
        }

        public bool IsInRole(string role)
        {
            return false;
        }
    }
}