using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using LMW.Libs;
using LMW.Models.Database;
using LMW.Models.Mobile;
using LMW.Models.Filters;

namespace LMW.Controllers.Mobile
{
    public class ConfigController : ApiController
    {
        DBContext db = new DBContext();

        /// <summary>
        /// Đọc thông tin contacts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage getContacts()
        {
            var eventLogs = "";

            try
            {
                var contacts = db.Database.SqlQuery<Contact>("EXEC [dbo].[usp_getContacts]").ToList();
                if (contacts != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = true,
                        Data = contacts
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Đọc danh sách liên hệ thất bại!"
                    });
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction 
                eventLogs = "ConfigController - getContacts:  " + ex.ToString();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = false,
                    Data = ex.ToString()
                });
            }
            finally
            {
                // write event logs
                if (string.IsNullOrWhiteSpace(eventLogs) == false)
                {
                    EventWriter.WriteEventLog(eventLogs);
                }
            }
        }

        /// <summary>
        /// Đọc thông tin hỗ trợ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage getSupportInfo()
        {
            var eventLogs = "";

            try
            {
                var supports = db.Database.SqlQuery<SupportInfoResult>("EXEC [dbo].[usp_getSupportInfo]").ToList();
                if (supports != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = true,
                        Data = supports
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Đọc thông tin hỗ trợ thất bại!"
                    });
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction 
                eventLogs = "ConfigController - getSupportInfo:  " + ex.ToString();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = false,
                    Data = ex.ToString()
                });
            }
            finally
            {
                // write event logs
                if (string.IsNullOrWhiteSpace(eventLogs) == false)
                {
                    EventWriter.WriteEventLog(eventLogs);
                }
            }
        }

        /// <summary>
        /// Đọc thông tin phiên bản mới nhất ứng dụng
        /// </summary>
        /// <param name="platform">1: Android, 2: iOS</param>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage getLatestVersion(int platform)
        {
            var eventLogs = "";

            try
            {
                var version = db.Database.SqlQuery<AppVersion>("EXEC [dbo].[usp_getLastestAppVersion] @platform",
                    new SqlParameter("platform", platform)).FirstOrDefault();
                if (version != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = true,
                        Data = version
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Đọc thông tin phiên bản thất bại!"
                    });
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction 
                eventLogs = "ConfigController - getLatestVersion:  " + ex.ToString();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = false,
                    Data = ex.ToString()
                });
            }
            finally
            {
                // write event logs
                if (string.IsNullOrWhiteSpace(eventLogs) == false)
                {
                    EventWriter.WriteEventLog(eventLogs);
                }
            }
        }
    }
}
