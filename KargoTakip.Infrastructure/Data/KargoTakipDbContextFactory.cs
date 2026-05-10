using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Data
{
    public class KargoTakipDbContextFactory
        : IDesignTimeDbContextFactory<KargoTakipDbContext>
    {
        public KargoTakipDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<KargoTakipDbContext>();

            optionsBuilder.UseSqlServer(
    "Server=localhost,1433;Database=KargoTakipDB;User Id=sa;Password=KargoTakip123!;TrustServerCertificate=True;"
);

            return new KargoTakipDbContext(optionsBuilder.Options);
        }
    }
}
