using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VNNSIS.Common;
using VNNSIS.Data;
using VNNSIS.Models;
using VNNSIS.Repositories;
using Microsoft.AspNetCore.Http;
using PlcEthernetCommunication;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System;
using System.Data;

namespace VNNSIS.Controllers
{
    public class HomeController : Controller
    {
        private static IMitsubishiPlc m_plc = null;
        private readonly UnitOfWork uow;

        public HomeController(SqlDbContext sqlDbContext, PostgreDbContext postGreDbContext)
        {
            uow = new UnitOfWork(sqlDbContext, postGreDbContext);
            m_plc = new UdpCommunication();
        }

        public IActionResult Index()
        {
            //khi redirect từ trang InputDefect thì xóa session
            HttpContext.Session.Remove(CommonConstants.USER_SEESION);
            return View();
        }

        public IActionResult Index1()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public JsonResult CheckUser(string MenuRole, string UserId)
        {
            string userID = UserId.Replace("*", "").Trim();
            string userRole = uow.Accounts.GetUserRole(userID);
            string roleName = uow.Accounts.GetRoleName(userRole);

            if (userRole.Trim() == "")  ////new
            {
                return Json("Không tìm thấy User");
            }
            if (userRole.Trim() == "Role04")
            {
                if (MenuRole == "Role03")
                {
                    return Json("Bạn không có quyền truy cập chức năng này.!");
                }
                HttpContext.Session.SetString(CommonConstants.USER_SEESION, userID);
                return Json("OK");
            }
            else if (Convert.ToInt16(userRole.Substring(4).ToString()) >= 3)
            {
                HttpContext.Session.SetString(CommonConstants.USER_SEESION, userID);
                return Json("OK");
            }
            else
            {
                return Json("Bạn không có quyền truy cập");
            }
        }

        //20200529
        [HttpGet]
        public JsonResult GetErrorMenu() //error menu show when you click on Card Reset Supervisor
        {
            var dt = uow.MenuMasters.GetErrorMenu();
            if (dt.Rows.Count <= 0)
            {
                return Json(new
                {
                    menu = "",
                    success = false,
                    error = "Không tìm thấy Error Menu GetErrorMenu()"
                });
            }
            var resData = JsonConvertDatatable(dt);
            return Json(new { menu = resData, success = true, error = "" });
        }

        [HttpGet]
        public JsonResult GetSection() //fetch section
        {
            var dt = uow.MenuMasters.GetSection();
            if (dt.Rows.Count <= 0)
            {
                return Json(new
                {
                    section = "",
                    success = false,
                    error = "Không tìm thấy Section GetSection()"
                });
            }
            var resData = JsonConvertDatatable(dt);
            return Json(new { section = resData, success = true, error = "" });
        }

        public string JsonConvertDatatable(DataTable dt)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            var serializer = JsonConvert.SerializeObject(rows);
            return serializer;
        }

        [HttpGet]
        public JsonResult GetLineBySection(string section)  //fetch line
        {
            if (section.Length <= 0)
            {
                return Json(new
                {
                    success = false,
                    error = "Không có Section được chọn GetLineBySection()"
                });
            }
            var dt = uow.MenuMasters.GetLineBySection(section);
            if (dt.Rows.Count <= 0)
            {
                return Json(new
                {
                    success = false,
                    error = "Không tìm thấy dữ liệu Line GetLineBySection()"
                });
            }
            var resData = JsonConvertDatatable(dt);
            return Json(new { sectionLine = resData, success = true });
        }

        [HttpGet]
        public JsonResult GetMachineByLine(string lineNo)
        {
            var lstMachineErr = new List<string>();
            var lstMachineErrPlc = new List<string>();
            int ret;
            
            var dataMachine = uow.MenuMasters.GetMachineByLine(lineNo);
            if (dataMachine.Rows.Count <= 0)
            {
                return Json(new { error = "Không tím thấy Line GetMachineByLine()", success = false });
            }
            var resdataMachine = JsonConvertDatatable(dataMachine);
            var dataUsr = uow.MenuMasters.GetUserInMachine(lineNo);
            if (dataUsr.Rows.Count <= 0)
            {
                return Json(new
                {
                    success = true,
                    dataMachine = resdataMachine,
                    dataUsr = dataUsr.Rows.Count,
                    dataErr = "",
                    plc = ""
                });
            }
            var resdataUsr = JsonConvertDatatable(dataUsr);

            var combindedStringPlcErr = string.Empty;
            var combindedString = string.Empty;

            for (int i = 0; i < dataMachine.Rows.Count; i++)
            {
                if (Convert.ToInt16(dataMachine.Rows[i]["status"].ToString()) > 0)
                {
                    try
                    {
                        //read error signal here
                        var rcv = string.Empty;
                        ret = m_plc.ReadDeviceA(dataMachine.Rows[i]["ip"].ToString(), 25884, "W", "62", 1, ref rcv);
                        if (rcv == "000E") //14
                        {
                            lstMachineErr.Add(dataMachine.Rows[i]["press_no"].ToString()); //its mean, this line has machine error
                        }
                        if (ret < 0)
                        {
                            lstMachineErrPlc.Add(dataMachine.Rows[i]["press_no"].ToString());
                            if (lstMachineErrPlc.Count() > 0)
                            {
                                combindedStringPlcErr = JsonConvert.SerializeObject(lstMachineErrPlc);
                                return Json(new { dataUsr = resdataUsr, dataMachine = resdataMachine, dataErr = combindedString, plc = combindedStringPlcErr, error = "", success = false });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        return Json(new { error = "Lỗi đọc PLC", plc = "",  success = false });
                    }
                }
            }
            
           
            if (lstMachineErr.Count() > 0) //response to client
            {
                combindedString = JsonConvert.SerializeObject(lstMachineErr);
                if (lstMachineErrPlc.Count() > 0)
                {
                    combindedStringPlcErr = JsonConvert.SerializeObject(lstMachineErrPlc);
                    return Json(new { dataUsr = resdataUsr, dataMachine = resdataMachine, dataErr = combindedString, plc = combindedStringPlcErr, error = "", success = true });
                }
                return Json(new { dataUsr = resdataUsr, dataMachine = resdataMachine, dataErr = combindedString, plc = "", error = "", success = true });
            }

            if (lstMachineErrPlc.Count() > 0)
            {
                combindedStringPlcErr = JsonConvert.SerializeObject(lstMachineErrPlc);
                return Json(new { dataUsr = resdataUsr, dataMachine = resdataMachine, dataErr = "", plc = combindedStringPlcErr, error = "", success = true });
            }
            //if no error machine response here
            return Json(new { dataUsr = resdataUsr, dataMachine = resdataMachine, dataErr = "", plc = "", success = true, error = "" });
        }

        public string GetShift(TimeSpan nowTime)
        {
            string shift = string.Empty;
            TimeSpan timeStartShiftA = new TimeSpan(05, 0, 0);
            TimeSpan timeEndShiftA = new TimeSpan(15, 0, 0); // make it easy
            TimeSpan timeStartShiftB = new TimeSpan(13, 0, 0);
            TimeSpan timeEndShiftB = new TimeSpan(23, 0, 0);

            if (nowTime >= timeStartShiftA && nowTime <= timeEndShiftA)
            {
                shift = "A";
            }
            else if (nowTime > timeStartShiftB && nowTime <= timeEndShiftB)
            {
                shift = "B";
            }
            else
            {
                shift = "0";
            }

            return shift;
        }

        [HttpPost]
        public JsonResult UpdAndInsrt([FromBody] List<DataMachine> data)
        {
            var ret = 0;
            var flagSave = 0;
            var typeMachine = string.Empty;
            string active = string.Empty;
            string shift = string.Empty;
            var lstError = new List<string>();
            var lstErrorWashing = new List<string>(); //20200901

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    type = 2,
                    success = false,
                    error = ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                                    .Select(m => m.ErrorMessage).ToArray()
                });
            }

            if (data.Count() <= 0)
            {
                return Json(new
                {
                    type = 2,
                    success = false,
                    error = "Không tìm thấy dữ liệu Line/Máy UpdAndInsrt()"
                });
            }

            shift = GetShift(DateTime.Now.TimeOfDay);

            if (shift.Equals("0"))
            {
                return Json(new
                {
                    type = 2,
                    success = false,
                    error = "Không lấy được dữ liệu Shift UpdAndInsrt()"
                });
            }

            var lineNo = data[0].LineNo.ToString().Trim();
            typeMachine = data[0].value.ToString().Trim(); //trang thai may khi chon duoi UI
            string user = data[0].users.ToString().Trim();
            string errType = data[0].errType.ToString().Trim();

            string valueInsrt = "('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}'),";

            string date = DateTime.Now.ToString("yyyyMMdd");
            string time = DateTime.Now.ToString("HHmmss");

            active = "1";
            //make data to save
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Count; i++)
            {
                sb.Append("(press_no = ");
                sb.Append("'" + data[i].PressNo + "') or");
            }
            String strPrs = sb.ToString().Substring(0, sb.ToString().Length - 2);

            //check and update trang thai ve running
            //1. get data
            var dataSaved = uow.MenuMasters.GetDataSavedTdsisCuringRecord(lineNo, strPrs, typeMachine);
            if (dataSaved.Rows.Count > 0)
            {
                flagSave = 1;
                typeMachine = "1"; //neu da co du lieu se update ve trang thay == 1. running
                active = "2"; //trang thai khi di ve
            }
            //end
            var dataMachine = uow.MenuMasters.GetDataMachine(lineNo, strPrs);
            if (dataMachine.Rows.Count <= 0)
            {
                return Json(new { type = 2, error = "Không tím thấy Line UpdAndInsrt() ", success = false });
            }
            
            string values = string.Empty;
            if (flagSave == 0) //insert
            {
                //send to PLC tin hieu luc chon di here
                for (int i = 0; i < dataMachine.Rows.Count; i++)
                {
                    //read W60 thi moi send tin hieu, nghia la da nhan Pre-heat
                    values = string.Empty;
                    //read error signal here
                    var rcv = string.Empty;
                    ret = m_plc.ReadDeviceA(dataMachine.Rows[i]["ip"].ToString(), 25884, "W", "60", 1, ref rcv);
                    if (ret < 0) //neu doc PLC bi loi thi pass qua may do
                    {
                        return Json(new { type = 2, error = "Không đọc được tín hiệu PLC UpdAndInsrt() W60", success = false });
                    }
                    //20200901 doc tin hieu rua khuon, neu dang rua khuon thi khong cho chon tren website
                    var rcvWashing = string.Empty;
                    ret = m_plc.ReadDeviceA(dataMachine.Rows[i]["ip"].ToString(), 25884, "W", "100", 1, ref rcvWashing);
                    if (ret < 0) //neu doc PLC bi loi thi pass qua may do
                    {
                        return Json(new { type = 2, error = "Không đọc được tín hiệu PLC UpdAndInsrt() W100", success = false });
                    }
                    //20200901 doc tin hieu rua khuon, neu dang rua khuon thi khong cho chon tren website
                    if (Convert.ToInt16(rcv) == 1) //Pre-heat
                    {
                        try
                        {
                            //"0A" = 10; "0B" = 11, "0C" = 12, "0D" = 13, "0E" = 14
                            var addr = string.Empty;
                            if (typeMachine.Length > 1)
                            {
                                switch (typeMachine)
                                {
                                    case "10":
                                        addr = string.Concat("00", "0A");
                                        break;
                                    case "11":
                                        addr = string.Concat("00", "0B");
                                        break;
                                    case "12":
                                        addr = string.Concat("00", "0C");
                                        break;
                                    case "13":
                                        addr = string.Concat("00", "0D");
                                        break;
                                }
                            }
                            else
                            {
                                addr = string.Concat("000", typeMachine);
                            }

                            for (int j = 0; j < data.Count; j++)
                            {
                                if (dataMachine.Rows[i]["press_no"].ToString() == data[j].PressNo.ToString())
                                {
                                    user = data[j].users.ToString();
                                }
                            }
                            values = string.Format(valueInsrt, lineNo, dataMachine.Rows[i]["press_no"].ToString(), typeMachine, date, time, user, "", "", "", "", "", "", active, shift, errType);
                            if (values.Length > 0)
                            {
                                values = values.Substring(0, values.LastIndexOf(','));
                            }
                            ret = uow.MenuMasters.SaveTdsisCuringRecord(values);
                            if (ret <= 0)
                            {
                                return Json(new
                                {
                                    type = 2,
                                    success = false,
                                    error = "Không thêm được dữ liệu td_sis_curing_record SaveTdsisCuringRecord()"
                                });
                            }

                            ret = uow.MenuMasters.UpdateTmMachine(lineNo, dataMachine.Rows[i]["press_no"].ToString(), typeMachine);
                            if (ret <= 0)
                            {
                                return Json(new
                                {
                                    type = 2,
                                    success = false,
                                    error = "Không cập nhật được tm_postmachine_os UpdAndInsrt()"
                                });
                            }
                             
                            ret = m_plc.WriteDeviceA(dataMachine.Rows[i]["ip"].ToString(), 25884, "W", "62", 1, addr);
                            if (ret < 0)
                            {
                                return Json(new { type = 2, error = "Không gửi được tín hiệu PLC UpdAndInsrt() W62 ", success = false });
                            }
                        }
                        catch (Exception ex)
                        {
                            return Json(new { type = 2, error = ex, success = false }); ;
                        }
                    }
                    else
                    {
                        lstError.Add(dataMachine.Rows[i]["press_no"].ToString());
                    }
                    if (Convert.ToInt16(rcvWashing) == 3) //washing
                    {
                        lstErrorWashing.Add(dataMachine.Rows[i]["press_no"].ToString());
                    }
                }
                if (lstError.Count() > 0)
                {
                    var combindedString = JsonConvert.SerializeObject(lstError);
                    if (lstErrorWashing.Count() > 0)
                    {
                        var combindedStringWashing = JsonConvert.SerializeObject(lstErrorWashing);
                        return Json(new {type = 1, error = combindedStringWashing, errWashing = combindedStringWashing, success = false });
                    }
                    return Json(new { type = 1, error = combindedString, errWashing = "", success = false });
                }

                if (lstErrorWashing.Count() > 0)
                {
                    var combindedStringWashing = JsonConvert.SerializeObject(lstErrorWashing);
                    return Json(new { type = 1, error = "", errWashing = combindedStringWashing, success = false });
                }
            }
            else  //update
            {
                //2. update td_sis_curing_record
                //send tin hieu hoan thanh khi di ve
                //lay user de update hoan thanh
                for (int i = 0; i < dataSaved.Rows.Count; i++)
                {
                    var rcv = string.Empty;
                    ret = m_plc.ReadDeviceA(dataMachine.Rows[i]["ip"].ToString(), 25884, "W", "62", 1, ref rcv);
                    if (ret < 0) //neu doc PLC bi loi thi pass qua may do
                    {
                        return Json(new { type = 2, error = "Không đọc được tín hiệu PLC máy " + dataMachine.Rows[i]["press_no"].ToString(), success = false });
                    }
                }
                for (int i = 0; i < dataSaved.Rows.Count; i++)
                {
                    try
                    {
                        //end lay user de update hoan thanh
                        user = dataSaved.Rows[i]["start_user"].ToString().Trim();
                        ret = uow.MenuMasters.UpdateTdSisCuringRecord(date, time, user, lineNo, dataSaved.Rows[i]["press_no"].ToString().Trim(), active);
                        if (ret <= 0)
                        {
                            return Json(new
                            {
                                type = 2,
                                success = false,
                                error = "Không cập nhật được td_sis_curing_record UpdAndInsrt()"
                            });
                        }
                        ret = uow.MenuMasters.UpdateTmMachine(lineNo, dataSaved.Rows[i]["press_no"].ToString().Trim(), typeMachine);
                        if (ret <= 0)
                        {
                            return Json(new
                            {
                                type = 2,
                                success = false,
                                error = "Không cập nhật được tm_postmachine_os UpdAndInsrt()"
                            });
                        }
                        ret = m_plc.WriteDeviceA(dataMachine.Rows[i]["ip"].ToString(), 25884, "W", "62", 1, "0000"); //W62 = 0
                        if (ret < 0)
                        {
                            return Json(new { type = 2, error = "Không tím gửi được tín hiệu PLC Upd UpdAndInsrt() W62", success = false });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { type = 2, error = ex, success = false });
                    }
                }
                //end send tin hieu hoan thanh khi di ve
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult ResetMachine([FromBody] List<DataMachine> data)
        {
            var lstMachine = new List<string>();
            var typeMachine = string.Empty;
            string active = string.Empty;
            var ret = 0;
            string shift = string.Empty;

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    error = ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                                    .Select(m => m.ErrorMessage).ToArray()
                });
            }
            if (data.Count() <= 0)
            {
                return Json(new
                {
                    success = false,
                    error = "Không tìm thấy dữ liệu Line/Máy reset ResetMachine()"
                });
            }

            var lineNo = data[0].LineNo.ToString().Trim();
            typeMachine = data[0].value.ToString().Trim(); //trang thai may khi chon duoi UI
            string user = data[0].users.ToString().Trim();

            if (user.Substring(0, 1) == "¤")
            {
                user = user.Substring(1);
            }

            string errType = data[0].errType.ToString().Trim();

            string valueInsrt = "('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}'),";

            string date = DateTime.Now.ToString("yyyyMMdd");
            string time = DateTime.Now.ToString("HHmmss");
            active = "2";

            shift = GetShift(DateTime.Now.TimeOfDay);

            if (shift.Equals("0"))
            {
                return Json(new
                {
                    success = false,
                    error = "Không lấy được dữ liệu Shift ResetMachine()."
                });
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Count; i++)
            {
                sb.Append("(press_no = ");
                sb.Append("'" + data[i].PressNo + "') or");
            }
            String strPrs = sb.ToString().Substring(0, sb.ToString().Length - 2);

            var dataMachine = uow.MenuMasters.GetDataMachine(lineNo, strPrs);
            if (dataMachine.Rows.Count <= 0)
            {
                return Json(new { error = "Không tím thấy line reset ResetMachine()", success = false });
            }

            string values = string.Empty;
            for (int i = 0; i < dataMachine.Rows.Count; i++)
            {
                try
                {
                    values = string.Empty;
                    //insert record here
                    values = string.Format(valueInsrt, lineNo, dataMachine.Rows[i]["press_no"].ToString(), typeMachine, "", "", "", "", "", "", date, time, user, active, shift, errType);
                    if (values.Length > 0)
                    {
                        values = values.Substring(0, values.LastIndexOf(','));
                    }
                    ret = uow.MenuMasters.SaveTdsisCuringRecord(values);
                    if (ret <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            error = "Không thêm được dữ liệu td_sis_curing_record ResetMachine()."
                        });
                    }
                    if (Convert.ToInt16(dataMachine.Rows[i]["status"].ToString()) > 0)
                    {
                        ret = m_plc.WriteDeviceA(dataMachine.Rows[i]["ip"].ToString(), 25884, "W", "62", 1, "0000");

                        if (ret < 0)
                        {
                            return Json(new { error = "Không tím gửi được tín hiệu PLC ResetMachine().", success = false });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex, success = false }); ;
                }
            }
            return Json(new { success = true, error = "" });
        }
        //end 20200525
    }
}