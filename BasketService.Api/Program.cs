using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Infrastructure.Repository;
using BookService.Api.Extensions;
using MassTransit;
using OcelotApiGateway.Api.Middleware;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


//ocelot consul register ayarlarý
builder.Services.ConfigureConsul(builder.Configuration);


// rabbitmq masstransit yapýlandýrma
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", 5673, "/", h => // Docker konteynerýndaki RabbitMQ AMQP portuna yönlendirme
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});


// Redis baðlantýsýný yapýlandýrma
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));


// Repository'yi kaydetme
builder.Services.AddScoped<IBasketRepository, RedisBasketRepository>();


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

// middleware tüm servislerde olacak
app.UseMiddleware<CustomHeaderMiddleware>();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
