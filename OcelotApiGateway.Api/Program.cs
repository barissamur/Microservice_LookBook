
using BookService.Api.Aggregator;
using LookBook.Web.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using OcelotApiGateway.Api.Middleware;
using Serilog;
using Serilog.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// aggregator için gerekli adresler 
builder.Services.AddHttpClient("BaseAdres", c =>
{
    c.BaseAddress = new Uri("https://localhost:5000/v1/");
});




// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();


// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // Host'u Serilog ile kullan


//ocelot  
// Ocelot yapılandırmasını ekleyin
builder.Configuration.AddJsonFile("Configurations/ocelot.json");

builder.Services.AddOcelot(builder.Configuration)
    .AddSingletonDefinedAggregator<BooksAndOrdersAggregator>()
    .AddConsul()
    .AddPolly();


// Ocelot JWT Authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    Console.WriteLine(options);
})
.AddJwtBearer("Bearer", options =>
{
    //options.Authority = "https://localhost:5005"; // Yetkilendirme sunucusunun URL'si

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_here_please_change")),
        ValidateIssuer = true,
        ValidIssuer = "ExampleIssuer",
        ValidateAudience = true,
        ValidAudience = "ExampleAudience",
        ValidateLifetime = true
    };
    Console.WriteLine(options);
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
});

builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRouting();

//ocelot servise giderken header'a eklenecekler
app.Use(async (context, next) =>
{
    string tokenValue = null;

    if (context.Request.Headers.TryGetValue("Authorization", out var headerValue))
        tokenValue = headerValue.ToString().Split(" ").Last().Trim();

    else if (context.Request.Cookies.TryGetValue("Authorization", out var cookieValue))
        tokenValue = cookieValue.Trim();


    if (!string.IsNullOrEmpty(tokenValue))
    {
        if (tokenValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            tokenValue = tokenValue.Substring("Bearer ".Length).Trim();
        }

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(tokenValue) as JwtSecurityToken;

        var userId = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == "nameid")?.Value;
        var userName = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            context.Request.Headers["X-User-Id"] = userId;
        }

        if (!string.IsNullOrEmpty(userName))
        {
            context.Request.Headers["X-User-Name"] = userName;
        }

        context.Request.Headers["X-Ocelot-Secret"] = "fromOcelot";
    }

    await next.Invoke();
});


 
// custom middleware'lar burada
app.UseMiddleware<TokenMiddleware>();
app.UseMiddleware<RedirectOn401Middleware>();

await app.UseOcelot();

app.MapControllers();

app.Run();
