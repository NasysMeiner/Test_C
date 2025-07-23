using Microsoft.EntityFrameworkCore;
using Test_prod.Models;

namespace Test_prod.Data
{
    public class PostgreDbContext : DbContext
    {
        public PostgreDbContext(DbContextOptions<PostgreDbContext> options) : base(options)
        {
            Database.Migrate();
        }

        public DbSet<DataCell> Values { get; set; }
        public DbSet<StatisticsDataCell> Results { get; set; }
    }
}
