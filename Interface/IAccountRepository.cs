
namespace VNNSIS.Interface
{
    public interface  IAccountRepository 
    {
        string GetUserRole(string UserID);
        string GetRoleName(string RoleID);
    }
}