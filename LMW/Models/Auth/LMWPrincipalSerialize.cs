﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMW.Models.Auth
{
    public class LMWPrincipalSerialize
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public String[] Roles { get; set; }
    }
}