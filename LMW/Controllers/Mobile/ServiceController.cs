using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LMW.Libs;
using LMW.Models.Filters;
using LMW.Models.Database;
using LMW.Models.Mobile;

namespace LMW.Controllers.Mobile
{
    public class ServiceController : ApiController
    {
        DBContext db = new DBContext();

        /// <summary>
        /// API đọc danh sách nhóm dịch vụ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage getServiceGroups()
        {
            var eventLogs = "";
            try
            {
                // get groups
                var groups = db.Database.SqlQuery<CarServiceGroupResult>("EXEC [dbo].[usp_getServiceGroups]").ToList();

                // get services
                foreach (var group in groups)
                {
                    var services = db.Database.SqlQuery<CarServiceResult>("EXEC [dbo].usp_getServices @groupId",
                        new SqlParameter("groupId", group.groupId))
                        .ToList();

                    group.services = services;

                    // get service details
                    foreach (var service in services)
                    {
                        var details = db.Database.SqlQuery<CarServiceDetailsResult>("EXEC [dbo].usp_getServiceDetails @serviceId",
                        new SqlParameter("serviceId", service.serviceId))
                        .ToList();

                        service.serviceDetails = details;
                    }
                }

                // return service groups
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = true,
                    Data = groups
                });
            }
            catch (Exception ex)
            {
                eventLogs = "ServiceController - getServiceGroups: " + ex.ToString();

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
        /// API đọc danh sách dịch vụ thuộc nhóm dịch vụ
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage getServices(int groupId)
        {
            var eventLogs = "";
            try
            {
                // get services
                var services = db.Database.SqlQuery<CarServiceResult>("EXEC [dbo].[usp_getServices] @groupId",
                    new SqlParameter("groupId", groupId))
                    .ToList();

                // get service details
                foreach (var service in services)
                {
                    var details = db.Database.SqlQuery<CarServiceDetailsResult>("EXEC [dbo].usp_getServiceDetails @serviceId",
                    new SqlParameter("serviceId", service.serviceId))
                    .ToList();

                    service.serviceDetails = details;
                }

                // return group services
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = true,
                    Data = services
                });
            }
            catch (Exception ex)
            {
                eventLogs = "ServiceController - getServices: " + ex.ToString();

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
        /// API đọc chi tiết dịch vụ
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage getServiceDetails(int serviceId)
        {
            var eventLogs = "";
            try
            {
                // Get service details
                var details = db.Database.SqlQuery<CarServiceDetailsResult>("EXEC [dbo].usp_getServiceDetails @serviceId",
                    new SqlParameter("serviceId", serviceId))
                    .ToList();

                // return group services
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = true,
                    Data = details
                });
            }
            catch (Exception ex)
            {
                eventLogs = "CarService controller - getServiceDetails: " + ex.ToString();

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
