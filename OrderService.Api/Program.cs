using BookService.Api.Areas.GraphQL.Queries;
using BookService.Api.Extensions;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Playground;
using Microsoft.EntityFrameworkCore;
using OcelotApiGateway.Api.Middleware;
using OrderService.Api.Data;
using OrderService.Api.IRepo;
using OrderService.Api.Repository;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


// Serilog configuration
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day));


//ocelot consul register ayarlar�
builder.Services.ConfigureConsul(builder.Configuration);


// postgresql context
builder.Services.AddDbContext<OrderContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// repoyu ekliyoruz 
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));


// graphql
builder.Services
      .AddGraphQLServer()
      .AddQueryType<Query>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

 
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UsePlayground(new PlaygroundOptions
    {
        QueryPath = "/api/graphql",
        Path = "/playground"
    });

    app.UseSwagger();
    app.UseSwaggerUI();
}


// ocelot consul register
// Uygulama �mr�n� y�netmek i�in IHostApplicationLifetime al�n
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();


// Consul ile kay�t i�lemi
app.RegisterWithConsul(lifetime, builder.Configuration);
app.UseMiddleware<CustomHeaderMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();


// graphql
app.UseEndpoints(endpoints =>
{
    app.MapGraphQL("/api/graphql");
});

app.MapControllers();

app.Run();
