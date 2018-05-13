using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net;
using System.Net.Http;

namespace LMW.Models.Filters
{
    public class LMWApiAuthorizationFilter : AuthorizationFilterAttribute
    {
        public static string BasicAuthUser = ConfigurationManager.AppSettings["BASIC_AUTH_USER"];
        public static string BasicAuthPass = ConfigurationManager.AppSettings["BASIC_AUTH_PASS"];

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            // get authorization header
            var header = actionContext.Request.Headers.Authorization;
            if (header != null &&
                header.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) &&
                String.IsNullOrWhiteSpace(header.Parameter) == false)
            {
                // get authorization info
                var isAuthorized = false;
                var encoding = Encoding.GetEncoding("UTF-8");
                var credential = encoding.GetString(Convert.FromBase64String(header.Parameter));
                if (credential.Split(':').Length > 1)
                {
                    // verify username and password
                    string username = credential.Split(':')[0];
                    string password = credential.Split(':')[1];
                    if (String.Compare(BasicAuthUser, username, false) == 0 &&
                        String.Compare(BasicAuthPass, password, false) == 0)
                    {
                        isAuthorized = true;
                    }
                }

                if (isAuthorized)
                {
                    base.OnAuthorization(actionContext);
                }
                else
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    var param = Newtonsoft.Json.JsonConvert.SerializeObject(
                        new
                        {
                            Error = 1,
                            Message = "Unauthorized requested!!!"
                        });
                    actionContext.Response.Content = new StringContent(param, Encoding.UTF8, "application/json");
                }
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
                var param = Newtonsoft.Json.JsonConvert.SerializeObject(
                    new
                    {
                        Error = 1,
                        Message = "Bad requested!!!!"
                    });
                actionContext.Response.Content = new StringContent(param, Encoding.UTF8, "application/json");
            }
        }
    }
}