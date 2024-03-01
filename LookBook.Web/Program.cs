using LookBook.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// appsettings.json'dan yapýlandýrmayý yükle
var serviceUrls = builder.Configuration.GetSection("ServiceUrls").Get<Dictionary<string, string>>();

// HttpClientFactory'yi servis konteynerine ekleyin
builder.Services.AddHttpClient();

// Özel HttpClient yapýlandýrmalarý
builder.Services.AddHttpClient("IdentityService", client =>
{
    client.BaseAddress = new Uri(serviceUrls["IdentityService"]);
});

// IdentityService'yi DI konteynerine ekleyin
builder.Services.AddScoped<IdentityService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
