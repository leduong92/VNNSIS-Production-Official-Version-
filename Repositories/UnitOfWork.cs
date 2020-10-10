
using System.Threading.Tasks;
using VNNSIS.Data;
using VNNSIS.Interface;

namespace VNNSIS.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private SqlDbContext _sqlDbContext;
        private PostgreDbContext _postGreDbContext;
        
        public UnitOfWork(SqlDbContext sqlDbContext, PostgreDbContext postGreDbContext)
        {
            _sqlDbContext = sqlDbContext;
            _postGreDbContext = postGreDbContext;
            Customers = new CustomerRepository(_sqlDbContext);
            MenuMasters = new MenuMasterRepository(_sqlDbContext, _postGreDbContext);        
            Accounts = new AccountRepository(_sqlDbContext);       
            InputDefectRepository = new InputDefectRepository(_sqlDbContext, _postGreDbContext);
        }
        public ICustomerRepository Customers {get;}
        public IMenuMasterRepository MenuMasters{get;}
        public IAccountRepository Accounts{get;}


        public IInputDefectRepository InputDefectRepository {get;}
        public async Task SQLSaveChanges()
        {
            await _sqlDbContext.SaveChangesAsync();
        }
        public async Task PostGreSaveChanges()
        {
            await _postGreDbContext.SaveChangesAsync();
        }
        public void Dispose()
        {
            if(_sqlDbContext != null)
                _sqlDbContext.Dispose();

            if(_postGreDbContext != null)
                _postGreDbContext.Dispose();
        }
    }
}