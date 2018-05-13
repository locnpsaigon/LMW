using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using LMW.Models.Database;

namespace LMW.Models.Admin
{
    public class ListOrderModel
    {
        public DateTime Date1 { get; set; }
        public DateTime Date2 { get; set; }
        public string Filter { get; set; }
        public int Status { get; set; }
        public IEnumerable<Order> Items { get; set; }
        public IEnumerable<SelectListItem> StatusOptions { get; set; }
        public Pager Pager { get; set; }

        public ListOrderModel()
        {
            Items = new List<Order>();
            StatusOptions = new List<SelectListItem>();
        }
    }

    public class ListCustomerModel
    {
        public string Filter { get; set; }
        public IEnumerable<Customer> Items { get; set; }
        public Pager Pager { get; set; }

        public ListCustomerModel()
        {
            Items = new List<Customer>();
        }
    }

    public class EditOrderModel
    {
        [Key]
        public int OrderId { get; set; }

        public string OrderDate { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được rỗng!")]
        public string Phone { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Trạng thái đơn hàng không được rỗng")]
        public int Status { get; set; }

        public string Note { get; set; }

        public string Title { get; set;}

        public IEnumerable<SelectListItem> StatusOptions { get; set; }

        public IEnumerable<OrderDetails> OrderDetails { get; set; }

        public EditOrderModel()
        {
            StatusOptions = new List<SelectListItem>();
            OrderDetails = new List<OrderDetails>();
        }

    }

    public class EditCustomerModel
    {
        [Key]
        public string Phone { get; set; }

        [RegularExpression(@"^.*(?=.{5,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[_!@#$%^&+=]).*$", ErrorMessage = "Mật khẩu dài 5 đến 18 ký tự. Bao gồm ít nhất 1 ký tự hoa, 1 ký tự thường và 1 ký tự đặc biệt (_!@#$%^&+=)")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ tên không được rỗng")]
        public string FullName { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public EditCustomerModel()
        {
            this.Phone = "";
            this.Password = "";
            this.FullName = "";
            this.Email = "";
            this.Address = "";
        }
    }
}