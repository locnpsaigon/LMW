using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using LMW.Models.Database;

namespace LMW.Models.Admin
{

    #region Groups

    public class ListServiceGroupsModel
    {
        public string Filter { get; set; }
        public List<CarServiceGroup> Items { get; set; }
        public Pager Pager { get; set; }
    }

    public class CreateServiceGroupModel
    {
        [Required(ErrorMessage ="Tên nhóm dịch vụ không được rỗng")]
        public string groupName { get; set; }
        public string iconURL { get; set; }
        public string description { get; set; }
        public string fullDescription { get; set; }
    }

    public class EditServiceGroupModel
    {
        [Key]
        public int groupId { get; set; }
        [Required(ErrorMessage = "Tên nhóm dịch vụ không được rỗng")]
        public string groupName { get; set; }
        public string iconURL { get; set; }
        public string description { get; set; }
        public string fullDescription { get; set; }
    }

    #endregion

    #region Services 

    public class ListServiceModel
    {
        public string Filter { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
        public List<CarService> Items { get; set; }
        public Pager Pager { get; set; }
    }

    public class CreateServiceModel
    {
        [Required(ErrorMessage = "Tên dịch vụ không được rỗng")]
        public string serviceName { get; set; }
        [Required(ErrorMessage = "Nhóm dịch vụ không được rỗng")]
        public int groupId { get; set; }
        public string groupName { get; set; }
        [Required(ErrorMessage = "Đường dẩn ảnh icon không được rỗng")]
        public string iconURL { get; set; }
        public string description { get; set; }
    }

    public class EditServiceModel
    {
        [Key]
        public int serviceId { get; set; }
        [Required(ErrorMessage = "Tên dịch vụ không được rỗng")]
        public string serviceName { get; set; }
        [Required(ErrorMessage = "Nhóm dịch vụ không được rỗng")]
        public int groupId { get; set; }
        public string groupName { get; set; }
        [Required(ErrorMessage = "Đường dẩn ảnh icon không được rỗng")]
        public string iconURL { get; set; }
        public string description { get; set; }
    }

    #endregion

    #region Service Details

    public class ListServiceDetailsModel
    {
        public string Filter { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
        public int serviceId { get; set; }
        public string serviceName { get; set; }
        public List<CarServiceDetails> Items { get; set; }
        public Pager Pager { get; set; }
    }

    public class CreateServiceDetailsModel
    {
        [Required(ErrorMessage = "Tên dịch vụ không được rỗng")]
        public string itemName { get; set; }

        [Required(ErrorMessage = "Đơn vị tính không được rỗng")]
        public string unit { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm không được rỗng")]
        public string price { get; set; }

        [Required(ErrorMessage = "Nguyên giá không được rỗng")]
        public string priceOriginal { get; set; }

        public int groupId { get; set; }

        public string groupName { get; set; }

        [Required(ErrorMessage = "Dịch vụ không được rỗng")]
        public int serviceId { get; set; }

        public string serviceName { get; set; }
    }

    public class EditServiceDetailsModel
    {
        [Key]
        public int itemId { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ không được rỗng")]
        public string itemName { get; set; }

        [Required(ErrorMessage = "Đơn vị tính không được rỗng")]
        public string unit { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm không được rỗng")]
        public string price { get; set; }

        [Required(ErrorMessage = "Nguyên giá không được rỗng")]
        public string priceOriginal { get; set; }

        public int groupId { get; set; }

        public string groupName { get; set; }

        [Required(ErrorMessage = "Mã dịch vụ không được rỗng")]
        public int serviceId { get; set; }

        public string serviceName { get; set; }
    }

    #endregion
}