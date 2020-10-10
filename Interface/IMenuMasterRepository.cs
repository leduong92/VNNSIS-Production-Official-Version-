
using System.Collections.Generic;
using System.Data;
using VNNSIS.Models;

namespace VNNSIS.Interface
{
    public interface IMenuMasterRepository : IRepository<MenuMasterModel>
    {
        IEnumerable<MenuMasterModel> GetMenuMaster();
        IEnumerable<MenuMasterModel> GetMenuMaster(string UserRole);

        //20200525
        DataTable GetErrorMenu();
        DataTable GetSection(); //20200529
        DataTable GetLineBySection(string section);
        DataTable GetMachineByLine(string lineNo);
        DataTable GetUserInMachine(string lineNo);
        DataTable GetDataMachine(string lineNo, string pressNo);
        int UpdateTmMachine(string lineNo, string pressNo, string values); 
        int SaveTdsisCuringRecord(string values);
        DataTable GetDataSavedTdsisCuringRecord(string lineNo, string pressNo, string status);
        int UpdateTdSisCuringRecord(string endDate, string endTime, string endUser, string lineNo, string pressNo, string active);

        //end 20200525

    }
}