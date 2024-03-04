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
                    // Giriþ baþarýlý, token'ý cookie olarak ayarla
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true, // Cookie'ye sadece HTTP üzerinden eriþilebilir
                        Secure = true,   // Cookie'yi yalnýzca güvenli baðlantýlar üzerinden gönder
                        IsEssential = true, // Cookie, uygulamanýn çalýþmasý için gerekli
                        Expires = DateTime.UtcNow.AddDays(1) // Cookie'nin geçerlilik süresi
                    };

                    // "Bearer" önekini çýkartýp yalnýzca token'ý sakla
                    Response.Cookies.Append("Authorization", $"Bearer {token}", cookieOptions);


                    // Ana sayfaya yönlendir
                    return RedirectToAction("Index");
                }
                else
                {
                    // Token boþ döndü, giriþ baþarýsýz
                    ViewBag.ErrorMessage = "Login failed. Please check your credentials and try again.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Login failed for user {Username}", userName);

                // Hata mesajý ile View'ý tekrar göster
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
