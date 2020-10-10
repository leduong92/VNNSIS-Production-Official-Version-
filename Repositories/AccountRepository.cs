using System.Linq;
using VNNSIS.Data;
using VNNSIS.Interface;

namespace VNNSIS.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly SqlDbContext _sqlDbContext;
        public AccountRepository(SqlDbContext sqlDbContext)
        {
            _sqlDbContext = sqlDbContext;
        }
        public string GetUserRole(string UserID)
        { 
            var result = (from x in _sqlDbContext.UserRoles
                                where x.UserId == UserID
                                select new {x.RoleId}).FirstOrDefault(); 
            if(result is null)
                return "";
			return result.RoleId;
        }     

        public string GetRoleName(string RoleID)
        { 
            var result = (from x in _sqlDbContext.Roles
                                where x.Id == RoleID
                                select new {x.Name}).FirstOrDefault(); 
            if(result is null)
                return "";
			return result.Name;
        }    
    }
}