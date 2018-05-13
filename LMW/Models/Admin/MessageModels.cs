using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LMW.Models.Database;

namespace LMW.Models.Admin
{

    public class CreateMessageModel
    {
        [Required(ErrorMessage = "Số điện thoại không được rỗng")]
        public string Phones { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được rỗng")]
        public string Title { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "Nội dung không được rỗng")]
        public string Message { get; set; }
    }

    public class EditMessageModel
    {
        [Key]
        public int MessageId { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được rỗng")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được rỗng")]
        public string Title { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "Nội dung không được rỗng")]
        public string Message { get; set; }
    }

    public class ListMessageModel
    {
        public DateTime Date1 { get; set; }
        public DateTime Date2 { get; set; }
        public string Filter { get; set; }
        public int Status { get; set; }
        public IEnumerable<Message> Items { get; set; }
        public IEnumerable<SelectListItem> StatusOptions { get; set; }
        public Pager Pager { get; set; }

        public ListMessageModel()
        {
            Items = new List<Message>();
            StatusOptions = new List<SelectListItem>();
        }
    }


}