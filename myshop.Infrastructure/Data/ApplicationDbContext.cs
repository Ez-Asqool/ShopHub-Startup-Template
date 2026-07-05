using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using myshop.Domain.Entities;

namespace myshop.Infrastructure.Data
{
    public class ApplicationDbContext: IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {            
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        
    }
}
