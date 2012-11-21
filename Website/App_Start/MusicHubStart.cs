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
            if (ConfigurationManager.AppSettings["Autostart playback"] == "true")
                PlayRandomSong();
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