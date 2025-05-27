using Microsoft.EntityFrameworkCore;
using System;

namespace AvaloniaAzora.Data
{
    public class CustomDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public CustomDbContextFactory(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public AppDbContext CreateDbContext()
        {
            return new AppDbContext(_options);
        }
    }
}