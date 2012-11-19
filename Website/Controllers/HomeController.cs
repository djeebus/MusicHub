using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website.Controllers
{
	public class HomeController : Controller
	{
		public HomeController()
		{
		}

		public ActionResult Index()
		{
			return this.View();
		}
	}
}
