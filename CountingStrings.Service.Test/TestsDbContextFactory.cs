using CountingStrings.Service.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CountingStrings.Service.Test
{
    public class TestsDbContextFactory : IDesignTimeDbContextFactory<CountingStringsContext>
    {
        public CountingStringsContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CountingStringsContext>();

            builder.UseSqlServer(
                $"Server=(localdb)\\mssqllocaldb;Database=CountingStrings;Trusted_Connection=True;MultipleActiveResultSets=true");

            var context = new CountingStringsContext(builder.Options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
