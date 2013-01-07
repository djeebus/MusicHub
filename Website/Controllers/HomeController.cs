using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website.Controllers
{
	public class HomeController : Controller
	{
        private readonly MusicHub.IJukebox _jukebox;

		public HomeController(
            MusicHub.IJukebox jukebox)
		{
            this._jukebox = jukebox;
		}

        public string UserId
        {
            get
            {
                var identity = Website.Models.MusicHubIdentity.CurrentIdentity;
                if (identity == null)
                    return null;

                return identity.User.Id;
            }
        }

		public ActionResult Index()
		{
			return this.View();
		}

        [HttpPost]
        public RedirectToRouteResult AddGoogleMusic(string username, string password)
        {
            this._jukebox.CreateLibrary(this.UserId, MusicHub.LibraryType.GoogleMusic, null, username, password);

            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public RedirectToRouteResult AddSharedFolderLibrary(string path)
        {
            this._jukebox.CreateLibrary(this.UserId, MusicHub.LibraryType.SharedFolder, path, null, null);

            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public RedirectToRouteResult RemoveLibrary(string libraryId)
        {
            this._jukebox.DeleteLibrary(libraryId);

            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public RedirectToRouteResult ResyncLibrary(string libraryId)
        {
            _jukebox.UpdateLibrary(libraryId);

            return this.RedirectToAction("Index");
        }
	}
}
