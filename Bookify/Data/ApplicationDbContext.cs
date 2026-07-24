using Bookify.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BookCategory>().HasKey(b => new { b.BookId, b.CategoryId });
            // builder.Entity<Category>().Property(c=>c.CreatedOn).HasDefaultValueSql("GETDATE()");

            //sequence for book copy serial number
            builder.HasSequence<int>("SerialNumber", schema: "shared")
                .StartsAt(1000001);
            builder.Entity<BookCopy>()
                .Property(e => e.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");


            base.OnModelCreating(builder);
            //builder.Entity<IdentityUser>().ToTable("Users");        
            
        }

        public DbSet<Category> Categories { get; set; }


        public DbSet<Author> Authors { get; set; }
        public DbSet<Book>Books{ get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }


    }
}
