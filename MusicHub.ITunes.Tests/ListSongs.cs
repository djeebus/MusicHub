using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicHub.ITunes.Tests
{
    [TestClass]
    public class ListSongs
    {
        [TestMethod]
        public void CreateLibrary()
        {
            var info = new LibraryInfo
            {
                Location = "127.0.0.1",
            };

            var library = new ITunesMediaLibrary(info);

            foreach (var song in library.GetSongs())
            {
                Trace.WriteLine(string.Format("Song: {0} by {1}", song.Title, song.Artist), "ListSongs");
            }
        }
    }
}
