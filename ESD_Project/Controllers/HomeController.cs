using System.Diagnostics;
using ESD_Project.Models;
using ESD_Project.Services;
using Microsoft.AspNetCore.Mvc;

namespace ESD_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet("/test-email")]
        public async Task<IActionResult> TestEmail([FromServices] IEmailSender email)
        {
            await email.SendEmailAsync(
                "darelleong29@gmail.com",
                "SMTP Test",
                "If you can read this, SMTP is working!"
            );
            return Content("Test email sent.");
        }
    }
}
