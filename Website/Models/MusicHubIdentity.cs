using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Website.Models
{
    public class MusicHubIdentity : IIdentity
    {
        public static MusicHubIdentity CurrentIdentity
        {
            get
            {
                var principal = MusicHubPrincipal.CurrentPrincipal;
                if (principal == null)
                    return null;

                return principal.Identity as MusicHubIdentity;
            }
        }

        public MusicHub.User User { get; private set; }

        public MusicHubIdentity(MusicHub.User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            this.User = user;
        }

        public string AuthenticationType
        {
            get { return "MusicHub"; }
        }

        public bool IsAuthenticated
        {
            get { return true; }
        }

        public string Name
        {
            get { return this.User.DisplayName; }
        }
    }
}