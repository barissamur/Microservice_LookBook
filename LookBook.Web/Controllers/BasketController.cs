using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LookBook.Web.Controllers;

public class BasketController : Controller
{
    //[Authorize]
    public IActionResult Index()
    {
        return View();
    }
}
