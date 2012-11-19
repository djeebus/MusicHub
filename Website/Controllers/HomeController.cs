using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website.Controllers
{
	public class HomeController : Controller
	{
		private readonly MusicHub.IMusicLibrary _musicRepository;

		public HomeController(MusicHub.IMusicLibrary musicRepository)
		{
			this._musicRepository = musicRepository;
		}

		public JsonResult Songs()
		{
			var songs = this._musicRepository.GetSongs();

			return this.Json(songs, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Index()
		{
			return this.View();
		}
	}
}
