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

		public HomeController(MusicHub.ILibraryRepository libraryRepository)
		{
            this._libraryRepository = libraryRepository;
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

        public RedirectToRouteResult AddLibrary(string path)
        {
            this._libraryRepository.Create(this.UserId, path);

            return this.RedirectToAction("Index");
        }
	}
}
