using EasyCaching.Core;
using EasyCaching.Serialization.SystemTextJson.Configurations;
using Microsoft.EntityFrameworkCore;
using OpenIddictSetUp.Context;
using OpenIddictSetUp.Contract.Abstraction;
using OpenIddictSetUp.Contract.Implementation;

namespace OpenIddictSetUp
{
    public static partial class Startup
    {
        public static void ConfigureServices( this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data source=OpenIddictSetup.db"));

            builder.Services.AddEasyCaching(options =>
            {
                //use memory cache

                options.WithSystemTextJson();
                options.UseInMemory(builder.Configuration, "default", "easycaching:inmemory");
            });

            builder.Services.AddTransient<IMemoryCacheService, MemoryCacheService>();
            builder.Services.AddTransient(typeof(TimeSpan), _ => TimeSpan.FromSeconds(1D));

            builder.Services.AddSingleton<IMemoryCacheService>(provider =>
            {
                return new MemoryCacheService(provider.GetRequiredService<IEasyCachingProvider>(),
                    TimeSpan.FromDays(1));
            });
        }
    }
}
