using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using LMW.Libs;
using LMW.Models.Filters;
using LMW.Models.Database;
using LMW.Models.Mobile;

namespace LMW.Controllers.Mobile
{
    public class MessageController : ApiController
    {
        DBContext db = new DBContext();

        /// <summary>
        /// Get customer's message
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage getMessages(string phone, int skip, int take)
        {
            var eventLogs = "";
            try
            {
                // Get service details
                var messages = db.Database.SqlQuery<Message>("EXEC [dbo].[usp_getMessage] @phone, @skip, @take",
                    new SqlParameter("phone", phone),
                    new SqlParameter("skip", skip),
                    new SqlParameter("take", take))
                    .ToList();


                // return group services
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = true,
                    MaxRows = db.Messages.Where(r => r.phone.Equals(phone)).Count(),
                    Data = messages
                });
            }
            catch (Exception ex)
            {
                eventLogs = "MessageController - getMessage: " + ex.ToString();

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
        /// Update message status
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="status">0: unread, 1:readed, 2: removed</param>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage updateMessageStatus(string messageId, int status)
        {
            var eventLogs = "";
            try
            {
                // Get service details
                var updateResult = db.Database.SqlQuery<UpdateMessageStatusResult>("EXEC [dbo].[usp_updateMessageStatus] @messageId, @status",
                    new SqlParameter("messageId", messageId),
                    new SqlParameter("status", status))
                    .FirstOrDefault();

                // return group services
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = true,
                    Data = updateResult
                });
            }
            catch (Exception ex)
            {
                eventLogs = "MessageController - updateMessageStatus: " + ex.ToString();

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

        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage setAllMessagesAsRead(string phone)
        {
            var eventLogs = "";
            try
            {
                // Get service details
                var messageStatus = db.Database.SqlQuery<UpdateMessageStatusResult>("EXEC [dbo].[usp_setAllMessagesAsRead] @phone",
                    new SqlParameter("phone", phone)).FirstOrDefault();

                // return group services
                if (messageStatus != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = true,
                        Data = messageStatus
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Fail to get message status"
                    });
                }
            }
            catch (Exception ex)
            {
                eventLogs = "MessageController - getCustomerMessageStatus: " + ex.ToString();

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

        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage getCustomerMessageSummary(string phone)
        {
            var eventLogs = "";
            try
            {
                // Get service details
                var summary = db.Database.SqlQuery<MessageStatusResult>("EXEC [dbo].[usp_getCustomerMessageSummary] @phone",
                    new SqlParameter("phone", phone)).FirstOrDefault();

                // return group services
                if (summary != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = true,
                        Data = summary
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Fail to get message status"
                    });
                }
            }
            catch (Exception ex)
            {
                eventLogs = "MessageController - getCustomerMessageSummary: " + ex.ToString();

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
