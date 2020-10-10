using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VNNSIS.ViewModels;
using VNNSIS.Models;
using VNNSIS.Repositories;
using VNNSIS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using VNNSIS.Common;

namespace VNNSIS.Controllers
{
    
    public class InputDefectController : BaseController
    {
        private readonly UnitOfWork _uow;
        public InputDefectController(SqlDbContext sqlDbContext, PostgreDbContext postGreDbContext)
        {
            _uow = new UnitOfWork(sqlDbContext, postGreDbContext);
        }

        public IActionResult Index()
        {
            ViewBag.UserID = HttpContext.Session.GetString(CommonConstants.USER_SEESION);
            return View();
        }
        
        [HttpGet]
        public async Task<JsonResult> GetDataByJobNo(string jobOrderNo) 
        {
            string Jobtag = jobOrderNo.Replace("JO","");
            var data = await _uow.InputDefectRepository.GetDataByJobNo(Jobtag);
            
            if (data.Count() == 0)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                                    .Select(m => m.ErrorMessage).ToArray()
                });
            }
            return Json(new { data, success = true } );
        }

        [HttpGet]
        public async Task<JsonResult> GetErrListByOperationCode(string jobOrderNo, string operationCode) 
        { 
            var data = await _uow.InputDefectRepository.GetErrListByOperationCode(jobOrderNo, operationCode);
            if (data.Count() == 0)
            {
                return Json(new
                {
                    success = false,
                    data = Enumerable.Empty<ErrorListViewModel>()
                });
            }

            var operationNumber = await _uow.InputDefectRepository.GetOperationNumber(jobOrderNo, operationCode);

            if (operationNumber.Count() == 0)
            {
                operationNumber.Count();
            }
            
            return Json(new { data, success = true, operationNumber });
        }

        [HttpPost]
        
        public JsonResult SaveData([FromBody]List<SisProErrorRecord> sisProErrorRecord) 
        {
            string value = "('{0}', {1}, '{2}', '{3}', {4}, '{5}', '{6}', {7}, {8}, {9}, {10}, '{11}', '{12}', '{13}', '{14}', '{15}', {16}, '{17}', {18}, '{19}', '{20}', N'{21}', N'{22}', {23}, N'{24}', '{25}', '{26}', '{27}', '{28}', '{29}', '{30}', '{31}', '{32}', '{33}', '{34}', '{35}', '{36}', '{37}'), ";
            string values = string.Empty;

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                                    .Select(m => m.ErrorMessage).ToArray()
                });
            }   

            foreach (var item in sisProErrorRecord)
            {
                values += string.Format(value, item.JobOrderNo, item.OperationNumber, item.FinishedGoodsCode, item.LotNo, item.CavityQty, item.LineNo, 
                item.RubberName, item.PlanCycle, item.PlanQty, item.UnitCost, item.UnitPrice, item.JobStartDate, item.JobEndDate, item.OperationStartDate, 
                item.OperationEndDate, item.MachineNo, item.OkQty, item.ProgressOperationCode, item.ProgressOperationSeq, item.ProgressOperationName, 
                item.ErrorID, item.ErrorName, item.ErrorNameJP, item.ErrorQty, item.Notes, item.EntryDate, item.EntryTime, item.EntryUser, item.UpdateDate, //28
                item.UpdateTime, item.UpdateUser, item.Status, item.ErrorNameEn, item.CuringDate, item.Department, item.Area, item.ProgramID, item.PressNo);
            }
            if (values.Length > 0)
            {
                values = values.Substring(0, values.LastIndexOf(','));
            }
            var result =   _uow.InputDefectRepository.SaveAsync(values);

            if (result <= 0)
            {
                //Core.DataProvider.SqlCommit(Core.commit.SQL_ROLLBACK);
                return Json(new
                {
                    success = false,
                    errors = ModelState.Keys.SelectMany(k => ModelState[k].Errors)
                                    .Select(m => m.ErrorMessage).ToArray()
                });
            }
            //Core.DataProvider.SqlCommit(Core.commit.SQL_COMMIT);
            //await _uow.SQLSaveChanges();
            return Json(new { success = true });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


