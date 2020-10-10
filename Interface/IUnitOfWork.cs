
using System;
using System.Threading.Tasks;

namespace VNNSIS.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository Customers {get;}
        IMenuMasterRepository MenuMasters {get;}
        IAccountRepository Accounts {get;}
        IInputDefectRepository InputDefectRepository {get;}

        Task SQLSaveChanges();
        Task PostGreSaveChanges();
    }
}