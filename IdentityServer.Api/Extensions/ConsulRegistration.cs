using Consul;

namespace IdentityServer.Api.Extensions;

public static class ConsulRegistration
{
    public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfiguration configuration)
    {
        // consula hangi adresten set olacağımızı belirledik
        services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
        {
            var address = configuration["ConsulConfig:Address"];
            consulConfig.Address = new Uri(address);
        }));

        return services;
    }


    public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime, IConfiguration configuration)
    {
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();

        var loggingFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

        var logger = loggingFactory.CreateLogger<IApplicationBuilder>();

        // burada register işlemi yapılıyor
        var uri = configuration.GetValue<Uri>("ConsulConfig:ServiceAddress");
        var serviceName = configuration.GetValue<string>("ConsulConfig:ServiceName");
        var serviceId = configuration.GetValue<string>("ConsulConfig:ServiceId");

        var a = $"{uri.Scheme}://{uri.Host}:{uri.Port}/health";

        var registration = new AgentServiceRegistration()
        {
            ID = serviceId ?? "IdentityService",
            Name = serviceName ?? "IdentityService",
            Address = $"{uri.Host}",
            Port = uri.Port,
            Tags = [serviceName, serviceId]
        };

        logger.LogInformation("Registering with Consul");
        consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        consulClient.Agent.ServiceRegister(registration).Wait();

        lifetime.ApplicationStopping.Register(() =>
        {
            logger.LogInformation("Deregistering from Consul");
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        });

        return app;
    }
}
