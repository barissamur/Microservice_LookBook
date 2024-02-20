using IdentityServer.Api.Data;
using IdentityServer.Api.Extensions;
using IdentityServer.Api.Models;
using IdentityServer.Api.SeedData;
using IdentityServer.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//health check için
builder.Services.AddHealthChecks();

//ocelot consul register ayarlarý
builder.Services.ConfigureConsul(builder.Configuration);

// Serilog configuration
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day));

// identity için
builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
     .AddDefaultTokenProviders();



// JWT Authentication
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);


var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience
    };
});

//jwt servis
builder.Services.AddScoped<TokenService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Uygulama baþlatýldýðýnda rolleri oluþtur

// Uygulama baþlamadan önce rolleri oluþtur
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await SeedCreateRoles.CreateRoles(serviceProvider);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ocelot consul register
// Uygulama ömrünü yönetmek için IHostApplicationLifetime alýn
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

// Consul ile kayýt iþlemi
app.RegisterWithConsul(lifetime, builder.Configuration);

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication(); // JWT için
app.UseAuthorization();

app.MapControllers();

//health check için
app.UseHealthChecks("/health");



app.Run();
