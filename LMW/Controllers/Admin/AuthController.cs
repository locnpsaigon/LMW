using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using LMW.Models.Database;
using LMW.Models.Auth;
using LMW.Models.Filters;
using LMW.Libs;

namespace LMW.Controllers.Admin
{
    public class AuthController : Controller
    {
        DBContext db = new DBContext();

        // GET: Authentication
        public ActionResult SignIn()
        {
            
            if (User.Identity.IsAuthenticated == false)
            {
                var model = new SignInModel();
                return View(model);
            }
            else
            {
                return RedirectToAction("SignOut");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SignIn(SignInModel model)
        {
            var eventLogs = "";
            try
            {
                if (ModelState.IsValid)
                {
                    eventLogs += "Login, user:" + model.UserName;

                    // get user info
                    var user = db.Users.Where(r => String.Compare(r.UserName, model.UserName, true) == 0).FirstOrDefault();
                    if (user != null)
                    {
                        if (SaltedHash.Verify(user.Salt, user.Password, model.Password))
                        {
                            // Save authen info
                            var principle = new LMWPrincipalSerialize();
                            principle.UserId = user.UserId;
                            principle.FullName = user.FullName;
                            principle.Roles = (from u in db.Users
                                               join ur in db.UserRoles on u.UserId equals ur.UserId into join1
                                               from j1 in join1.DefaultIfEmpty()
                                               join r in db.Roles on j1.RoleId equals r.RoleId into join2
                                               from j2 in join2.DefaultIfEmpty()
                                               where u.UserId == user.UserId
                                               select j2.RoleName).ToArray();

                            // Save authen cookies
                            var ticket = new FormsAuthenticationTicket(
                                1,
                                user.UserName,
                                DateTime.Now,
                                DateTime.Now.AddDays(7),
                                model.RememberMe,
                                JsonConvert.SerializeObject(principle));
                            var ticketEncrypted = FormsAuthentication.Encrypt(ticket);
                            var authenCookie = new HttpCookie(FormsAuthentication.FormsCookieName, ticketEncrypted);
                            Response.Cookies.Add(authenCookie);

                            eventLogs += ", success";

                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            eventLogs += ", fail, wrong password";

                            ModelState.AddModelError("", "Sai mật khẩu");
                        }
                    }
                    else
                    {
                        eventLogs += ", fail, invalid user";

                        ModelState.AddModelError("", "Sai tên tài khoản");
                    }
                }
            }
            catch (Exception ex)
            {
                // set error
                ModelState.AddModelError("", ex.Message);

                // write error log
                eventLogs += "error: " + ex.Message;
            }
            finally
            {
                // Write event log
                if (!string.IsNullOrWhiteSpace(eventLogs))
                {
                    EventWriter.WriteEventLog(eventLogs);
                }
            }

            return View(model);
        }

        // GET: Authentication
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("SignIn");
        }

        [Authorize]
        public ActionResult ChangePass()
        {
            var model = new ChangePassModel();
            try
            {
                var user = db.Users.Where(r => r.UserName.Equals(User.Identity.Name)).FirstOrDefault();
                if (user != null)
                {
                    model.UserId = user.UserId;
                    model.UserName = user.UserName;
                }
                else
                {
                    return RedirectToAction("SignOut");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                EventWriter.WriteEventLog("AuthController - ChangePass: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult ChangePass(ChangePassModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get user
                    var user = db.Users.Where(r => r.UserId == model.UserId).FirstOrDefault();
                    if (user != null)
                    {
                        // Validate current password
                        if (SaltedHash.Verify(user.Salt, user.Password, model.PasswordCurrent))
                        {
                            // Change password
                            var sh = new SaltedHash(model.PasswordNew);
                            user.Salt = sh.Salt;
                            user.Password = sh.Hash;
                            db.SaveChanges();
                            return RedirectToAction("ChangePassSuccess", "Auth");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
                        }
                    }
                    else
                    {
                        return RedirectToAction("SignOut");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                EventWriter.WriteEventLog("AuthController - ChangePass: " + ex.ToString());
            }
            return View(model);
        }

        [Authorize]
        public ActionResult ChangePassSuccess()
        {
            return View();
        }
    }
}