
using System.Collections.Generic;
using System.Linq;
using VNNSIS.Data;
using VNNSIS.Models;
using VNNSIS.Interface;

namespace VNNSIS.Repositories
{
    public class CustomerRepository : Repository<CustomerModel>, ICustomerRepository
    {
        private readonly SqlDbContext _sqlDbContext;
        public CustomerRepository(SqlDbContext sqlDbContext) : base(sqlDbContext)
        {
            _sqlDbContext = sqlDbContext;
        }

        public IEnumerable<CustomerModel> GetBestCustomers(int amountOfCustomers)
        {
            return _sqlDbContext.Customers.OrderByDescending(x => x.FirstName).Take(amountOfCustomers).ToList();
        }
    }
}