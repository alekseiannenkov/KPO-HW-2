using Microsoft.AspNetCore.HttpLogging;

namespace APIGateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All;
        });

        var app = builder.Build();

        app.UseHttpLogging();
        app.MapReverseProxy();
        app.Run();
    }
}