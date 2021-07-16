using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WIN.Models;

namespace WIN.Controllers
{
    public class HelloWordController : Controller
    {
        public IActionResult HelloWord()
        {
            return Json(new { etna = "Hello World"});
        }
    }
}
