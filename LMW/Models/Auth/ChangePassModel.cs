using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LMW.Models.Auth
{
    public class ChangePassModel
    {
        [Key]
        public int UserId { get; set; }

        public string UserName { get; set; }

        [Required(ErrorMessage = "Mật khẩu hiện tại không được rỗng")]
        [RegularExpression(@"^.*(?=.{5,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[_!@#$%^&+=]).*$", ErrorMessage = "Mật khẩu dài 5 đến 18 ký tự. Bao gồm ít nhất 1 ký tự hoa, 1 ký tự thường và 1 ký tự đặc biệt (_!@#$%^&+=)")]
        public string PasswordCurrent { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được rỗng")]
        [RegularExpression(@"^.*(?=.{5,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[_!@#$%^&+=]).*$", ErrorMessage = "Mật khẩu dài 5 đến 18 ký tự. Bao gồm ít nhất 1 ký tự hoa, 1 ký tự thường và 1 ký tự đặc biệt (_!@#$%^&+=)")]
        public string PasswordNew { get; set; }

        [Compare("PasswordNew", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không trùng khớp")]
        public string PasswordConfirm { get; set; }
    }
}