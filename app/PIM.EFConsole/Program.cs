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

        // PimDbContext holds IArchivable and IHasConcurrencyToken entities but is registered here without UseEntities(),
        // so DbContextWiring never reaches it — add the archived filter and the token convention to the options directly.
        services.AddDbContext<PimDbContext>(options => options.UseSqlite(connectionString).AddArchivedQueryFilter().AddConcurrencyTokenConvention());
    })
    .Build();

Console.WriteLine("Created Host");
