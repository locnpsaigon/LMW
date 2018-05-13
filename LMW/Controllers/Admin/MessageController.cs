using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using LMW.Libs;
using LMW.Models.Admin;
using LMW.Models.Filters;
using LMW.Models.Database;

namespace LMW.Controllers.Admin
{
    public class MessageController : Controller
    {
        DBContext db = new DBContext();

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult ListMessages(string date1, string date2, string filter = "", int status = 0, int page = 1)
        {
            try
            {
                // Validate page 
                page = page > 0 ? page : 1;

                // Get default pagesize
                var pageSize = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"]);

                // Get search date range
                var provider = CultureInfo.InvariantCulture;
                var d1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var d2 = DateTime.Now;
                if (!String.IsNullOrWhiteSpace(date1) && !String.IsNullOrWhiteSpace(date2))
                {
                    DateTime.TryParseExact(date1, "dd/MM/yyyy", provider, DateTimeStyles.None, out d1);
                    DateTime.TryParseExact(date2 + " 23:59:59", "dd/MM/yyyy HH:mm:ss", provider, DateTimeStyles.None, out d2);
                }

                // Query data
                var pDate1 = new SqlParameter("date1", d1);
                var pDate2 = new SqlParameter("date2", d2);
                var pFilter = new SqlParameter("filter", filter);
                var pStatus = new SqlParameter("status", status);
                var pPageIndex = new SqlParameter("pageIndex", page);
                var pPageSize = new SqlParameter("pageSize", pageSize);
                var pItemCount = new SqlParameter("itemCount", System.Data.SqlDbType.Int, 4);

                pPageIndex.Direction = System.Data.ParameterDirection.InputOutput;
                pItemCount.Direction = System.Data.ParameterDirection.Output;

                var messages = db.Database.SqlQuery<Message>(@"EXEC [dbo].[usp_searchMessages] 
                                @date1,
                                @date2,
                                @filter, 
                                @status, 
                                @pageIndex OUT, 
                                @pageSize, 
                                @itemCount OUT",
                                pDate1,
                                pDate2,
                                pFilter,
                                pStatus,
                                pPageIndex,
                                pPageSize,
                                pItemCount).ToList();

                // Create view model
                var totalItems = (int)pItemCount.Value;
                var pager = new Pager(totalItems, (int)pPageIndex.Value, pageSize);
                var statusOptions = new SelectListItem[]
                {
                    new SelectListItem { Value = "0", Text = "--Tất cả--", Selected = (status == 0) },
                    new SelectListItem { Value = "1", Text = "Chưa đọc", Selected = (status == (int)Message.Status.UNREAD) },
                    new SelectListItem { Value = "2", Text = "Đã đọc", Selected = (status == (int)Message.Status.READ) },
                    new SelectListItem { Value = "3", Text = "Đã xóa", Selected = (status == (int)Message.Status.REMOVED) },
                };

                var model = new ListMessageModel
                {
                    Date1 = d1,
                    Date2 = d2,
                    Filter = filter,
                    Status = status,
                    StatusOptions = statusOptions,
                    Items = messages,
                    Pager = pager
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("MessageController - ListMessages: " + ex.ToString());
            }
            return View();
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateMessage(string phone)
        {
            var model = new CreateMessageModel();
            model.Phones = phone;
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateMessage(CreateMessageModel model)
        {
            var eventLogs = "";

            // Start transaction
            var transaction = db.Database.BeginTransaction();
            try
            {
                if (ModelState.IsValid)
                {
                    var arrPhones = model.Phones.Split(',');
                    foreach(var phone in arrPhones)
                    {
                        var message = new Message();
                        message.date = DateTime.Now;
                        message.phone = phone;
                        message.title = model.Title;
                        message.message = model.Message;
                        message.status = (int)Message.Status.UNREAD;
                        db.Messages.Add(message);
                    }
                    db.SaveChanges();
                    transaction.Commit();

                    return RedirectToAction("ListMessages");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                // Rollback transaction
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                // Update event logs
                eventLogs += "MessageController - CreateMessage: " + ex.Message;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(eventLogs))
                {
                    EventWriter.WriteEventLog(eventLogs);
                }
            }
            return View(model);
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditMessage(int id)
        {
            var model = new EditMessageModel();
            try
            {
                var message = db.Messages.Where(r => r.messageId == id).FirstOrDefault();
                if (message != null)
                {
                    model.MessageId = message.messageId;
                    model.Title = message.title;
                    model.Message = message.message;
                }
                else
                {
                    ModelState.AddModelError("", "Thông báo mã #" + id + " không tồn tại trong hệ thống");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("MessageController - EditMessage: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditMessage(EditMessageModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // get message
                    var message = db.Messages.Where(r => r.messageId == model.MessageId).FirstOrDefault();
                    if (message != null)
                    {
                        message.title = model.Title;
                        message.message = model.Message;
                        message.status = (int)Message.Status.UNREAD;
                        db.SaveChanges();
                        return RedirectToAction("ListMessages");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("MessageController - EditMessage: " + ex.ToString());
            }
            return View(model);
        }
        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public JsonResult DeleteMessage(int id)
        {
            try
            {
                // Get message 
                var message = db.Messages.Where(r => r.messageId == id).FirstOrDefault();
                if (message != null)
                {
                    // Remove group
                    db.Messages.Remove(message);
                    db.SaveChanges();

                    // Success
                    return Json(new
                    {
                        Success = true,
                        Message = "Xóa thông báo thành công!"
                    });
                }
                else
                {
                    // Fail
                    return Json(new
                    {
                        Success = false,
                        Message = "Thông báo #" + id + " không tồn tại trong hệ thống!"
                    });
                }
            }
            catch (Exception ex)
            {
                // Write error logs
                EventWriter.WriteEventLog("MessageController - DeleteMessage: " + ex.ToString());

                // Return error
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}