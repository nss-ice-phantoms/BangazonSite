using System.Collections.Generic;
using Bangazon.Models;
using Bangazon.Data;
using System.ComponentModel.DataAnnotations;

namespace Bangazon.Models.ProductViewModels
{
  public class ProductTypesViewModel
  {
    public List<GroupedProducts> GroupedProducts { get; set; }
    
  }
}