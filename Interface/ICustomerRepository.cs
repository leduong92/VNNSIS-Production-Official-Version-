
using System.Collections.Generic;
using VNNSIS.Models;

namespace VNNSIS.Interface
{
    public interface ICustomerRepository : IRepository<CustomerModel>
    {
        IEnumerable<CustomerModel> GetBestCustomers(int amountOfCustomers);
    }
}