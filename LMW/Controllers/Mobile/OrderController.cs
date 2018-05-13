using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LMW.Libs;
using LMW.Models.Filters;
using LMW.Models.Database;
using LMW.Models.Mobile;
namespace LMW.Controllers.Mobile
{
    public class OrderController : ApiController
    {
        DBContext db = new DBContext();

        /// <summary>
        /// API save an order
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage save([FromBody] JObject json)
        {
            var eventLogs = "";

            // Start transaction
            var dbTrans = db.Database.BeginTransaction();

            try
            {
                eventLogs += "Create order:\r\n" + json.ToString();

                // Create new order
                var order = new Order();
                order.phone = json.Value<string>("phone") ?? string.Empty;
                order.fullname = json.Value<string>("fullname") ?? string.Empty;
                order.address = json.Value<string>("address") ?? string.Empty;
                order.amount = json.Value<decimal?>("amount") ?? 0;
                order.note = json.Value<string>("note") ?? string.Empty;
                order.status = (int)Order.Status.STATUS_OPENED;

                var orderDetails = new List<OrderDetails>();
                var services = json.Value<JArray>("serviceBookings");
                var title = "";
                foreach (var service in services)
                {
                    var groupId = service.Value<int>("groupId");
                    var groupName = service.Value<string>("groupName");
                    var serviceId = service.Value<int>("serviceId");
                    var serviceName = service.Value<string>("serviceName");
                    var bookType = service.Value<int>("bookType");
                    var serviceDetails = service.Value<JArray>("details");
                    foreach (var details in serviceDetails)
                    {
                        var item = new OrderDetails();
                        item.groupId = groupId;
                        item.groupName = groupName;
                        item.serviceId = serviceId;
                        item.serviceName = serviceName;
                        item.itemId = details.Value<int>("itemId");
                        item.itemName = details.Value<string>("itemName");
                        item.unit = details.Value<string>("unit");
                        item.quantity = 1;
                        item.price = details.Value<decimal>("price");
                        item.priceOriginal = details.Value<decimal>("priceOriginal");

                        orderDetails.Add(item);
                    }
                    title += string.IsNullOrWhiteSpace(title) ? serviceName : " + " + serviceName;
                }
                order.title = title;

                // Validate order
                if (string.IsNullOrWhiteSpace(order.phone))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Đơn hàng không hợp lệ! Số điện thoại không được rỗng"
                    });
                }
                if (orderDetails.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Đơn hàng không hợp lệ! Đơn hàng không có thông tin chi tiết"
                    });
                }

                // Save order info
                db.Orders.Add(order);
                db.SaveChanges();

                // update order details
                var orderId = order.orderId;
                var amount = (decimal)0.0;
                foreach (var item in orderDetails)
                {
                    item.orderId = orderId;
                    amount += item.price;
                }
                order.amount = amount;
                db.OrderDetails.AddRange(orderDetails);
                db.SaveChanges();
                dbTrans.Commit();

                // Append event logs
                eventLogs += "OrderController - save: success";

                // Send email
                var receiver = ConfigurationManager.AppSettings["ADMIN_EMAIL"]; 
                var receiverName = string.IsNullOrWhiteSpace(order.fullname) ? order.phone : order.fullname;
                var subject = "Đặt hàng thành công";
                var body = "";

                body += "<strong>Kính chào quí khách " + receiverName + ",</strong><br />";
                body += "<p>L.M.W vừa nhận được đơn hàng <strong>#" + order.orderId + "</strong> của quí khách đặt ngày " + order.date.ToString("dd/MM/yyyy HH:mm") + " ";
                body += "với hình thức thanh toán <strong>Cash On Delivery (COD)</strong>.</p><br />";
                body += "<p><strong>Sau đây là thông tin chi tiết đơn hàng:</strong></p><br />";
                body += "<p>Đơn hàng: " + order.orderId + "</p><br />";
                body += "<p>Điện thoại: " + order.phone + "</p><br />";
                body += "<p>Địa chỉ: " + order.address + "</p><br /><br />";
                body += "<table><tr><th nowrap>STT</th><th nowrap>Tên</th><th nowrap>ĐVT</th><th nowrap>Số lượng</th><th nowrap>Giá</th><th nowrap>Thành tiền</th></tr>";
                var count = 0;
                foreach (var item in orderDetails)
                {
                    count++;
                    body += "<tr>";
                    body += "<td nowrap>" + count + "</td>";
                    body += "<td nowrap>" + item.itemName + "</td>";
                    body += "<td nowrap>" + item.unit + "</td>";
                    body += "<td align='right' nowrap>" + item.quantity + "</td>";
                    body += "<td align='right' nowrap>" + item.price.ToString("#,###") + "</td>";
                    body += "<td align='right' nowrap>" + (item.quantity * item.price).ToString("#,###") + "</td>";
                    body += "</tr>";
                }
                body += "<tr><td colspan='5'><strong>Số tiền tạm tính:</strong></td><td align='right'><strong>" + order.amount.ToString("#,###") + "</strong></td></tr>";
                body += "</table><br />";
                body += "<p>Cám ơn quí khác đã sử dụng dịch vụ của chúng tôi!</p>";
                body += "<p><strong>L.M.W</strong></p>";

                Emailer.sendMail(receiver, receiverName, subject, body);

                // return service groups
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = true,
                    Data = "Lưu đơn hàng thành công!"
                });
            }
            catch (Exception ex)
            {
                // Rollback transaction 
                dbTrans.Rollback();

                eventLogs = "OrderController - save: " + ex.ToString();

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
        /// Get customer's orders
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage getOrders(String phone, int skip, int take)
        {
            var eventLogs = "";

            try
            {
                var orders = db.Database.SqlQuery<Order>("EXEC [dbo].[usp_getOrders] @phone, @skip, @take",
                    new SqlParameter("phone", phone),
                    new SqlParameter("skip", skip),
                    new SqlParameter("take", take)).ToList();
                if (orders != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = true,
                        MaxRows = db.Orders.Where(r => r.phone.Equals(phone)).Count(),
                        Data = orders
                    });
                } 
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Nạp danh sách đơn hàng thất bại!"
                    });
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction 
                eventLogs = "OrderController - getCustomerOrders: " + ex.ToString();

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
        /// Get order details
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage getOrderDetails(int orderId)
        {
            var eventLogs = "";

            try
            {
                var orderDetails = db.Database.SqlQuery<OrderDetails>("EXEC [dbo].[usp_getOrderDetails] @orderId",
                    new SqlParameter("orderId", orderId)).ToList();
                if (orderDetails != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = true,
                        Data = orderDetails
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Nạp thông tin chi tiết đơn hàng thất bại!"
                    });
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction 
                eventLogs = "OrderController - getCustomerOrders: " + ex.ToString();

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
        /// Reuse order details of an existed order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage reuseOrderDetails(int orderId)
        {
            var eventLogs = "";

            try
            {
                var orderDetails = db.Database.SqlQuery<OrderDetails>("EXEC [dbo].[usp_reuseOrderDetails] @orderId",
                    new SqlParameter("orderId", orderId)).ToList();
                if (orderDetails != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = true,
                        Data = orderDetails
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Nạp thông tin chi tiết đơn hàng thất bại!"
                    });
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction 
                eventLogs = "OrderController - getCustomerOrders: " + ex.ToString();

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
