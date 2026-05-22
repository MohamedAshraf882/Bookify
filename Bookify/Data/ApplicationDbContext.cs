using Bookify.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
           // builder.Entity<Category>().Property(c=>c.CreatedOn).HasDefaultValueSql("GETDATE()");
            base.OnModelCreating(builder);
            
        }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Author> Authors { get; set; }
        
    }
}
