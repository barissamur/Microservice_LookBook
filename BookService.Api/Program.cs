using BookService.Api.Areas.GraphQL.Queries;
using BookService.Api.Extensions;
using BookService.Api.Repository;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Playground;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using OcelotApiGateway.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


//ocelot consul register ayarlar�
builder.Services.ConfigureConsul(builder.Configuration);
 
// Serilog configuration
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day));


// MongoDB ba�lant� dizesini appsettings.json dosyas�ndan al
var mongoConnectionString = builder.Configuration.GetConnectionString("BookStoreDatabase");


// MongoDB istemcisini kaydet ve loglama i�in yap�land�r
builder.Services.AddSingleton<IMongoClient>(s =>
{
    var mongoClientSettings = MongoClientSettings.FromConnectionString(mongoConnectionString);

    mongoClientSettings.ClusterConfigurator = cb =>
    {
        cb.Subscribe<CommandStartedEvent>(e =>
        {
            Log.Information("MongoDB Command Started: {CommandName} - {Command}", e.CommandName, e.Command.ToJson());
        });
    };

    return new MongoClient(mongoClientSettings);
});


// Uygulama i�in MongoDB veritaban�n� kaydet
builder.Services.AddSingleton<IMongoDatabase>(s =>
{
    var client = s.GetRequiredService<IMongoClient>();
    return client.GetDatabase("bookstore"); // Veritaban� ad�n�z
});


//mongodb servisi
builder.Services.AddSingleton<BookRepository>();


// graphql
builder.Services
      .AddGraphQLServer()
      .AddQueryType<Query>()
      .AddProjections();

// Add services to the container.

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

// middleware t�m servislerde olacak
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
