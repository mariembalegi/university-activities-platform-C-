using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace UniHub.DAL
{
    public class DataContextFactory : IDesignTimeDbContextFactory<UniHubDbContext>
    {
        public UniHubDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UniHubDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=UniHubDB;Trusted_Connection=True;MultipleActiveResultSets=true");
            return new UniHubDbContext(optionsBuilder.Options);
        }
    }
}
