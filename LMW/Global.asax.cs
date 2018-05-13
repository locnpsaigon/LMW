using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Security;
using Newtonsoft.Json;
using LMW.Models.Auth;

namespace LMW
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Application["OnlineVisitors"] = 0;
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Application.Lock();
            Application["OnlineVisitors"] = (int)Application["OnlineVisitors"] + 1;
            Application.UnLock();
        }

        protected void Session_End(object sender, EventArgs e)
        {
            Application.Lock();
            Application["OnlineVisitors"] = (int)Application["OnlineVisitors"] - 1;
            Application.UnLock();
        }

        protected void Application_PostAuthenticateRequest()
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                LMWPrincipalSerialize model = JsonConvert.DeserializeObject<LMWPrincipalSerialize>(ticket.UserData);
                LMWPrincipal principle = new LMWPrincipal(ticket.Name);
                principle.UserId = model.UserId;
                principle.FullName = model.FullName;
                principle.Roles = model.Roles;
                HttpContext.Current.User = principle;
            }
        }
    }
}
