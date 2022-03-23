using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Ordering.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, Action<TContext, IServiceProvider> seeder, int? retry=0) where TContext : DbContext
        {
            int retryForAvailability = retry.Value;

            using (var scope =  host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();

                try
                {
                    logger.LogInformation("Migrating Database associated with context {DbContextName}", typeof(TContext).Name);

                    InvokeSeeder(seeder, context, services);

                    logger.LogInformation("Migrated Database associated with the context {DbContextName}", typeof(TContext).Name);
                }
                catch (SqlException e)
                {
                    logger.LogError(e, "An error occured while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                    if (retryForAvailability < 50)
                    {
                        retryForAvailability++;
                        Thread.Sleep(2000);
                        MigrateDatabase<TContext>(host, seeder, retryForAvailability);
                    }
                    throw;
                }
            }

            return host;
        }

        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services) where TContext : DbContext
        {
            context.Database.Migrate();
            seeder(context, services);
        }
    }
}
