using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public class TextController : Controller
    {
        readonly ILogger<TextController> log;
        private readonly IStringLocalizer<TextController> _localizer;

        public TextController(IStringLocalizer<TextController> localizer)
        {
            //this.log = log;
            _localizer = localizer;
        }

        // GET: /Text/
        public IActionResult Index()
        {
            //BackgroundJob.Enqueue(
            //    () => Log.SeqLog.WriteNewLogMessage("Blub - Hello current time is {Time}", DateTime.Now));

            // return View();

            string msg = "Shared resx: " + _localizer["Hello!"];
            return View();
        }

        // GET: /Text/Add/
        public IActionResult Add()
        {
            log.LogInformation("Logging: asdfasdf");
            return View("TextCreate");
        }
    }
}