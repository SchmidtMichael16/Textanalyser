using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Controllers
{
    public class TextController : Controller
    {
        readonly ILogger<TextController> log;

        public TextController(ILogger<TextController> log)
        {
            this.log = log;
        }

        // GET: /Text/
        public IActionResult Index()
        {
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