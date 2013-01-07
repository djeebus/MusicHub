using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Website.Models;

namespace Website.App_Start
{
    public static class MusicHubStart
    {
        public static void Start(HttpApplication application)
        {
            BeginUpdatingLibraries();

            if (ConfigurationManager.AppSettings["Autostart playback"] == "true")
                PlayRandomSong();
        }

        private static void BeginUpdatingLibraries()
        {
            var resolver = DependencyResolver.Current;

            var jukebox = resolver.GetService<MusicHub.IJukebox>();

            //var factory = resolver.GetService<Models.MediaLibraryFactory>();

            //foreach (var libraryInfo in libraries)
            //{
            //    var library = factory.GetLibrary(libraryInfo.Id);

            //    spider.QueueLibrary(library);
            //}
        }

        private static void PlayRandomSong()
        {
            var resolver = DependencyResolver.Current;

            var jukebox = resolver.GetService<MusicHub.IJukebox>();
            jukebox.Play();
        }
    }
}