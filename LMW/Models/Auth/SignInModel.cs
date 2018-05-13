using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LMW.Models.Auth
{
    public class SignInModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được rỗng")]
        [RegularExpression(@"^[a-z0-9_-]{2,15}$", ErrorMessage = "Tên đăng nhập dài 2 ~ 15 gồm ký tự alphabet, chữ số, gạch ngang, gạch dưới!")]
        public String UserName { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được rỗng")]
        public String Password { get; set; }

        public Boolean RememberMe { get; set; }

        public SignInModel()
        {
            UserName = "";
            Password = "";
            RememberMe = true;
        }
    }
}