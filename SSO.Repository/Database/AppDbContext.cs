using Microsoft.EntityFrameworkCore;
using SSO.Repository.Entities;

namespace SSO.Repository.Database
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }


    }
}
