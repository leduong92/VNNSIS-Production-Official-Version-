using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using VNNSIS.Common;

namespace VNNSIS.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext  filterContext)
        {
            var sessionUserID = HttpContext.Session.GetString(CommonConstants.USER_SEESION);
            if(sessionUserID == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new {controller = "Home", action = "Index"}));
            }
            base.OnActionExecuting(filterContext);
        }
    }
}