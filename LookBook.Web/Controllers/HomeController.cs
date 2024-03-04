using LookBook.Web.Models;
using LookBook.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LookBook.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IdentityService _identityService;

        public HomeController(ILogger<HomeController> logger
            , IdentityService identityService)
        {
            _logger = logger;
            _identityService = identityService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            try
            {
                var token = await _identityService.LoginAsync(userName, password);

                if (!string.IsNullOrEmpty(token))
                {
                    // Giri� ba�ar�l�, token'� cookie olarak ayarla
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true, // Cookie'ye sadece HTTP �zerinden eri�ilebilir
                        Secure = true,   // Cookie'yi yaln�zca g�venli ba�lant�lar �zerinden g�nder
                        IsEssential = true, // Cookie, uygulaman�n �al��mas� i�in gerekli
                        Expires = DateTime.UtcNow.AddDays(1) // Cookie'nin ge�erlilik s�resi
                    };

                    // "Bearer" �nekini ��kart�p yaln�zca token'� sakla
                    Response.Cookies.Append("Authorization", $"Bearer {token}", cookieOptions);


                    // Ana sayfaya y�nlendir
                    return RedirectToAction("Index");
                }
                else
                {
                    // Token bo� d�nd�, giri� ba�ar�s�z
                    ViewBag.ErrorMessage = "Login failed. Please check your credentials and try again.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Login failed for user {Username}", userName);

                // Hata mesaj� ile View'� tekrar g�ster
                ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
                return View();
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
