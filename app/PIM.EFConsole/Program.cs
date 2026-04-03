using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PIM.Data;

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
        var connectionString = config.GetConnectionString("PIM");

        services.AddDbContext<PimDbContext>(options => options.UseSqlite(connectionString));
    })
    .Build();

Console.WriteLine("Created Host");
