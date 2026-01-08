using Microsoft.AspNetCore.Mvc;

namespace AgentPay.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        return View();
    }
}
