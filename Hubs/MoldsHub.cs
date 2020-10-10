
using System;
using System.Web;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Threading;
using MvcIdentity.Controllers;
using System.Threading.Tasks;
using VNNSIS.Data;

namespace VNNSIS.Hubs {
    public class MoldsHub : Hub
    {
        private Object threadSafeCode = new Object(); 
        private IMemoryCache _cache;
        private IHubContext<MoldsHub> _hubContext; 
        private readonly PostgreDbContext _postGreDbContext;
        public MoldsHub(PostgreDbContext postGreDbContext, IMemoryCache cache, IHubContext<MoldsHub> hubContext)
        {
            _cache = cache;
            _hubContext = hubContext;
            _postGreDbContext = postGreDbContext;
        }
        public async Task SendMessage()
        {
            MoldWashingController moldList = new MoldWashingController(_postGreDbContext, _cache, _hubContext);
             moldList.ListenForAlarmNotifications();
            string jsonMoldalarm = moldList.GetMoldList();
            _cache.Set("MoldAlarm", jsonMoldalarm);
            await Clients.All.SendAsync("ReceiveMessage", _cache.Get("MoldAlarm").ToString());
            // if (!_cache.TryGetValue("MoldAlarm", out string response))
            // {
            //     MoldWashingController moldList = new MoldWashingController(_postGreDbContext, _cache, _hubContext);
            //     moldList.ListenForAlarmNotifications();
            //     string jsonMoldalarm = moldList.GetMoldList();
            //     _cache.Set("MoldAlarm", jsonMoldalarm);
            //     await Clients.All.SendAsync("ReceiveMessage", _cache.Get("MoldAlarm").ToString());
            // }
            // else
            // {
            //     await Clients.All.SendAsync("ReceiveMessage", _cache.Get("MoldAlarm").ToString());
            // }
        }
    }
}