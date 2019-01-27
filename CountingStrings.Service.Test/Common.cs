using AutoMapper;
using CountingStrings.Service.Data;
using CountingStrings.Service.Data.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CountingStrings.Service.Test
{
    class Common
    {
        public static CountingStringsContext GetDbContext(string connectionString)
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<CountingStringsContext>();

            builder.UseSqlServer(connectionString).UseInternalServiceProvider(serviceProvider);

            return new CountingStringsContext(builder.Options);
        }

        public static IMapper GetMapper()
        {
            var serviceProvider = new ServiceCollection()
                .AddAutoMapper(x => x.AddProfile(new SessionMapping()))
                .BuildServiceProvider();
            return serviceProvider.GetRequiredService<IMapper>();
        }
    }
}
