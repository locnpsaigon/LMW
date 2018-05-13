using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMW.Models.Mobile
{
    public class CarServiceDetailsResult
    {
        public int itemId { get; set; }
        public string itemName { get; set; }
        public int serviceId { get; set; }
        public string serviceName { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
        public string unit { get; set; }
        public decimal price { get; set; }
        public decimal priceOriginal { get; set; }
    }

    public class CarServiceResult
    {
        public int serviceId { get; set; }
        public string serviceName { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
        public string iconURL { get; set; }
        public string description { get; set; }
        public IList<CarServiceDetailsResult> serviceDetails;

        public CarServiceResult()
        {
            serviceDetails = new List<CarServiceDetailsResult>();
        }
    }

    public class CarServiceGroupResult
    {
        public int groupId { get; set; }
        public string groupName { get; set; }
        public string iconURL { get; set; }
        public string description { get; set; }
        public string fullDescription { get; set; }
        public IList<CarServiceResult> services;

        public CarServiceGroupResult()
        {
            services = new List<CarServiceResult>();
        }
    }

    public class RegisterCustomerResult
    {
        public int ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
    }

    public class UpdateMessageStatusResult
    {
        public int ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
    }

    public class MessageStatusResult
    {
        public int Read { get; set; }
        public int Unread { get; set; }
    }

    public class SupportInfoResult
    {
        public int supportId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

}