using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LMW.Libs;
using LMW.Models.Filters;
using LMW.Models.Database;
using LMW.Models.Admin;

namespace LMW.Controllers.Admin
{
    public class SaleController : Controller
    {
        DBContext db = new DBContext();

        #region Order

        [Authorize]
        public ActionResult ListOrders(string date1, string date2, string filter = "", int status = 0, int page = 1)
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

                var orders = db.Database.SqlQuery<Order>(@"EXEC [dbo].[usp_searchOrders]
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
                    new SelectListItem { Value = "1", Text = "Đang Mở", Selected = (status == (int)Order.Status.STATUS_OPENED) },
                    new SelectListItem { Value = "2", Text = "Đã tiếp nhận", Selected = (status == (int)Order.Status.STATUS_PENDIND) },
                    new SelectListItem { Value = "3", Text = "Đang xử lý lý", Selected = (status == (int)Order.Status.STATUS_PROCESSING) },
                    new SelectListItem { Value = "4", Text = "Hoàn tất", Selected = (status == (int)Order.Status.STATUS_FINISHED) },
                    new SelectListItem { Value = "5", Text = "Đã đóng", Selected = (status == (int)Order.Status.STATUS_CLOSED) }
                };
                var model = new ListOrderModel
                {
                    Date1 = d1,
                    Date2 = d2,
                    Filter = filter,
                    Status = status,
                    StatusOptions = statusOptions,
                    Items = orders,
                    Pager = pager
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SaleController - ListOrders: " + ex.ToString());
            }
            return View();
        }

        [Authorize]
        public ActionResult EditOrder(int id)
        {
            var model = new EditOrderModel();
            try
            {
                // Get order
                var order = db.Orders.Where(r => r.orderId == id).FirstOrDefault();
                if (order != null)
                {
                    // Get order details
                    var orderDetails = db.OrderDetails.Where(r => r.orderId == id).ToList();
                    var statusOptions = new SelectListItem[]
                                        {
                                            new SelectListItem { Value = "1", Text = "Đang Mở", Selected = (order.status == (int)Order.Status.STATUS_OPENED) },
                                            new SelectListItem { Value = "2", Text = "Đã tiếp nhận", Selected = (order.status == (int)Order.Status.STATUS_PENDIND) },
                                            new SelectListItem { Value = "3", Text = "Đang xử lý", Selected = (order.status == (int)Order.Status.STATUS_PROCESSING) },
                                            new SelectListItem { Value = "4", Text = "Hoàn tất", Selected = (order.status == (int)Order.Status.STATUS_FINISHED) },
                                            new SelectListItem { Value = "5", Text = "Đã đóng", Selected = (order.status == (int)Order.Status.STATUS_CLOSED) }
                                        };

                    // Create view model
                    model.OrderId = order.orderId;
                    model.OrderDate = order.date.ToString("dd/MM/yyyy");
                    model.Phone = order.phone;
                    model.FullName = order.fullname;
                    model.Address = order.address;
                    model.Amount = order.amount;
                    model.Status = order.status;
                    model.Note = order.note;
                    model.Title = order.title;
                    model.StatusOptions = statusOptions;
                    model.OrderDetails = orderDetails;
                }
                else
                {
                    ModelState.AddModelError("", "Đơn hàng mã #" + id + " không tồn tại trong hệ thống!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SaleController - EditOrder: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult EditOrder(EditOrderModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // get order
                    var order = db.Orders.Where(r => r.orderId == model.OrderId).FirstOrDefault();
                    if (order != null)
                    {
                        // update order
                        order.phone = model.Phone;
                        order.fullname = model.FullName;
                        order.address = model.Address;
                        order.note = model.Note;
                        order.status = model.Status;

                        db.SaveChanges();

                        return RedirectToAction("ListOrders");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Đơn hàng mã #" + model.OrderId + " không tồn tại trong hệ thống!");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SaleController - EditOrder: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult AddOrderDetails(int orderId, int itemId, int quantity)
        {
            try
            {
                // get order
                var order = db.Orders.Where(r => r.orderId == orderId).FirstOrDefault();
                if (order != null)
                {
                    // get service details
                    var serviceDetails = db.CarServiceDetails.Where(r => r.itemId == itemId).FirstOrDefault();
                    if (serviceDetails != null)
                    {
                        // get service 
                        var service = db.CarServices.Where(r => r.serviceId == serviceDetails.serviceId).FirstOrDefault();
                        if (service != null)
                        {
                            // get group
                            var group = db.CarServiceGroups.Where(r => r.groupId == service.groupId).FirstOrDefault();
                            if (group !=  null)
                            {
                                var orderDetails = new OrderDetails();
                                orderDetails.orderId = orderId;
                                orderDetails.itemId = orderId;
                                orderDetails.itemName = serviceDetails.itemName;
                                orderDetails.serviceId = service.serviceId;
                                orderDetails.serviceName = service.serviceName;
                                orderDetails.groupId = service.groupId;
                                orderDetails.groupName = group.groupName;
                                orderDetails.unit = serviceDetails.unit;
                                orderDetails.quantity = quantity;
                                orderDetails.price = serviceDetails.price;
                                orderDetails.priceOriginal = serviceDetails.priceOriginal;

                                db.OrderDetails.Add(orderDetails);
                                db.SaveChanges();

                                return Json(new
                                {
                                    Success = true,
                                    Message = "Thêm chi tiết đơn hàng thành công!"
                                });
                            }
                            else
                            {
                                return Json(new
                                {
                                    Success = false,
                                    Message = "Chi tiết dịch vụ không hợp lệ!"
                                });
                            }
                        }
                        else
                        {
                            return Json(new
                            {
                                Success = false,
                                Message = "Chi tiết dịch vụ không hợp lệ!"
                            });
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            Success = false,
                            Message = "Không tìm thấy chi tiết dịch vụ!"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn hàng!"
                    });
                }
            }
            catch (Exception ex)
            {
                // Write error logs
                EventWriter.WriteEventLog("SaleController - AddOrderDetails: " + ex.ToString());

                // Return error
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public JsonResult DeleteOrder(int id)
        {
            DbContextTransaction transaction = null;
            try
            {
                // Start transaction
                transaction = db.Database.BeginTransaction();

                // Get order
                var order = db.Orders.Where(r => r.orderId == id).FirstOrDefault();
                if (order != null)
                {
                    // Remove order details
                    db.OrderDetails.RemoveRange(db.OrderDetails.Where(r => r.orderId == order.orderId).ToList());

                    // Remove order
                    db.Orders.Remove(order);

                    db.SaveChanges();
                    transaction.Commit();

                    // Success
                    return Json(new
                    {
                        Success = true,
                        Message = "Xóa đơn hàng thành công!"
                    });
                }
                else
                {
                    // Fail
                    return Json(new
                    {
                        Success = false,
                        Message = "Đơn hàng #" + id + " không tồn tại trong hệ thống!"
                    });
                }
            }
            catch (Exception ex)
            {

                // Rollback
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                // Write error logs
                EventWriter.WriteEventLog("SaleController - DeleteOrder: " + ex.ToString());

                // Return error
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public JsonResult DeleteOrderDetails(int id)
        {
            try
            {
                // Get order details
                var details = db.OrderDetails.Where(r => r.id == id).FirstOrDefault();
                if (details != null)
                {
                    // Remove order details
                    db.OrderDetails.Remove(details);
                    db.SaveChanges();

                    // Success
                    return Json(new
                    {
                        Success = true,
                        Message = "Xóa chi tiết đơn hàng thành công!"
                    });
                }
                else
                {
                    // Fail
                    return Json(new
                    {
                        Success = false,
                        Message = "Chi tiết đơn hàng #" + id + " không tồn tại trong hệ thống!"
                    });
                }
            }
            catch (Exception ex)
            {
                // Write error logs
                EventWriter.WriteEventLog("SaleController - DeleteOrderDetails: " + ex.ToString());

                // Return error
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        #endregion

        #region Customers

        public ActionResult ListCustomers(string filter = "", int page = 1)
        {
            try
            {
                // Validate page 
                page = page > 0 ? page : 1;

                // Get default pagesize
                var pageSize = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"]);

                // Query data
                var pFilter = new SqlParameter("filter", filter);
                var pPageIndex = new SqlParameter("pageIndex", page);
                var pPageSize = new SqlParameter("pageSize", pageSize);
                var pItemCount = new SqlParameter("itemCount", System.Data.SqlDbType.Int, 4);

                pPageIndex.Direction = System.Data.ParameterDirection.InputOutput;
                pItemCount.Direction = System.Data.ParameterDirection.Output;

                var customers = db.Database.SqlQuery<Customer>(@"EXEC [dbo].[usp_searchCustomers]
                            @filter, 
                            @pageIndex OUT, 
                            @pageSize, 
                            @itemCount OUT",
                            pFilter,
                            pPageIndex,
                            pPageSize,
                            pItemCount).ToList();

                // Create view model
                var totalItems = (int)pItemCount.Value;
                var pager = new Pager(totalItems, (int)pPageIndex.Value, pageSize);
                
                var model = new ListCustomerModel
                {
                    Filter = filter,
                    Items = customers,
                    Pager = pager
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SaleController - ListCustomers: " + ex.ToString());
            }
            return View();
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditCustomer(string phone)
        {
            var model = new EditCustomerModel();
            try
            {
                // Get customer
                var customer = db.Customers.Where(r => r.phone.Equals(phone)).FirstOrDefault();
                if (customer != null)
                {
                    model.Phone = customer.phone;
                    model.FullName = customer.fullname;
                    model.Address = customer.address;
                    model.Email = customer.email;
                }
                else
                {
                    ModelState.AddModelError("", "Khách hàng số điện thoại #" + phone + " không tồn tại trong hệ thống!");
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SaleController - EditCustomer: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditCustomer(EditCustomerModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // get customer
                    var customer = db.Customers.Where(r => r.phone.Equals(model.Phone)).FirstOrDefault();
                    if (customer != null)
                    {
                        customer.phone = model.Phone;
                        customer.fullname = model.FullName;
                        customer.address = model.Address;
                        customer.email = model.Email;

                        // Update password
                        if (string.IsNullOrWhiteSpace(model.Password) == false)
                        {
                            var sh = new SaltedHash(model.Password);
                            customer.salt = sh.Salt;
                            customer.password = sh.Hash;
                        }

                        db.SaveChanges();

                        return RedirectToAction("ListCustomers");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Khách hàng số điện thoại #" + model.Phone + " không tồn tại trong hệ thống!");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SaleController - EditCustomer: " + ex.ToString());
            }
            return View(model);
        }


        #endregion
    }
}