using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using Npgsql;
using VNNSIS.Common;
using VNNSIS.Controllers;
using VNNSIS.Models;
using VNNSIS.Hubs;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Data;
using VNNSIS.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace MvcIdentity.Controllers
{
    public class MoldWashingController : BaseController
    {
        private IHubContext<MoldsHub> _hubContext;
        private IMemoryCache _cache;
        private static DbConnection _connection;
        private MoldWashingStatus moldWashingStatus;
        private Object threadSafeCode = new Object();

        public MoldWashingController(PostgreDbContext context, IMemoryCache cache, IHubContext<MoldsHub> hubContext)
        {
            _hubContext = hubContext;
            _cache = cache;
            moldWashingStatus = new MoldWashingStatus();
            _connection = context.Database.GetDbConnection();
        }

        //for testing
        private static string GetConnectionString()
        {
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = "10.203.192.193",
                Database = "qimlive",
                Username = "qimlive",
                Password = "qimlive",
                Port = 5432,
                KeepAlive = 30
            };

            return csb.ConnectionString;
        }

        public void ListenForAlarmNotifications()
        {
            NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString());
            //NpgsqlConnection conn = new NpgsqlConnection(_connection.ConnectionString);
            conn.StateChange += conn_StateChange;
            conn.Open();
            var listenCommand = conn.CreateCommand();
            listenCommand.CommandText = $"listen notifytdcurmoldlog;";
            listenCommand.ExecuteNonQuery();
            conn.Notification += PostgresNotificationReceived;
            _hubContext.Clients.All.SendAsync("ReceiveMessage", this.GetMoldList());
            while (true)
            {
                conn.Wait();
            }
        }

        private void PostgresNotificationReceived(object sender, NpgsqlNotificationEventArgs e)
        {
            string actionName = e.Payload.ToString();
            string actionType = string.Empty;

            if (actionName.Contains("DELETE"))
            {
                actionType = "Delete";
            }
            if (actionName.Contains("UPDATE"))
            {
                actionType = "Update";
            }
            if (actionName.Contains("INSERT"))
            {
                actionType = "Insert";
            }
            _hubContext.Clients.All.SendAsync("ReceiveMessage", this.GetMoldList());
        }

        public string GetMoldList()
        {
            //var AlarmList = new List<MoldWashingModel>();
            this.moldWashingStatus.lstMoldWashing = new List<MoldWashingModel>();

            using (NpgsqlCommand sqlCmd = new NpgsqlCommand())
            {
                //        string query =  " SELECT DISTINCT a.line_no || '/'|| b.press_no as wc, trim(b.material_mark1) material_mark1, d.type_name,  b.mold, a.memo mold_job, COALESCE(c.status, '1') status, " +
                //" COALESCE(fn_change_date(a.entry_date), '') entry_date, " +
                //" COALESCE(fn_change_hour(a.entry_time), '') entry_time, " +
                //" COALESCE(c.receive_wash_date, '') receive_wash_date , " +
                //" COALESCE(fn_change_date(c.receive_wash_date), '') receive_date, " +
                //                        " COALESCE(fn_change_hour(c.receive_wash_time), '') receive_time, " +
                //                        " COALESCE(fn_change_hour(c.start_wash_time), '') start_time, " +
                //                        " COALESCE(CASE WHEN (fn_change_hour(c.start_wash_time)::INTERVAL  + (d.washing_time || 'minutes')::INTERVAL) = '55'::INTERVAL " +
                //" THEN '0' WHEN (fn_change_hour(c.start_wash_time)::INTERVAL  + (d.washing_time || 'minutes')::INTERVAL) = '90'::INTERVAL " +
                //" THEN '0' ELSE (fn_change_hour(c.start_wash_time)::INTERVAL  + (d.washing_time || 'minutes')::INTERVAL)::TEXT " +
                //" END, '') estimate_time, " +
                //" COALESCE(fn_change_hour(c.end_wash_time), '') end_time, " +
                //" COALESCE(fn_change_hour(c.delivery_time), '') delivery_time, " +
                //                        " COALESCE (CASE WHEN (CURRENT_TIME::TEXT >  (fn_change_hour(COALESCE(c.start_wash_time, ''))::INTERVAL + (d.washing_time ||'minutes')::INTERVAL)::TEXT) AND c.status = '7' " +
                //" THEN 'Delay' " +
                //" WHEN (COALESCE(fn_change_hour(c.end_wash_time), '')::TEXT <=  (fn_change_hour(COALESCE(c.start_wash_time, ''))::INTERVAL + (d.washing_time ||'minutes')::INTERVAL)::TEXT) " +
                //" THEN 'Good' " +
                //" WHEN (COALESCE(fn_change_hour(c.end_wash_time), '')::TEXT >  (fn_change_hour(COALESCE(c.start_wash_time, ''))::INTERVAL + (d.washing_time ||'minutes')::INTERVAL)::TEXT) " +
                //" THEN 'Not Good' ELSE 'Judging' END, '') Judgment " +
                //      " FROM td_job_product_os_log a INNER JOIN tr_cur_job_nbcs b ON a.job_order_no = b.job_order_no " +
                //                        " LEFT JOIN td_cur_mold_log c ON a.memo = c.mold_job " +
                //                        " LEFT JOIN td_cur_mold_washing_time d on  a.line_no = d.line_no " +
                //      //" WHERE  a.memo <> ''  AND (a.mold_status = '1' OR a.mold_status = '2') AND (c.status IS NULL OR (c.status > '5' AND c.status <'9')) " + //20200905 DUONG CMT OUT
                //                        //truong hop SX moi quet se hien len WEB luon
                //                        " WHERE  a.memo <> ''  AND (a.mold_status = '1' OR a.mold_status = '2') AND (c.status IS NULL OR (c.status > '5' AND c.status <'9')) OR ((a.mold_status = '1' OR a.mold_status = '2') AND c.status = '3') " +
                //      " ORDER BY receive_wash_date, receive_time desc  " ;
                string query = " SELECT DISTINCT a.line_no || '/'|| b.press_no as wc, trim(b.material_mark1) material_mark1, d.type_name,  b.mold, a.memo mold_job, " +
                      " CASE WHEN a.mold_status = 4 THEN '4' else COALESCE( c.status, '1') end status, " +
                      " COALESCE(fn_change_date(a.entry_date), '') entry_date, " +
                      " COALESCE(fn_change_hour(a.entry_time), '') entry_time, " +
                      " COALESCE(c.receive_wash_date, '') receive_wash_date , " +
                      " COALESCE(fn_change_date(c.receive_wash_date), '') receive_date, " +
                      " COALESCE(fn_change_hour(c.receive_wash_time), '') receive_time, " +
                      " COALESCE(fn_change_hour(c.start_wash_time), '') start_time, " +
                      " COALESCE(CASE WHEN (fn_change_hour(c.start_wash_time)::INTERVAL  + (d.washing_time || 'minutes')::INTERVAL) = '55'::INTERVAL " +
                      " THEN '0' WHEN (fn_change_hour(c.start_wash_time)::INTERVAL  + (d.washing_time || 'minutes')::INTERVAL) = '90'::INTERVAL " +
                      " THEN '0' ELSE (fn_change_hour(c.start_wash_time)::INTERVAL  + (d.washing_time || 'minutes')::INTERVAL)::TEXT " +
                      " END, '') estimate_time, " +
                      " COALESCE(fn_change_hour(c.end_wash_time), '') end_time, " +
                      " COALESCE(fn_change_hour(c.delivery_time), '') delivery_time, " +
                      " COALESCE (CASE WHEN (CURRENT_TIME::TEXT >  (fn_change_hour(COALESCE(c.start_wash_time, ''))::INTERVAL + (d.washing_time ||'minutes')::INTERVAL)::TEXT) AND c.status = '7' " +
                      " THEN 'Delay' " +
                      " WHEN (COALESCE(fn_change_hour(c.end_wash_time), '')::TEXT <=  (fn_change_hour(COALESCE(c.start_wash_time, ''))::INTERVAL + (d.washing_time ||'minutes')::INTERVAL)::TEXT) " +
                      " THEN 'Good' " +
                      " WHEN (COALESCE(fn_change_hour(c.end_wash_time), '')::TEXT >  (fn_change_hour(COALESCE(c.start_wash_time, ''))::INTERVAL + (d.washing_time ||'minutes')::INTERVAL)::TEXT) " +
                      " THEN 'Not Good' ELSE 'Judging' END, '') Judgment " +
                      " FROM td_job_product_os_log a INNER JOIN tr_cur_job_nbcs b ON a.job_order_no = b.job_order_no " +
                      " LEFT JOIN td_cur_mold_log c ON a.memo = c.mold_job " +
                      " LEFT JOIN td_cur_mold_washing_time d on  a.line_no = d.line_no " +
                      //" WHERE  a.memo <> ''  AND (a.mold_status = '1' OR a.mold_status = '2') AND (c.status IS NULL OR (c.status > '5' AND c.status <'9')) " + //20200905 DUONG CMT OUT
                      //truong hop SX moi quet se hien len WEB luon
                      " WHERE  c.status IS NOT NULL AND c.status <> '4' AND c.status <> '5' AND  a.mold_status <> '3' and a.memo <> ''  " +
                      " ORDER BY receive_wash_date, receive_time desc  ";

                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.CommandText = query;
                NpgsqlConnection conn = new NpgsqlConnection(GetConnectionString());
                //NpgsqlConnection conn = new NpgsqlConnection(_connection.ConnectionString);
                conn.Open();
                sqlCmd.Connection = conn;

                using (NpgsqlDataReader reader = sqlCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MoldWashingModel mold = new MoldWashingModel();
                        // you must fill  your model items
                        mold.wc = !reader.IsDBNull(0) ? reader.GetString(0).Trim() : string.Empty;
                        mold.material_mark1 = !reader.IsDBNull(1) ? reader.GetString(1).Trim() : string.Empty;
                        mold.type_name = !reader.IsDBNull(2) ? reader.GetString(2).Trim() : string.Empty;
                        mold.mold = !reader.IsDBNull(3) ? reader.GetString(3).Trim() : string.Empty;
                        mold.mold_job = !reader.IsDBNull(4) ? reader.GetString(4).Trim() : string.Empty;
                        mold.status = !reader.IsDBNull(5) ? reader.GetString(5).Trim() : string.Empty;
                        mold.entry_date = !reader.IsDBNull(6) ? reader.GetString(6).Trim() : string.Empty;
                        mold.entry_time = !reader.IsDBNull(7) ? reader.GetString(7).Trim() : string.Empty;
                        mold.receive_wash_date = !reader.IsDBNull(8) ? reader.GetString(8).Trim() : string.Empty;
                        mold.receive_date = !reader.IsDBNull(9) ? reader.GetString(9).Trim() : string.Empty;
                        mold.receive_time = !reader.IsDBNull(10) ? reader.GetString(10).Trim() : string.Empty;
                        mold.start_time = !reader.IsDBNull(11) ? reader.GetString(11).Trim() : string.Empty;
                        mold.estimate_time = !reader.IsDBNull(12) ? reader.GetString(12).Trim() : string.Empty;
                        mold.end_time = !reader.IsDBNull(13) ? reader.GetString(13).Trim() : string.Empty;
                        mold.delivery_time = !reader.IsDBNull(14) ? reader.GetString(14).Trim() : string.Empty;
                        mold.judgment = !reader.IsDBNull(15) ? reader.GetString(15).Trim() : string.Empty;
                        moldWashingStatus.lstMoldWashing.Add(mold);
                    }
                    reader.Close();
                    conn.Close();
                }
            }
            _cache.Set("MoldAlarm", SerializeObjectToJson(moldWashingStatus));
            return _cache.Get("MoldAlarm").ToString();
        }

        public String SerializeObjectToJson(Object alarms)
        {
            try
            {
                return JsonConvert.SerializeObject(alarms);
            }
            catch (Exception) { return null; }
        }

        private void conn_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            _hubContext.Clients.All.SendAsync("Current State: " + e.CurrentState.ToString() + " Original State: " + e.OriginalState.ToString(), "connection state changed");
        }

        [HttpGet]
        // public async Task<JsonResult> GetData()
        // {
        //     try
        //     {
        //         using (var client = new HttpClient())
        //         {
        //             client.BaseAddress = new Uri("http://localhost:8000/mold");
        //             //client.BaseAddress = new Uri("http://localhost:8000/mold");
        //             var responseTask = await client.GetAsync("mold");
        //            // responseTask.Wait();

        //             var result = responseTask;
        //             if (result.IsSuccessStatusCode)
        //             {
        //                 var data  = result.Content.ReadAsStringAsync().Result;
        //                 var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MoldWashingModel>>(data);
        //                 if (jsonData == null)
        //                 {
        //                     // Console.WriteLine("aaa");
        //                     return Json(new { jsonData = "", success = false } );
        //                 }
        //                 return Json(new { jsonData, success = true } );
        //             }
        //         }
        //     }
        //     catch (HttpRequestException e)
        //     {
        //         Console.WriteLine("\nException Caught!");
        //         Console.WriteLine("Message :{0} ",e.Message);
        //     }
        //     return Json("NG");
        // }
        public IActionResult Index()
        {
            ViewBag.UserID = HttpContext.Session.GetString(CommonConstants.USER_SEESION);
            return View();
        }
    }
}