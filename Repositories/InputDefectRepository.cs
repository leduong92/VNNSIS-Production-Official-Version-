
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using VNNSIS.Data;
using VNNSIS.Models;
using VNNSIS.Interface;
using VNNSIS.ViewModels;


namespace VNNSIS.Repositories
{
    public class InputDefectRepository :  IInputDefectRepository 
    {
        private readonly SqlDbContext _sqlDbContext;
        private readonly PostgreDbContext _postGreDbContext;
        public InputDefectRepository(SqlDbContext sqlDbContext, PostgreDbContext postGreDbContext) 
        {
            _sqlDbContext = sqlDbContext;
            _postGreDbContext = postGreDbContext;
        }
        public async Task<IEnumerable<OperationNumberViewModel>> GetOperationNumber(string jobOrderNo, string operationCode)
        {
            var result =  await _sqlDbContext.OperationNumberViewModels.FromSqlRaw($"select top 1 operationnumber from sis_pro_error_record  " +
                                        " where joborderno = {0} and progressoperationcode = {1} group by JobOrderNo, OperationNumber order by operationnumber desc", jobOrderNo, operationCode).ToListAsync();
            return result;
        }
        public async Task<IEnumerable<InformationDataViewModel>> GetDataByJobNo(string jobOrderNo)
        {
            var result = await _postGreDbContext.InformationDataViewModels.FromSqlRaw($"select a.job_order_no, a.order_type lot_no, a.finished_goods_code, b.operation_code, b.operation_name " +
                                                                            " , b.operation_sequence, a.line_no||'/'||a.press_no as line_no, a.material_mark1, a.rotation, a.curing_qty, a.cavity_qty,  a.curing_date::text, '' as order_date   " +
                                                                            "  from tr_cur_job_nbcs a inner join td_cur_progress_master b on a.finished_goods_code = b.finished_goods_code " +
                                                                            " where a.job_order_no = {0} order by b.operation_sequence", jobOrderNo).ToListAsync();
            return result;
        }
        public async Task<IEnumerable<ErrorListViewModel>> GetErrListByOperationCode(string jobOrderNo, string operationCode)
        {
            if(operationCode == "999")
            {
                return await _postGreDbContext.errorListViewModels.FromSqlRaw($"select '999'::text as operation_code , 999 as operation_sequence,'KIEM LUA LAI'::text as operation_name, ''::text as special_sign, b.error_id, b.department, b.area, b.program_id,c.error_name,c.error_name_jp,c.error_name_en,''::text as start_date, ''::text as end_date, ''::text as machine_no  " +
                                                                            " from si_pro_error_detail b " +
                                                                            " left join si_pro_error_master c on b.error_id = c.error_id  " +
                                                                            " where b.progress_operation_seq = 999 and b.location = 'OS1' and b.department = '60' and program_id = 'ID' order by b.order_id").ToListAsync();
            }
            else
            {
                return  await _postGreDbContext.errorListViewModels.FromSqlRaw($"select a.*, b.error_id, b.department, b.area, b.program_id,c.error_name,c.error_name_jp,c.error_name_en,d.start_date, d.end_date, d.machine_no " +
                                                                            " from td_cur_progress a left join si_pro_error_detail b on a.operation_sequence = b.progress_operation_seq " +
                                                                            " left join si_pro_error_master c on b.error_id = c.error_id  " +
                                                                            " left join td_cur_progress_check d on a.operation_code = d.operation_code  " +
                                                                            " where d.job_order_no = {0} and a.operation_code = {1} and b.location = 'OS1' and b.department = '60' and program_id = 'ID' order by b.order_id", jobOrderNo, operationCode).ToListAsync();
            }
            
        }
        public int SaveAsync(string values)
        {
            var commandText = string.Format("INSERT INTO sis_pro_error_record(JobOrderNo, OperationNumber, FinishedGoodsCode, LotNo, CavityQty, [LineNo], RubberName, PlanCycle, PlanQty, UnitCost, UnitPrice, JobStartDate, JobEndDate, OperationStartDate, OperationEndDate, MachineNo, OkQty, ProgressOperationCode, ProgressOperationSeq, ProgressOperationName, ErrorID, ErrorName, ErrorNameJP, ErrorQty, Notes, EntryDate, EntryTime, EntryUser, UpdateDate, UpdateTime, UpdateUser, [Status], ErrorNameEn, CuringDate, Department, Area, ProgramID, PressNo) VALUES {0}", values);
            //var result =  _sqlDbContext.Database.ExecuteSqlCommand(commandText); //duong change to use dataprovider
            var result =  Core.DataProvider.SqlExcuteNonQuery(_sqlDbContext, commandText);
            return result;
        }

      
    }
}