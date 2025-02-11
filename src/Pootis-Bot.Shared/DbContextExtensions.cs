using System.Data.Common;
using System.Diagnostics;
using Medallion.Threading.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Shared.Messages;

namespace Pootis_Bot.Shared;

public static class DbContextExtensions
{
    /// <summary>
    ///     Adds <see cref="PootisBotDbContext"/> to a <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="typePrefix"></param>
    /// <returns></returns>
    public static IServiceCollection UsePootisBotDbContext(this IServiceCollection services,
        IConfiguration configuration, string typePrefix)
    {
        string connectionStringName = $"{typePrefix}Connection";
        string? connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new NullReferenceException($"Connection string for {typePrefix} was not provided!");
        
        //Need to enable dynamic JSON
        services.AddNpgsqlDataSource(connectionString, builder =>
        {
            builder.EnableDynamicJson();
        });
        
        services.AddDbContextFactory<PootisBotDbContext>(options => options.UseNpgsql());
        //services.AddDbContext<PootisBotDbContext>(options => options.UseNpgsql(x => x.MapEnum<MessageType>()));
        return services;
    }

    public static IHost HandleDbMigrations(this IHost host)
    {
        using (IServiceScope scope = host.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbContextExtensions));

            PootisBotDbContext dbContext = services.GetRequiredService<PootisBotDbContext>();
            
            logger.LogInformation("Checking database...");

            //Get connection and open it
            DbConnection connection = dbContext.Database.GetDbConnection();
            connection.Open();

            //PostgresDistributedLock uses Postgres's Advisory Locks.
            //Upto the app to respect the lock
            //
            //https://www.postgresql.org/docs/9.4/explicit-locking.html#ADVISORY-LOCKS
            PostgresDistributedLock migrationLock =
                new(new PostgresAdvisoryLockKey("MigrationsLock", true), connection);
            using (migrationLock.Acquire())
            {
                bool pendingMigrations = dbContext.Database.GetPendingMigrations().Any();

                if (pendingMigrations)
                {
                    logger.LogWarning("Database requires migrations! Migrating...");
                    try
                    {
                        dbContext.Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occured while migrating the database!");
                        throw;
                    }
                }
            }

            return host;
        }
    }
}