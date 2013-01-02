using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website.Controllers
{
	public class HomeController : Controller
	{
        private readonly MusicHub.ILibraryRepository _libraryRepository;
        private readonly Models.MediaLibraryFactory _mediaLibraryFactory;
        private readonly MusicHub.SongSpider _songSpider;

		public HomeController(
            MusicHub.ILibraryRepository libraryRepository, 
            Models.MediaLibraryFactory mediaLibraryFactory, 
            MusicHub.SongSpider songSpider)
		{
            this._libraryRepository = libraryRepository;
            this._mediaLibraryFactory = mediaLibraryFactory;
            this._songSpider = songSpider;
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
            this._libraryRepository.Create(this.UserId, MusicHub.LibraryType.GoogleMusic, null, username, password);

            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public RedirectToRouteResult AddSharedFolderLibrary(string path)
        {
            this._libraryRepository.Create(this.UserId, MusicHub.LibraryType.SharedFolder, path, null, null);

            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public RedirectToRouteResult RemoveLibrary(string libraryId)
        {
            this._libraryRepository.Delete(libraryId);

            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public RedirectToRouteResult ResyncLibrary(string libraryId)
        {
            var library = _mediaLibraryFactory.GetLibrary(libraryId);

            _songSpider.QueueLibrary(library);

            return this.RedirectToAction("Index");
        }
	}
}
