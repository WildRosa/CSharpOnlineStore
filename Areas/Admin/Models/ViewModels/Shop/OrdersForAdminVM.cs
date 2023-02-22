﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shop.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminVM
    {
        public int OrderNumber { get; set; }
        public string UserName { get; set; }
        public Decimal Total { get; set; }
        public Dictionary<string, int> ProductsAndQty { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}