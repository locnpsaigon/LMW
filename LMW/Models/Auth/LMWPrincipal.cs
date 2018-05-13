using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace LMW.Models.Auth
{
    public class LMWPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }

        public int UserId { get; set; }

        public string FullName { get; set; }

        public String[] Roles { get; set; }

        public LMWPrincipal(string username)
        {
            this.Identity = new GenericIdentity(username);
        }

        public Boolean IsInRole(string userRoles)
        {
            foreach (var role in Roles)
            {
                if (userRoles.Contains(role))
                {
                    return true;
                }
            }
            return false;
        }
    }
}