using Microsoft.EntityFrameworkCore;
using VNNSIS.Models;
using VNNSIS.ViewModels;

namespace VNNSIS.Data
{
    public class PostgreDbContext : DbContext
    {
        public DbSet<InformationDataViewModel> InformationDataViewModels {get; set;}
        public DbSet<ErrorListViewModel> errorListViewModels {get; set;}
        public DbSet<ErrorListModel> si_pro_error_master {get; set;}
        public DbSet<TrCurJobNbcs> tr_cur_job_nbcs {get; set;}
        public DbSet<MachineModel> tm_postmachine_os {get; set;}

        public PostgreDbContext (DbContextOptions<PostgreDbContext> options) : base(options)
        {}

    }
}