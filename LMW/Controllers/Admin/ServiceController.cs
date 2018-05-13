using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using LMW.Models.Admin;
using LMW.Models.Filters;
using LMW.Models.Database;
using LMW.Libs;

namespace LMW.Controllers.Admin
{
    public class ServiceController : Controller
    {
        DBContext db = new DBContext();

        #region Groups
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult ListServiceGroups(string filter, int? page)
        {
            try
            {
                filter = filter ?? "";
                page = page ?? 1;

                // Get default pagesize
                var pageSize = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"]);

                // Create SQL parameters
                var pFilter = new SqlParameter("filter", filter);
                var pPageIndex = new SqlParameter("pageIndex", page);
                var pPageSize = new SqlParameter("pageSize", pageSize);
                var pItemCount = new SqlParameter("itemCount", System.Data.SqlDbType.Int, 4);

                pPageIndex.Direction = System.Data.ParameterDirection.InputOutput;
                pItemCount.Direction = System.Data.ParameterDirection.Output;

                // Search users
                var groups = db.Database.SqlQuery<CarServiceGroup>(@"EXEC [dbo].[usp_searchCarServiceGroups] 
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
                var model = new ListServiceGroupsModel { Filter = filter, Items = groups, Pager = pager };

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SystemController - ListServiceGroups: " + ex.ToString());
            }

            // Create empty model
            return View();
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateServiceGroup()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateServiceGroup(CreateServiceGroupModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var group = db.CarServiceGroups.Where(u => String.Compare(u.groupName, model.groupName, true) == 0).FirstOrDefault();
                    if (group == null)
                    {
                        group = new CarServiceGroup();
                        group.groupName = model.groupName;
                        group.iconURL = model.iconURL;
                        group.description = model.description;
                        group.fullDescription = model.fullDescription;
                        group.creationDate = DateTime.Now;
                        group.lastUpdate = DateTime.Now;
                        group.updatedBy = User.Identity.Name;
                        group.sortIdx = db.CarServiceGroups.Count() + 1;

                        db.CarServiceGroups.Add(group);
                        db.SaveChanges();

                        return RedirectToAction("ListServiceGroups");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Nhóm dịch vụ tên <strong>" + model.groupName + "</strong> đã tồn tại trong hệ thống. Vui lòng nhập tên khác!");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.ToString());

                    EventWriter.WriteEventLog("SystemController - CreateServiceGroup: " + ex.ToString());
                }
            }
            return View(model);
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditServiceGroup(int id)
        {
            var model = new EditServiceGroupModel();
            try
            {
                var group = db.CarServiceGroups.Where(r => r.groupId == id).FirstOrDefault();
                if (group != null)
                {
                    model.groupId = group.groupId;
                    model.groupName = group.groupName;
                    model.iconURL = group.iconURL;
                    model.description = group.description;
                    model.fullDescription = group.fullDescription;
                }
                else
                {
                    ModelState.AddModelError("", "Nhóm dịch vụ #" + id + " không tồn tại trong hệ thống!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.ToString());

                EventWriter.WriteEventLog("SystemController - EditCarServiceGroup: " + ex.ToString());
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditServiceGroup(EditServiceGroupModel model)
        {
            try
            {
                // Get current service group
                var group = db.CarServiceGroups.Where(r => r.groupId == model.groupId).FirstOrDefault();
                if (group != null)
                {
                    // Check group name existed
                    var groupExisted = db.CarServiceGroups
                        .Where(r => r.groupId != model.groupId && String.Compare(r.groupName, model.groupName, true) == 0)
                        .FirstOrDefault();
                    if (groupExisted == null)
                    {
                        // Update group
                        group.groupName = model.groupName;
                        group.iconURL = model.iconURL;
                        group.description = model.description;
                        group.fullDescription = model.fullDescription;

                        db.SaveChanges();
                        return RedirectToAction("ListServiceGroups");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Nhóm dịch vụ #" + model.groupName + " đã được sử dụng! Bạn vui lòng nhập tên khác.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Nhóm dịch vụ #" + model.groupId + " không tồn tại trong hệ thống!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.ToString());

                EventWriter.WriteEventLog("SystemController - EditCarServiceGroup: " + ex.ToString());
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public JsonResult DeleteServiceGroup(int id)
        {
            DbContextTransaction transaction = null;
            try
            {
                // Start transaction
                transaction = db.Database.BeginTransaction();

                // Get car service group info
                var group = db.CarServiceGroups.Where(r => r.groupId == id).FirstOrDefault();
                if (group != null)
                {
                    // Remove service details
                    var services = db.CarServices.Where(r => r.groupId == group.groupId).ToList();
                    foreach (var service in services)
                    {
                        db.CarServiceDetails.RemoveRange(db.CarServiceDetails.Where(r => r.serviceId == service.serviceId).ToList());
                    }

                    // Remove services
                    db.CarServices.RemoveRange(services);

                    // Remove group
                    db.CarServiceGroups.Remove(group);

                    db.SaveChanges();

                    transaction.Commit();

                    // Success
                    return Json(new
                    {
                        Success = true,
                        Message = "Xóa nhóm dịch vụ thành công!"
                    });
                }
                else
                {
                    // Fail
                    return Json(new
                    {
                        Success = false,
                        Message = "Nhóm dịch vụ #" + id + " không tồn tại trong hệ thống!"
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
                EventWriter.WriteEventLog("SystemController - DeleteCarServiceGroup: " + ex.ToString());

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
        public JsonResult GetServiceGroups()
        {
            try
            {
                // Get all service groups
                var groups = db.CarServiceGroups.OrderBy(r => r.sortIdx).ThenBy(r => r.groupName).ToList();

                return Json(new
                {
                    Success = true,
                    Message = groups
                });
            }
            catch (Exception ex)
            {
                // Write error logs
                EventWriter.WriteEventLog("SystemController - GetAllServiceGroups: " + ex.ToString());

                // Return error
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        #endregion

        #region Services

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult ListServices(string filter, int? groupId, int? page = 1)
        {
            filter = filter ?? "";
            groupId = groupId ?? 0;
            page = page ?? 1;

            try
            {
                // Get default pagesize
                var pageSize = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"]);

                // Get service group info
                var group = db.CarServiceGroups.Where(r => r.groupId == groupId).FirstOrDefault();

                if (group != null)
                {
                    // Search group's services
                    var pFilter = new SqlParameter("filter", filter);
                    var pGroupId = new SqlParameter("groupId", groupId);
                    var pPageIndex = new SqlParameter("pageIndex", page);
                    var pPageSize = new SqlParameter("pageSize", pageSize);
                    var pItemCount = new SqlParameter("itemCount", System.Data.SqlDbType.Int, 4);
                    pPageIndex.Direction = System.Data.ParameterDirection.InputOutput;
                    pItemCount.Direction = System.Data.ParameterDirection.Output;
                    var services = db.Database.SqlQuery<CarService>(@"EXEC [dbo].[usp_searchCarServices]  
                                                                            @filter, 
                                                                            @groupId, 
                                                                            @pageIndex OUT, 
                                                                            @pageSize, 
                                                                            @itemCount OUT",
                                                                            pFilter,
                                                                            pGroupId,
                                                                            pPageIndex,
                                                                            pPageSize,
                                                                            pItemCount).ToList();

                    // Generate service group options
                    var totalItems = (int)pItemCount.Value;
                    var pager = new Pager(totalItems, (int)pPageIndex.Value, pageSize);

                    // Return view model
                    var model = new ListServiceModel
                    {
                        Filter = filter,
                        groupId = groupId ?? 0,
                        groupName = group.groupName,
                        Items = services,
                        Pager = pager,
                    };
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy nhóm dịch vụ mã #" + groupId + "!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SystemController - ListServices: " + ex.ToString());
            }
            return View();
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateService(int? groupId)
        {
            var model = new CreateServiceModel();
            try
            {
                var group = db.CarServiceGroups.Where(r => r.groupId == (groupId ?? 0)).FirstOrDefault();
                if (group != null)
                {
                    model.groupId = group.groupId;
                    model.groupName = group.groupName;
                }
                else
                {
                    ModelState.AddModelError("", "Nhóm dịch vụ không hợp lệ!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SystemController - CreateService: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateService(CreateServiceModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var service = db.CarServices
                        .Where(r => r.groupId == model.groupId && String.Compare(r.serviceName, model.serviceName, true) == 0)
                        .FirstOrDefault();
                    if (service == null)
                    {
                        service = new CarService();
                        service.serviceName = model.serviceName;
                        service.groupId = model.groupId;
                        service.iconURL = model.iconURL;
                        service.description = model.description;
                        service.owner = User.Identity.Name;
                        service.creationDate = DateTime.Now;
                        service.sortIdx = db.CarServices.Count() + 1;

                        db.CarServices.Add(service);
                        db.SaveChanges();

                        return RedirectToAction("ListServices", new { groupID = model.groupId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Dịch vụ tên <strong>" + model.serviceName + "</strong> đã tồn tại trong hệ thống. Vui lòng nhập tên khác!");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.ToString());

                    EventWriter.WriteEventLog("SystemController - CreateService: " + ex.ToString());
                }
            }
            return View(model);
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditService(int id)
        {
            var model = new EditServiceModel();
            try
            {
                var service = db.CarServices.Where(r => r.serviceId == id).FirstOrDefault();
                if (service != null)
                {
                    var group = db.CarServiceGroups.Where(r => r.groupId == service.groupId).FirstOrDefault();
                    if (group != null)
                    {
                        model.serviceId = service.serviceId;
                        model.serviceName = service.serviceName;
                        model.groupId = service.groupId;
                        model.groupName = group.groupName;
                        model.iconURL = service.iconURL;
                        model.description = service.description;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Nhóm dịch vụ mã #" + service.groupId + " không tồn tại trong hệ thống!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Dịch vụ mã #" + id + " không tồn tại trong hệ thống!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());
                EventWriter.WriteEventLog("SystemController - EditService: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditService(EditServiceModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get service
                    var service = db.CarServices.Where(r => r.serviceId == model.serviceId).FirstOrDefault();
                    if (service != null)
                    {
                        // Check service name exsited
                        var serviceExisted = db.CarServices
                            .Where(r =>
                                r.serviceId != model.serviceId &&
                                String.Compare(r.serviceName, model.serviceName, true) == 0)
                            .FirstOrDefault();
                        if (serviceExisted == null)
                        {
                            // Update service 
                            service.serviceName = model.serviceName;
                            service.groupId = model.groupId;
                            service.iconURL = model.iconURL;
                            service.description = model.description;

                            db.SaveChanges();
                            return RedirectToAction("ListServices", new { groupID = model.groupId });
                        }
                        else
                        {
                            ModelState.AddModelError("", "Dịch vụ #" + model.serviceName + " đã tồn tại trong hệ thống! Vui lòng nhập tên khác");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Dịch vụ mã #" + model.serviceId + " không tồn tại trong hệ thống!");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SystemController - EditService: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public JsonResult DeleteService(int id)
        {
            DbContextTransaction transaction = null;
            try
            {
                // Start transaction
                transaction = db.Database.BeginTransaction();

                // Get service
                var service = db.CarServices.Where(r => r.serviceId == id).FirstOrDefault();
                if (service != null)
                {
                    // Remove all service details
                    db.CarServiceDetails.RemoveRange(db.CarServiceDetails.Where(r => r.serviceId == service.serviceId).ToList());

                    // Remove service
                    db.CarServices.Remove(service);

                    db.SaveChanges();

                    transaction.Commit();

                    // Success
                    return Json(new
                    {
                        Success = true,
                        Message = "Xóa dịch vụ thành công!"
                    });
                }
                else
                {
                    // Fail
                    return Json(new
                    {
                        Success = false,
                        Message = "Nhóm dịch vụ #" + id + " không tồn tại trong hệ thống!"
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
                EventWriter.WriteEventLog("SystemController - DeleteCarServiceGroup: " + ex.ToString());

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
        public JsonResult GetServices(int groupId)
        {
            try
            {
                // Get all service groups
                var services = db.CarServices
                    .Where(r => r.groupId == groupId)
                    .OrderBy(r => r.sortIdx).ThenBy(r => r.serviceName).ToList();

                return Json(new
                {
                    Success = true,
                    Message = services
                });
            }
            catch (Exception ex)
            {
                // Write error logs
                EventWriter.WriteEventLog("SystemController - GetAllServiceGroups: " + ex.ToString());

                // Return error
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        #endregion

        #region Service Details

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult ListServiceDetails(string filter, int? serviceId, int? page)
        {
            filter = filter ?? "";
            serviceId = serviceId ?? 0;
            page = page ?? 1;
            try
            {
                // Get service info
                var service = db.CarServices.Where(r => r.serviceId == serviceId).FirstOrDefault();
                if (service != null)
                {
                    var group = db.CarServiceGroups.Where(r => r.groupId == service.groupId).FirstOrDefault();
                    if (group != null)
                    {
                        // Get default pagesize
                        var pageSize = int.Parse(ConfigurationManager.AppSettings["PAGE_SIZE"]);

                        // Search service details
                        var pFilter = new SqlParameter("filter", filter);
                        var pServiceId = new SqlParameter("serviceId", serviceId);
                        var pPageIndex = new SqlParameter("pageIndex", page);
                        var pPageSize = new SqlParameter("pageSize", pageSize);
                        var pItemCount = new SqlParameter("itemCount", System.Data.SqlDbType.Int, 4);
                        pPageIndex.Direction = System.Data.ParameterDirection.InputOutput;
                        pItemCount.Direction = System.Data.ParameterDirection.Output;
                        var serviceDetails = db.Database.SqlQuery<CarServiceDetails>(@"EXEC [dbo].[usp_searchCarServiceDetails]  
                                                                            @filter, 
                                                                            @serviceId, 
                                                                            @pageIndex OUT, 
                                                                            @pageSize, 
                                                                            @itemCount OUT",
                                                                                pFilter,
                                                                                pServiceId,
                                                                                pPageIndex,
                                                                                pPageSize,
                                                                                pItemCount).ToList();

                        // Generate service group options
                        var totalItems = (int)pItemCount.Value;
                        var pager = new Pager(totalItems, (int)pPageIndex.Value, pageSize);

                        // Return view model
                        var model = new ListServiceDetailsModel
                        {
                            Filter = filter,
                            groupId = group.groupId,
                            groupName = group.groupName,
                            serviceId = service.serviceId,
                            serviceName = service.serviceName,
                            Items = serviceDetails,
                            Pager = pager
                        };
                        return View(model);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Dịch vụ #" + service.serviceName + " không thuộc nhóm nào!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy dịch vụ #" + serviceId + "!");
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SystemController - ListServices: " + ex.ToString());
            }

            return View();
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateServiceDetails(int serviceId)
        {
            var model = new CreateServiceDetailsModel();
            try
            {
                var service = db.CarServices.Where(r => r.serviceId == serviceId).FirstOrDefault();
                if (service != null)
                {
                    model.serviceId = service.serviceId;
                    model.serviceName = service.serviceName;
                    model.groupId = service.groupId;

                    var group = db.CarServiceGroups.Where(r => r.groupId == service.groupId).FirstOrDefault();
                    if (group != null)
                    {
                        model.groupName = group.groupName;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Nhóm dịch vụ mã #" + service.groupId + " không tồn tại trong hệ thống!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Dịch vụ mã #" + serviceId + " không tồn tại trong hệ thống!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                // Write error logs
                EventWriter.WriteEventLog("SystemController - CreateServiceDetails: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateServiceDetails(CreateServiceDetailsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var details = new CarServiceDetails();
                    details.serviceId = model.serviceId;
                    details.itemName = model.itemName;
                    details.unit = model.unit;
                    details.price = decimal.Parse(model.price.Replace(",", ""));
                    details.priceOriginal = decimal.Parse(model.priceOriginal.Replace(",", ""));
                    details.creationDate = DateTime.Now;
                    details.owner = User.Identity.Name;

                    db.CarServiceDetails.Add(details);
                    db.SaveChanges();

                    return RedirectToAction("ListServiceDetails", new { serviceId = details.serviceId });
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                // Write error logs
                EventWriter.WriteEventLog("SystemController - CreateServiceDetails: " + ex.ToString());
            }
            return View(model);
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditServiceDetails(int id)
        {
            var model = new EditServiceDetailsModel();
            try
            {
                // Get service details
                var details = db.CarServiceDetails.Where(r => r.itemId == id).FirstOrDefault();
                if (details != null)
                {
                    // get service info
                    var service = db.CarServices.Where(r => r.serviceId == details.serviceId).FirstOrDefault();
                    if (service != null)
                    {
                        // get service group
                        var group = db.CarServiceGroups.Where(r => r.groupId == service.groupId).FirstOrDefault();
                        if (group != null)
                        {
                            model.itemId = details.itemId;
                            model.itemName = details.itemName;
                            model.unit = details.unit;
                            model.price = details.price.ToString("#,###");
                            model.priceOriginal = details.priceOriginal.ToString("#,###");
                            model.serviceId = service.serviceId;
                            model.serviceName = service.serviceName;
                            model.groupId = service.groupId;
                            model.groupName = group.groupName;
                        }
                        else
                        {
                            ModelState.AddModelError("", "Chi tiết dịch vụ không thuộc nhóm dịch vụ nào!");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Chi tiết dịch vụ không thuộc dịch vụ nào!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Chi tiết dịch vụ mã #" + id + " không tồn tại trong hệ thống!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                // Write error logs
                EventWriter.WriteEventLog("SystemController - EditServiceDetails: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditServiceDetails(EditServiceDetailsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var details = db.CarServiceDetails.Where(r => r.itemId == model.itemId).FirstOrDefault();
                    if (details != null)
                    {
                        // item name existed
                        var existedDetails = db.CarServiceDetails
                            .Where(r => r.itemId != model.itemId && String.Compare(r.itemName, model.itemName, true) == 0)
                            .FirstOrDefault();
                        if (existedDetails == null)
                        {
                            // update service details
                            details.itemName = model.itemName;
                            details.unit = model.unit;
                            details.price = decimal.Parse(model.price.Replace(",", ""));
                            details.priceOriginal = decimal.Parse(model.priceOriginal.Replace(",", ""));
                            details.lastUpdate = DateTime.Now;
                            details.updatedBy = User.Identity.Name;

                            db.SaveChanges();
                            return RedirectToAction("ListServiceDetails", new { serviceId = details.serviceId });
                        }
                        else
                        {
                            ModelState.AddModelError("", "Chi tiết dịch vụ tên #" + model.itemName + " đã được sử dụng! Vui lòng nhập tên khác.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Chi tiết dịch vụ #" + model.itemName + " không tồn tại trong hệ thống!");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                // Write error logs
                EventWriter.WriteEventLog("SystemController - CreateServiceDetails: " + ex.ToString());
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public JsonResult DeleteServiceDetails(int id)
        {
            try
            {
                // Get service details
                var details = db.CarServiceDetails.Where(r => r.itemId == id).FirstOrDefault();
                if (details != null)
                {
                    // Remove service details
                    db.CarServiceDetails.Remove(details);
                    db.SaveChanges();

                    // Success
                    return Json(new
                    {
                        Success = true,
                        Message = "Xóa dịch vụ thành công!"
                    });
                }
                else
                {
                    // Fail
                    return Json(new
                    {
                        Success = false,
                        Message = "Nhóm dịch vụ #" + id + " không tồn tại trong hệ thống!"
                    });
                }
            }
            catch (Exception ex)
            {
                // Write error logs
                EventWriter.WriteEventLog("SystemController - DeleteCarServiceGroup: " + ex.ToString());

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
        public JsonResult GetServiceDetails(int serviceId)
        {
            try
            {
                // Get all service groups
                var details = db.CarServiceDetails
                    .Where(r => r.serviceId == serviceId)
                    .OrderBy(r => r.sortIdx).ThenBy(r => r.itemName).ToList();

                return Json(new
                {
                    Success = true,
                    Message = details
                });
            }
            catch (Exception ex)
            {
                // Write error logs
                EventWriter.WriteEventLog("SystemController - GetAllServiceGroups: " + ex.ToString());

                // Return error
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        #endregion
    }
}