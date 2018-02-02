using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Contexts;
using Data.DbTasks;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers
{
    public class SearchController : Controller
    {

        private readonly TextContext textContext;

        public SearchController(TextContext textContext)
        {
            this.textContext = textContext;
        }

            public IActionResult Index()
        {

            return View();
        }

        [HttpPost("")]
        public IActionResult Index(SearchViewModel search)
        {
            if (ModelState.IsValid)
            {
                //return View(name);
                List<string> searchWords = new List<string>{ search.Word1, search.Word2, search.Word3 };
                return new ObjectResult(BackgroundTasks.FindWorsdInTexts(this.textContext, searchWords, search.AlsoSynonyms));
                //return $"post request {search.Word1}  {search.Word2}   {search.Word3}  {search.AlsoSynonyms}";
            }
            else
            {
                return View(null);
            }


        }
    }
}