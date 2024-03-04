using BookService.Api.Extensions;
using LookBook.Web.Services;
using OcelotApiGateway.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//ocelot consul register ayarlar�
builder.Services.ConfigureConsul(builder.Configuration);


// appsettings.json'dan yap�land�rmay� y�kle
var serviceUrls = builder.Configuration.GetSection("ServiceUrls").Get<Dictionary<string, string>>();

// HttpClientFactory'yi servis konteynerine ekleyin
builder.Services.AddHttpClient();

// �zel HttpClient yap�land�rmalar�
builder.Services.AddHttpClient("BaseUrl", client =>
{
    client.BaseAddress = new Uri(serviceUrls["BaseAddress"]);
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

// ocelot consul register
// Uygulama �mr�n� y�netmek i�in IHostApplicationLifetime al�n
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

// Consul ile kay�t i�lemi
app.RegisterWithConsul(lifetime, builder.Configuration);

// middleware t�m servislerde olacak
app.UseMiddleware<CustomHeaderMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
