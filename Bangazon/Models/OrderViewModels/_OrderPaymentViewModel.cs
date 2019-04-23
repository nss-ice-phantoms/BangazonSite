using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon.Models.OrderViewModels
{
    public class OrderPaymentViewModel
    {
        public Order Order { get; set; }
        public SelectList PaymentOptions { get; set; }
    }
}
