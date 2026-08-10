using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PIM.Data;
using Regira.Entities.EFcore.Extensions;

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

        // PimDbContext holds IArchivable entities but is registered here without UseEntities(), so
        // DbContextWiring.ArchivedQueryFilter never reaches it — add the filter to the options directly.
        services.AddDbContext<PimDbContext>(options => options.UseSqlite(connectionString).AddArchivedQueryFilter());
    })
    .Build();

Console.WriteLine("Created Host");
