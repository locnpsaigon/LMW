using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LMW.Models.Database
{
    public class Customer
    {
        [Key]
        public string phone { get; set; }
        public string password { get; set; }
        public string salt { get; set; }
        public string fullname { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public DateTime regdate { get; set; }
    }

    public class Order
    {
        [Key]
        public int orderId { get; set; }
        public DateTime date { get; set; }
        public string phone { get; set; }
        public string fullname { get; set; }
        public string address { get; set; }
        public decimal amount { get; set; }
        public int status { get; set; }
        public string note { get; set; }
        public string title { get; set; }

        public Order()
        {
            this.date = DateTime.Now;
        }

        public enum Status
        {
            STATUS_OPENED = 1,
            STATUS_PENDIND = 2,
            STATUS_PROCESSING = 3,
            STATUS_FINISHED = 4,
            STATUS_CLOSED = 5
        }
    }

    public class OrderDetails
    {
        [Key]
        public int id { get; set; }
        public int orderId { get; set; }
        public int itemId { get; set; }
        public string itemName { get; set; }
        public int serviceId { get; set; }
        public string serviceName { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
        public string unit { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public decimal priceOriginal { get; set; }

        public OrderDetails()
        {
            this.quantity = 1;
        }
    }

    public class AppVersion
    {
        [Key]
        public int id { get; set; }
        public DateTime date { get; set; }
        public int versionCode { get; set; }
        public string versionName { get; set; }
        public int platform { get; set; }
        public int deviceType { get; set; }
        public string updateURL { get; set; }
        public int forceUpdate { get; set; }
    }

    public class Contact
    {
        [Key]
        public string phone { get; set; }
        public string contactName { get; set; }
        public string role { get; set; }
    }

    public class Message
    {
        [Key]
        public int messageId { get; set; }
        public DateTime date { get; set; }
        public string phone { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public int status { get; set; }

        public enum Status
        {
            UNREAD = 1,
            READ = 2,
            REMOVED = 3
        }
    }

    

    public class User
    {
        public int UserId { get; set; }
        public String UserName { get; set; }
        public String Password { get; set; }
        public String Salt { get; set; }
        public String FullName { get; set; }
        public String Phone { get; set; }
        public String Email { get; set; }
        public Boolean IsActive { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        [Required]
        public String RoleName { get; set; }
        public String Description { get; set; }
    }

    public class UserRole
    {
        [Key, Column(Order = 0)]
        public int UserId { get; set; }
        [Key, Column(Order = 1)]
        public int RoleId { get; set; }
    }

    public class CarServiceGroup
    {
        [Key]
        public int groupId { get; set; }
        public string groupName { get; set; }
        public string iconURL { get; set; }
        public string description { get; set; }
        public string fullDescription { get; set; }
        public string owner { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime? lastUpdate { get; set; }
        public string updatedBy { get; set; }
        public int sortIdx { get; set; }
    }

    public class CarService
    {
        [Key]
        public int serviceId { get; set; }
        public string serviceName { get; set; }
        public int groupId { get; set; }
        public string iconURL { get; set; }
        public string description { get; set; }
        public string owner { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime? lastUpdate { get; set; }
        public string updatedBy { get; set; }
        public int sortIdx { get; set; }
    }

    public class CarServiceDetails
    {
        [Key]
        public int itemId { get; set; }
        public string itemName { get; set; }
        public string unit { get; set; }
        public decimal price { get; set; }
        public decimal priceOriginal { get; set; }
        public string owner { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime? lastUpdate { get; set; }
        public string updatedBy { get; set; }
        public int serviceId { get; set; }
        public int sortIdx { get; set; }
    }
}