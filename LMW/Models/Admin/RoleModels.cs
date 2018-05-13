using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using LMW.Models.Database;

namespace LMW.Models.Admin
{
    public class ListRoleModel
    {
        public string Filter { get; set; }
        public List<Role> Items { get; set; }
        public Pager Pager { get; set; }
    }

    public class CreateRoleModel
    {
        [Required (ErrorMessage = "Tên chức danh không được rỗng")]
        public String RoleName { get; set; }
        public String Description { get; set; }
    }

    public class EditRoleModel
    {
        [Key]
        public int RoleId { get; set; }
        [Required (ErrorMessage = "Tên chức danh không được rỗng")]
        public String RoleName { get; set; }
        public String Description { get; set; }
    }
}