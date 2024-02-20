using BookService.Api.Extensions;
using BookService.Api.Repository;
using MongoDB.Driver;
using OcelotApiGateway.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day));

//ocelot consul register ayarlarý
builder.Services.ConfigureConsul(builder.Configuration);

// MongoDB baðlantý dizesini appsettings.json dosyasýndan al
var mongoConnectionString = builder.Configuration.GetConnectionString("BookStoreDatabase");

// MongoDB istemcisini kaydet
builder.Services.AddSingleton<IMongoClient>(s =>
{
    return new MongoClient(mongoConnectionString);
});

// Uygulama için MongoDB veritabanýný kaydet
builder.Services.AddSingleton<IMongoDatabase>(s =>
{
    var client = s.GetRequiredService<IMongoClient>();
    return client.GetDatabase("bookstore"); // Veritabaný adýnýz
});

//mongodb servisi
builder.Services.AddSingleton<BookRepository>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.UseMiddleware<CustomHeaderMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
