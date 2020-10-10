
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VNNSIS.Models;
using VNNSIS.ViewModels;

namespace VNNSIS.Data
{
    public class SqlDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<OperationNumberViewModel> OperationNumberViewModels {get; set;}
        public DbSet<SisProErrorRecord> sis_pro_error_record {get; set;}
        public SqlDbContext(DbContextOptions<SqlDbContext> options) :base(options) {}
        
        public virtual DbSet<MenuMasterModel> MenuMasterTraining { get; set; }
        public virtual DbSet<CustomerModel> Customers { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}







