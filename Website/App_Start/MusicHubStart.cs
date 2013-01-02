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

            var spider = resolver.GetService<MusicHub.SongSpider>();

            var libraryRepository = resolver.GetService<MusicHub.ILibraryRepository>();
            var libraries = libraryRepository.GetLibraries();

            var factory = resolver.GetService<Models.MediaLibraryFactory>();

            foreach (var libraryInfo in libraries)
            {
                var library = factory.GetLibrary(libraryInfo.Id);

                spider.QueueLibrary(library);
            }
        }

        private static void PlayRandomSong()
        {
            var resolver = DependencyResolver.Current;

            var songSelector = resolver.GetService<MusicHub.ISongRepository>();
            var randomSong = songSelector.GetRandomSong();
            if (randomSong == null)
                return;

            var factory = resolver.GetService<MediaLibraryFactory>();

            var library = factory.GetLibrary(randomSong.LibraryId);

            var player = DependencyResolver.Current.GetService<MusicHub.IMediaPlayer>();

            var songUrl = library.GetSongUrl(randomSong.ExternalId);

            player.PlaySong(randomSong, songUrl);
        }
    }
}