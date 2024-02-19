
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Serilog yap�land�rmas�
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // Host'u Serilog ile kullan


//ocelot  
// Ocelot yap�land�rmas�n� ekleyin
builder.Configuration.AddJsonFile("Configurations/ocelot.json");
builder.Services.AddOcelot(builder.Configuration).AddConsul().AddPolly();


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



// polly 
// ILogger �rne�ini do�rudan servisten almak i�in bir Factory metod kullan�n.
builder.Services.AddHttpClient("MyClient", client =>
{
    // HttpClient yap�land�rmas�
}).AddPolicyHandler((provider, request) =>
{
    // ILogger �rne�ini servis sa�lay�c� �zerinden al�n
    var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("HttpClientPollyLogs");

    // Retry politikas�
    var retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => !msg.IsSuccessStatusCode)
        .RetryAsync(3, onRetry: (outcome, retryCount, context) =>
        {
            logger.LogWarning($"Yeniden deneme {retryCount} i�in HTTP iste�i ba�ar�s�z. Hata: {outcome.Exception?.Message}");
        });

    // Circuit Breaker politikas�
    var circuitBreakerPolicy = Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 2,
            durationOfBreak: TimeSpan.FromMinutes(1),
            onBreak: (outcome, breakDelay, context) =>
            {
                logger.LogWarning($"Circuit Breaker devreye girdi: {breakDelay.TotalSeconds} saniye. Hata: {outcome.Exception?.Message}");
            },
            onReset: context =>
            {
                logger.LogInformation("Circuit Breaker s�f�rland�.");
            },
            onHalfOpen: () =>
            {
                logger.LogInformation("Circuit Breaker yar�-a��k.");
            });

    return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


//ocelot
await app.UseOcelot();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
