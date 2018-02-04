using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Web.Models;
using Web;
using Data.Contexts;
using Data.Entities;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer<SharedResources> _sharedLocalizer;
        private readonly TextContext textContext;
        public HomeController(TextContext textContext, IStringLocalizer<SharedResources> sharedLocalizer)
        {
            this.textContext = textContext;
            _sharedLocalizer = sharedLocalizer;
        }

        public IActionResult Index()
        {
            Log.SeqLog.WriteNewLogMessage("Home/Index was requested! - ");

            return View();
        }

        public IActionResult Add()
        {
            if (!User.Identity.IsAuthenticated)
            {
                Redirect("/Account/Login");
            }
            ViewData["Message"] = "Add";

            return View();
        }

        public IActionResult Edit()
        {
            ViewData["Message"] = "Edit";

            return View();
        }

        public IActionResult Delete()
        {
            ViewData["Message"] = "Delete";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public void TextCreate(TextViewModel newText)
        {
            textContext.Add<Text>(new Text() { Title = newText.Title, Data = newText.Text});
            int rowCount = textContext.SaveChanges();
            Log.SeqLog.WriteNewLogMessage("Add new Text with Title {Title} - {rows} rows inserted", newText.Title, rowCount);

            Redirect("/Home/Add");
            //return View();
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
