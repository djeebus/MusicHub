using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Website.App_Start
{
    public static class MusicHubStart
    {
        public static void Start(HttpApplication application)
        {
            //application.PostAuthenticateRequest += application_PostAuthenticateRequest;

            if (ConfigurationManager.AppSettings["Autostart playback"] == "true")
                PlayRandomSong();
        }

        static void application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var currentPrincipal = Thread.CurrentPrincipal;

            if (!currentPrincipal.Identity.IsAuthenticated)
                return;

            var userRepository = DependencyResolver.Current.GetService<MusicHub.IUserRepository>();
            var user = userRepository.GetByName(currentPrincipal.Identity.Name);

            Thread.CurrentPrincipal = new Models.MusicHubPrincipal(user);
        }

        private static void PlayRandomSong()
        {
            var songSelector = DependencyResolver.Current.GetService<MusicHub.ISongSelector>();
            var randomSong = songSelector.GetRandomSong();
            if (randomSong == null)
                return;

            var player = DependencyResolver.Current.GetService<MusicHub.IMediaPlayer>();
            player.PlaySong(randomSong);
        }
    }
}