using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LMW.Models.Filters;
using LMW.Libs;

namespace LMW.Controllers.Admin
{
    public class AdminController : Controller
    {
        // GET: Admin
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult Index()
        {
            try
            {
                // Emailer.sendMail("locnp.saigon@gmail.com", "test", "test", "body");
            }
            catch (Exception ex)
            {
                Response.Write("Error: " + ex.ToString());
            }
            return View();
        }

        public ActionResult ErrorPage(string title, string message)
        {
            ViewBag.Title = title;
            ViewBag.Message = message;
            return View();
        }
    }
}