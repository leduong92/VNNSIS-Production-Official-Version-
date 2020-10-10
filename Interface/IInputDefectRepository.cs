
using System.Collections.Generic;
using System.Threading.Tasks;
using VNNSIS.Models;
using VNNSIS.ViewModels;

namespace VNNSIS.Interface
{
    public interface IInputDefectRepository 
    {
        Task<IEnumerable<InformationDataViewModel>> GetDataByJobNo(string jobOrderNo);
       

        Task<IEnumerable<ErrorListViewModel>> GetErrListByOperationCode(string jobOrderNo, string operationCode);
    
        int SaveAsync(string values); 
        Task<IEnumerable<OperationNumberViewModel>> GetOperationNumber(string jobOrderNo, string operationCode); 
    }
}