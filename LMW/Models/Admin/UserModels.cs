using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using LMW.Models.Database;

namespace LMW.Models.Admin
{

    public class ListUserModel
    {
        public string Filter { get; set; }
        public IEnumerable<User> Items { get; set; }
        public Pager Pager { get; set; }

        public ListUserModel()
        {
            Items = new List<User>();
        }
    }

    public class CreateUserModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được rỗng")]
        [RegularExpression(@"^[a-z0-9_-]{2,15}$", ErrorMessage = "Tên tài khoản chỉ gồm chữ và số, gạch ngang và gạch dưới")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được rỗng")]
        [RegularExpression(@"^.*(?=.{5,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[_!@#$%^&+=]).*$", ErrorMessage = "Mật khẩu dài 5 đến 18 ký tự. Bao gồm ít nhất 1 ký tự hoa, 1 ký tự thường và 1 ký tự đặc biệt (_!@#$%^&+=)")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu và mật khẩu không trùng khớp")]
        [RegularExpression(@"^.*(?=.{5,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[_!@#$%^&+=]).*$", ErrorMessage = "Mật khẩu dài 5 đến 18 ký tự. Bao gồm ít nhất 1 ký tự hoa, 1 ký tự thường và 1 ký tự đặc biệt (_!@#$%^&+=)")]
        public string Password2 { get; set; }

        [Required(ErrorMessage = "Họ tên không được rỗng")]
        public String FullName { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public Boolean IsActive { get; set; }

        public List<SelectOptionModel> SelectedRoles { get; set; }

        public CreateUserModel()
        {
            SelectedRoles = new List<SelectOptionModel>();
        }
    }

    public class EditUserModel
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được rỗng")]
        [RegularExpression(@"^[a-z0-9_-]{2,15}$", ErrorMessage = "Tên tài khoản chỉ gồm chữ và số, gạch ngang và gạch dưới")]
        public string UserName { get; set; }

        [RegularExpression(@"^.*(?=.{5,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[_!@#$%^&+=]).*$", ErrorMessage = "Mật khẩu dài 5 đến 18 ký tự. Bao gồm ít nhất 1 ký tự hoa, 1 ký tự thường và 1 ký tự đặc biệt (_!@#$%^&+=)")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu và mật khẩu không trùng khớp")]
        [RegularExpression(@"^.*(?=.{5,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[_!@#$%^&+=]).*$", ErrorMessage = "Mật khẩu dài 5 đến 18 ký tự. Bao gồm ít nhất 1 ký tự hoa, 1 ký tự thường và 1 ký tự đặc biệt (_!@#$%^&+=)")]
        public string Password2 { get; set; }

        [Required(ErrorMessage = "Họ tên không được rỗng")]
        public String FullName { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public Boolean IsActive { get; set; }

        public List<SelectOptionModel> SelectedRoles { get; set; }

        public EditUserModel()
        {
            SelectedRoles = new List<SelectOptionModel>();
        }
    }

   
}