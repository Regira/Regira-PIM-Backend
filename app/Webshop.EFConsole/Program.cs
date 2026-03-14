using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Webshop.Data;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, builder) =>
    {
        builder.Sources.Clear();
        // add configuration
        builder
            .AddJsonFile("appsettings.json", true, true)
            .AddUserSecrets(typeof(Program).Assembly, true);
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;
        var connectionString = config.GetConnectionString("Webshop");

        services.AddDbContext<WebshopDbContext>(options => options.UseSqlite(connectionString));
    })
    .Build();

Console.WriteLine("Created Host");
