using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LMW.Libs;
using LMW.Models.Database;
using LMW.Models.Mobile;
using LMW.Models.Filters;

namespace LMW.Controllers.Mobile
{
    public class CustomerController : ApiController
    {
        DBContext db = new DBContext();

        /// <summary>
        /// Đăng ký thông tin khách hàng
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage register([FromBody] JObject json)
        {
            var eventLogs = "";
            try
            {
                /** 
                 * Description
                 * 
                 * Input:
                 * + phone: Số điện thoai khách hàng
                 * + email: Địa chỉ email khách hàng
                 * + address: Địa chỉ khách hàng
                 * 
                 * Return:
                 * + ReturnCode: 0 thành công (#0 mã lỗi)
                 * + ReturnMessage: thông báo trả về
                 * 
                 * */

                // Get requested params
                var phone = (json.GetValue("phone").Value<string>() ?? "").Trim();
                var password = (json.GetValue("password").Value<string>() ?? "").Trim();
                var fullname = (json.GetValue("fullname").Value<string>() ?? "").Trim();
                var email = (json.GetValue("email").Value<string>() ?? "").Trim();

                // Validate customer info
                if (string.IsNullOrWhiteSpace(phone))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Số điện thoại không được rỗng!"
                    });
                }

                if (password.Length < 6)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Độ dài mật khẩu tối thiểu 6 ký tự!"
                    });
                }

                // Hash password
                SaltedHash sh = new SaltedHash(password);
                var hash = sh.Hash;
                var salt = sh.Salt;

                // Save customer info
                var regCustResult = db.Database.SqlQuery<RegisterCustomerResult>(@"EXEC [dbo].[usp_registerCustomer] 
                                    @phone, 
                                    @password, 
                                    @salt, 
                                    @fullname,
                                    @email",
                   new SqlParameter("phone", phone),
                   new SqlParameter("password", hash),
                   new SqlParameter("salt", salt),
                   new SqlParameter("fullname", fullname),
                   new SqlParameter("email", email))
                   .FirstOrDefault();

                if (regCustResult != null)
                {
                    var success = (regCustResult.ReturnCode == 0);

                    // set event logs
                    eventLogs =
                        "Customer controller - register: \r\n" + json.ToString() + 
                        "\r\nReturnCode: " + regCustResult.ReturnCode + 
                        "\r\nReturnMessage" + regCustResult.ReturnMessage;

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = success,
                        Data = regCustResult.ReturnMessage
                    });
                }
                else
                {
                    // set event logs
                    eventLogs =
                        "Customer controller - register: \r\n" + json.ToString() +
                        "\r\nReturnMessage: Đăng ký tài khoản thất bại! Có lỗi xảy ra trong quá trình đăng ký tài khoản";

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Đăng ký tài khoản thất bại! Có lỗi xảy ra trong quá trình đăng ký tài khoản"
                    });
                }
            }
            catch (Exception ex)
            {
                // set event logs
                eventLogs = "CustomerController - register: " + ex.ToString();

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
        /// Kiểm tra đăng nhập khách hàng
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage verify([FromBody] JObject json)
        {
            var eventLogs = "";
            try
            {
                /** 
                 * Description
                 * Input
                 * + phone: Số điện thoai khách hàng
                 * + password: Địa chỉ email khách hàng
                 * 
                 * Output
                 * + Thông tin customer
                 * */

                // Get requested params
                var phone = (json.GetValue("phone").Value<string>() ?? "").Trim();
                var password = (json.GetValue("password").Value<string>() ?? "").Trim();

                var customer = db.Database.SqlQuery<Customer>("EXEC [dbo].[usp_getCustomer] @phone",
                    new SqlParameter("phone", phone)).FirstOrDefault();
                if (customer != null)
                {
                    // Verify password
                    var success = SaltedHash.Verify(customer.salt, customer.password, password);
                    if (success)
                    {
                        // set event logs
                        eventLogs += "Login phone number = " + phone + " success";

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Success = true,
                            Data = new {
                                phone = customer.phone,
                                fullname = customer.fullname,
                                email = customer.email,
                                address = customer.address
                            }
                        });
                    } else
                    {
                        // set event logs
                        eventLogs += "Login phone number = " + phone + " fail, wrong password";

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Success = false,
                            Data = "Đăng nhập thất bại! Vui lòng kiểm tra lại mật khẩu"
                        });
                    }

                } else
                {
                    // set event logs
                    eventLogs += "Login phone number = " + phone + " fail, wrong phone number";

                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Đăng nhập thất bại! Số điện thoại chưa được đăng ký"
                    });
                }
            }
            catch (Exception ex)
            {
                // set event logs
                eventLogs = "CustomerController - verify: " + ex.ToString();

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

        [HttpPost]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage changePassword([FromBody] JObject json)
        {
            var eventLogs = "";
            try
            {
                /** 
                 * Description
                 * Input
                 * + phone: Số điện thoai khách hàng
                 * + passwordCurrent: Mật khẩu hiện tại
                 * + password: Mật khẩu mới
                 * 
                 * Output
                 * + Success: true/false
                 * + Data: Mô tả kết quả trả về
                 * */

                // Get requested params
                var phone = (json.GetValue("phone").Value<string>() ?? "").Trim();
                var passwordCurrent = (json.GetValue("passwordCurrent").Value<string>() ?? "").Trim();
                var password = (json.GetValue("password").Value<string>() ?? "").Trim();

                var customer = db.Customers.Where(r => r.phone.Equals(phone)).FirstOrDefault();
                if (customer != null)
                {
                    // Verify password
                    var success = SaltedHash.Verify(customer.salt, customer.password, passwordCurrent);
                    if (success)
                    {
                        // Assign new password
                        var sh = new SaltedHash(password);
                        customer.salt = sh.Salt;
                        customer.password = sh.Hash;
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Success = true,
                            Data = "Đổi mật khẩu thành công!"
                        });
                    }
                    else
                    {
                        // Wrong password
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Success = false,
                            Data = "Đổi mật khẩu thất bại! Mật khẩu hiện tại không đúng"
                        });
                    }

                }
                else
                {
                    // Customer not found
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Đổi mật khẩu thất bại! Khách hàng #" + phone + " không tồn tại trong hệ thống!"
                    });
                }
            }
            catch (Exception ex)
            {
                // set event logs
                eventLogs = "CustomerController - changePassword: " + ex.ToString();

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

        [HttpPost]
        [LMWApiAuthorizationFilter]
        public HttpResponseMessage recoveryPassword([FromBody] JObject json)
        {
            var eventLogs = "";
            try
            {
                /** 
                 * Description
                 * Input
                 * + phone: Số điện thoai khách hàng
                 * + email: Địa chỉ email
                 * 
                 * Output
                 * + Thông tin customer
                 * */

                // Get requested params
                var phone = (json.GetValue("phone").Value<string>() ?? "").Trim();
                var email = (json.GetValue("email").Value<string>() ?? "").Trim();

                // Get customer
                var customer = db.Customers.Where(c => c.phone == phone).FirstOrDefault();
                if (customer != null && !string.IsNullOrWhiteSpace(customer.email)) 
                {
                    // Validate user email
                    if (String.Compare(customer.email, email, true) == 0)
                    {
                        // create random password
                        String newpass = Membership.GeneratePassword(6, 0);
                        SaltedHash sh = new SaltedHash(newpass);
                        customer.salt = sh.Salt;
                        customer.password = sh.Hash;
                        db.SaveChanges();

                        // email new password to user
                        string subject = "Khôi phục mật khẩu tài khoản";
                        string body = "<p><strong>Kính chào quý khách, </strong></p>";
                        body += "<p>Quý khách nhận được email này từ L.M.W vì quý khách hoặc ai đó đã có yêu cầu khôi phục mật khẩu tài khoản.</p>";
                        body += "<br />";
                        body += "<p>Sau đây là thông tin đăng nhập mới của bạn:</p>";
                        body += "<p>Tài khoản: <strong>" + phone + "</strong></p>";
                        body += "<p>Mật khẩu: <strong>" + newpass + "</strong></p>";
                        body += "<br />";
                        body += "<p>Bạn vui lòng đăng nhập lại ứng dụng với mật khẩu mới.</p>";
                        body += "<p>Chúc bạn một ngày tốt lành!</p>";
                        body += "<p>L.M.W</p>";
                        Emailer.sendMail(customer.email, customer.fullname, subject, body);

                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Success = true,
                            Data = "Khôi phục mật khẩu thành công!\r\nVui lòng kiểm tra email: " + customer.email + " để nhận mật khẩu mới"
                        });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Success = false,
                            Data = "Khôi phục mật khẩu thất bại! Địa chỉ email không khớp với địa chỉ email đã đăng ký. Vui lòng liên hệ email về ceo@letmewash.vn để được hỗ trợ!"
                        });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Data = "Khôi phục mật khẩu thất bại! Khách hàng #" + phone +" không tồn tại trong hệ thống. Vui lòng liên hệ email ceo@letmewash.vn để được hỗ trợ!"
                    });
                }
            }
            catch (Exception ex)
            {
                // set event logs
                eventLogs = "CustomerController - recoveryPassword: " + ex.ToString();

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
