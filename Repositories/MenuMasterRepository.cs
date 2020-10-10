using System.Collections.Generic;
using System.Linq;
using System.Data;

using VNNSIS.Data;
using VNNSIS.Models;
using VNNSIS.Interface;

namespace VNNSIS.Repositories
{
    public class MenuMasterRepository : Repository<MenuMasterModel>, IMenuMasterRepository
    {
        private readonly SqlDbContext _sqlDbContext;
        private readonly PostgreDbContext _postGreDbContext;

        public MenuMasterRepository(SqlDbContext sqlDbContext, PostgreDbContext postGreDbContext) : base(sqlDbContext)
        {
            _sqlDbContext = sqlDbContext;
            _postGreDbContext = postGreDbContext;
        }

        public IEnumerable<MenuMasterModel> GetMenuMaster()
        {
            return _sqlDbContext.MenuMasterTraining.AsEnumerable();
        }

        public IEnumerable<MenuMasterModel> GetMenuMaster(string UserRole)
        {
            var result = _sqlDbContext.MenuMasterTraining.Where(m => m.UserRole == UserRole).ToList();
            return result;
        }

        //20200525
        public DataTable GetErrorMenu()
        {
            var query = string.Format("SELECT error_id, error_menu FROM td_sis_error_menu WHERE type = '1' order by error_id");
            DataTable dt = Core.DataProvider.ExcuteQuery(_postGreDbContext, query);
            return dt;
        }

        public DataTable GetSection()
        {
            var query = string.Format("SELECT section_id, section_name FROM td_sis_section_master order by section_id");
            DataTable dt = Core.DataProvider.ExcuteQuery(_postGreDbContext, query);
            return dt;
        }

        public DataTable GetLineBySection(string section)
        {
            var query = string.Format("SELECT line_no, section_id FROM td_sis_section_line WHERE section_id = '{0}' order by line_no", section);
            DataTable dt = Core.DataProvider.ExcuteQuery(_postGreDbContext, query);
            return dt;
        }

        public DataTable GetMachineByLine(string lineNo)
        {
            var query = string.Format("SELECT line_no, press_no, plc_m, plc_m1, plc_m2, ip, status, mold_type, trim_type FROM tm_postmachine_os " +
                                                                        "where line_no = '{0}' and press_no like 'D%' order by press_no", lineNo);
            DataTable dt = Core.DataProvider.ExcuteQuery(_postGreDbContext, query);
            return dt;
        }

        public DataTable GetUserInMachine(string lineNo)
        {
            var query = string.Format("SELECT a.line_no, a.press_no, a.plc_m, a.plc_m1, a.plc_m2, a.ip, a.status, a.mold_type, a.trim_type, b.start_date, b.start_time, b.start_user, b.start_date_01, b.start_time_01, b.start_user_01, c.menu_name " +
                                    "FROM tm_postmachine_os a left join td_job_prmold_os b on a.line_no = b.line_no and a.press_no = b.press_no " +
                                    "LEFT JOIN td_sis_cur_menu c on c.menu_id::int = a.status " +
                                    "where a.line_no = '{0}' and b.status = '1' and a.press_no like 'D%' order by a.press_no", lineNo);

            DataTable dt = Core.DataProvider.ExcuteQuery(_postGreDbContext, query);
            return dt;
        }

        //public int UpdateTmMachine(string lineNo, string pressNo, string values)
        //{
        //    var query = string.Format("UPDATE tm_postmachine_os set status = {0} where line_no = '{1}' and ({2})", values, lineNo, pressNo);
        //    var ret = Core.DataProvider.ExcuteNonQuery(_postGreDbContext, query);
        //    return ret;
        //}
        public int UpdateTmMachine(string lineNo, string pressNo, string values)
        {
            var query = string.Format("UPDATE tm_postmachine_os set status = {0} where line_no = '{1}' and press_no = '{2}'", values, lineNo, pressNo);
            var ret = Core.DataProvider.ExcuteNonQuery(_postGreDbContext, query);
            return ret;
        }

        public DataTable GetDataMachine(string lineNo, string pressNo)
        {
            var query = string.Format("SELECT * FROM tm_postmachine_os where line_no = '{0}' and ({1}) order by press_no", lineNo, pressNo);

            DataTable dt = Core.DataProvider.ExcuteQuery(_postGreDbContext, query);
            return dt;
        }

        public DataTable GetDataSavedTdsisCuringRecord(string lineNo, string pressNo, string status)
        {
            var query = string.Format("SELECT *  " +
                                    "FROM td_sis_cur_record " +
                                    "where line_no = '{0}' and ({1}) and type = '{2}' and active = '1'  order by press_no", lineNo, pressNo, status);

            DataTable dt = Core.DataProvider.ExcuteQuery(_postGreDbContext, query);
            return dt;
        }

        public int UpdateTdSisCuringRecord(string endDate, string endTime, string endUser, string lineNo, string pressNo, string active)
        {
            var query = string.Format("UPDATE td_sis_cur_record set end_date = '{0}', end_time = '{1}', end_user = '{2}', active = '{3}' where line_no = '{4}' and press_no = '{5}' and active = '1'", endDate, endTime, endUser, active, lineNo, pressNo);
            var ret = Core.DataProvider.ExcuteNonQuery(_postGreDbContext, query);
            return ret;
        }

        public int SaveTdsisCuringRecord(string values)
        {
            var query = string.Format("INSERT INTO td_sis_cur_record(line_no, press_no, type, start_date, start_time, start_user, end_date, end_time, end_user, reset_date, reset_time, reset_user, active, shift, error_type) VALUES {0}", values);
            var ret = Core.DataProvider.ExcuteNonQuery(_postGreDbContext, query);
            return ret;
        }

        //20200525
    }
}