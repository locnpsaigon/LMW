using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using LMW.Models.Database;
using LMW.Models.Admin;
using LMW.Models.Filters;
using LMW.Libs;

namespace LMW.Controllers.Admin
{
    public class SystemController : Controller
    {
        DBContext db = new DBContext();

        #region User

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult ListUsers(string filter, int? page)
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
                var users = db.Database.SqlQuery<User>(@"EXEC [dbo].[usp_searchUsers] 
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
                var model = new ListUserModel { Filter = filter, Items = users, Pager = pager };
                
                return View (model);
            }
            catch (Exception ex)
            {
                EventWriter.WriteEventLog("SystemController - ListUsers: " + ex.ToString());
            }

            // Create empty model
            return View();
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateUser()
        {
            try
            {
                var model = new CreateUserModel();
                var roles = db.Roles.OrderBy(r => r.RoleName).ToList();
                foreach (var role in roles)
                {
                    model.SelectedRoles.Add(new SelectOptionModel
                    {
                        Id = role.RoleId.ToString(),
                        Name = role.RoleName,
                        Selected = false
                    });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                // set error
                ModelState.AddModelError("", ex.Message);

                // write error log
                EventWriter.WriteEventLog("SystemController - CreateUser: " + ex.ToString());
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateUser(CreateUserModel model)
        {
            DbContextTransaction transaction = null;

            if (ModelState.IsValid)
            {
                try
                {
                    var user = db.Users.Where(u => String.Compare(u.UserName, model.UserName, true) == 0).FirstOrDefault();
                    if (user == null)
                    {
                        // Begin transaction
                        transaction = db.Database.BeginTransaction();

                        // Save user info
                        user = new Models.Database.User();
                        SaltedHash sh = new SaltedHash(model.Password);

                        user.UserName = model.UserName;
                        user.Password = sh.Hash;
                        user.Salt = sh.Salt;
                        user.FullName = model.FullName;
                        user.Phone = model.Phone;
                        user.Email = model.Email;
                        user.IsActive = model.IsActive;
                        user.CreateDate = DateTime.Now;

                        db.Users.Add(user);
                        db.SaveChanges();

                        // Save user roles
                        foreach (var item in model.SelectedRoles)
                        {
                            var userRole = new UserRole();
                            userRole.UserId = user.UserId;
                            userRole.RoleId = int.Parse(item.Id);
                            db.UserRoles.Add(userRole);
                        }
                        db.SaveChanges();
                        transaction.Commit();

                        return RedirectToAction("ListUsers");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Tài khoản tên <strong>" + model.UserName + "</strong> đã được đăng ký. Vui lòng nhập tên khác!");
                    }
                }
                catch (Exception ex)
                {
                    // Rollback transaction
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    ModelState.AddModelError("", "Error: " + ex.ToString());
                }
            }
            return View(model);
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditUser(int id)
        {
            var model = new EditUserModel();

            try
            {
                var user = db.Users.Where(r => r.UserId == id).FirstOrDefault();
                var roles = db.Roles.OrderBy(r => r.RoleName).ToList();

                // create user model
                if (user != null)
                {
                    model.UserId = user.UserId;
                    model.UserName = user.UserName;
                    model.FullName = user.FullName;
                    model.Phone = user.Phone;
                    model.Email = user.Email;
                    model.IsActive = user.IsActive;
                }
                else
                {
                    ModelState.AddModelError("", "Tài khoản có mã #" + id + " không tồn trong hệ thống!");
                }

                // create user role models
                foreach (var role in roles)
                {
                    Boolean isUserInRole =
                        user != null &&
                        db.UserRoles.Where(r => r.UserId == user.UserId && r.RoleId == role.RoleId).FirstOrDefault() != null;

                    model.SelectedRoles.Add(new SelectOptionModel
                    {
                        Id = role.RoleId.ToString(),
                        Name = role.RoleName,
                        Selected = isUserInRole
                    });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SystemController - EditUser: " + ex.ToString());
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditUser(EditUserModel model)
        {
            DbContextTransaction transaction = null;

            if (ModelState.IsValid)
            {
                try
                {
                    var user = db.Users.Where(u => u.UserId == model.UserId).FirstOrDefault();
                    if (user != null)
                    {
                        // Begin transaction
                        transaction = db.Database.BeginTransaction();

                        // Update user info
                        user.UserName = model.UserName;
                        user.FullName = model.FullName;
                        user.Phone = model.Phone;
                        user.Email = model.Email;
                        user.IsActive = model.IsActive;

                        // Update password
                        if (!string.IsNullOrWhiteSpace(model.Password))
                        {
                            var sh = new SaltedHash(model.Password);
                            user.Salt = sh.Salt;
                            user.Password = sh.Hash;
                        }

                        // Remove current roles
                        db.UserRoles.RemoveRange(db.UserRoles.Where(r => r.UserId == user.UserId).ToList());

                        // Add new user roles
                        foreach (var item in model.SelectedRoles)
                        {
                            var userRole = new UserRole();
                            userRole.UserId = user.UserId;
                            userRole.RoleId = int.Parse(item.Id);
                            db.UserRoles.Add(userRole);
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        return RedirectToAction("ListUsers");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Tài khoản có mã #" + model.UserId + " không tồn tại trong hệ thống!");
                    }
                }
                catch (Exception ex)
                {
                    // Rollback transaction
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    ModelState.AddModelError("", "Error: " + ex.ToString());
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public JsonResult DeleteUser(int id)
        {
            DbContextTransaction transaction = null;
            try
            {
                var user = db.Users.Where(r => r.UserId == id).FirstOrDefault();
                if (user != null)
                {
                    transaction = db.Database.BeginTransaction();
                    db.Users.Remove(user);
                    db.UserRoles.RemoveRange(db.UserRoles.Where(r => r.UserId == id).ToList());
                    db.SaveChanges();
                    transaction.Commit();
                    // Success
                    return Json(new
                    {
                        Success = true,
                        Message = "Xóa tài khoản thành công!"
                    });
                } else
                {
                    // Fail
                    return Json(new
                    {
                        Success = false,
                        Message = "Tài khoảng #" + id + " không tồn tại trong hệ thống!"
                    });
                }
            }
            catch(Exception ex)
            {
                // rollback transaction
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                // Error
                return Json(new {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        #endregion

        #region Role

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult ListRoles(string filter, int? page)
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
                var roles = db.Database.SqlQuery<Role>(@"EXEC [dbo].[usp_searchRoles] 
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
                var model = new ListRoleModel { Filter = filter, Items = roles, Pager = pager };

                return View(model);
            }
            catch (Exception ex)
            {
                EventWriter.WriteEventLog("SystemController - ListRoles: " + ex.ToString());
            }

            // Create empty model
            return View();
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult CreateRole(CreateRoleModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var role = db.Roles.Where(u => String.Compare(u.RoleName, model.RoleName, true) == 0).FirstOrDefault();
                    if (role == null)
                    {
                        role = new Role();
                        role.RoleName = model.RoleName;
                        role.Description = model.Description;

                        db.Roles.Add(role);
                        db.SaveChanges();

                        return RedirectToAction("ListRoles");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Chức danh tên <strong>" + model.RoleName + "</strong> đã tồn tại trong hệ thống. Vui lòng nhập chức danh khác!");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.ToString());

                    EventWriter.WriteEventLog("SystemController - CreateRole: " + ex.ToString());
                }
            }
            return View(model);
        }

        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditRole(int id)
        {
            var model = new EditRoleModel();

            try
            {
                var role = db.Roles.Where(r => r.RoleId == id).FirstOrDefault();

                // create user model
                if (role != null)
                {
                    model.RoleId = role.RoleId;
                    model.RoleName = role.RoleName;
                    model.Description = role.Description;
                }
                else
                {
                    ModelState.AddModelError("", "Chức danh có mã #" + id + " không tồn trong hệ thống!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());

                EventWriter.WriteEventLog("SystemController - EditUser: " + ex.ToString());
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public ActionResult EditRole(EditRoleModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var role = db.Roles.Where(u => u.RoleId == model.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        var roleExisted = db.Roles.Where(r => r.RoleId != model.RoleId &&
                                string.Compare(r.RoleName, model.RoleName, true) == 0).FirstOrDefault();
                        if (roleExisted == null)
                        {   // Update user info
                            role.RoleName = model.RoleName;
                            role.Description = model.Description;

                            db.SaveChanges();

                            return RedirectToAction("ListRoles");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Chức danh có tên #" + model.RoleName + " đã được sử dụng! Vui lòng nhập tên khác!");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Chức danh có mã #" + model.RoleId + " không tồn tại trong hệ thống!");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.ToString());

                    EventWriter.WriteEventLog("SystemController - EditRole: " + ex.ToString());
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [LMWAdminAuthorizationFilter(Roles = "Administrators")]
        public JsonResult DeleteRole(int id)
        {
            try
            {
                var role = db.Roles.Where(r => r.RoleId == id).FirstOrDefault();
                if (role != null)
                {
                    db.Roles.Remove(role);
                    db.SaveChanges();
                    // Success
                    return Json(new
                    {
                        Success = true,
                        Message = "Xóa chức danh thành công!"
                    });
                }
                else
                {
                    // Fail
                    return Json(new
                    {
                        Success = false,
                        Message = "Chức danh #" + id + " không tồn tại trong hệ thống!"
                    });
                }
            }
            catch (Exception ex)
            {
                EventWriter.WriteEventLog("SystemController - DeleteRole: " + ex.ToString());

                // Error
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