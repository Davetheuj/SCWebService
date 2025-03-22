using Microsoft.EntityFrameworkCore;
using SCWebService.Models.Entities;

namespace SCWebService.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
 