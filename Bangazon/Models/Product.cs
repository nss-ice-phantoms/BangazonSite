using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bangazon.Models
{
  public class Product
  {
    [Key]
    public int ProductId {get;set;}

    [Required]
    [DataType(DataType.Date)]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime DateCreated {get;set;}

    [Required]
    [StringLength(255)]
    [RegularExpression(@"^([a-zA-Z0-9_\s\-]*)$",
        ErrorMessage = "Characters are not allowed.")]
    public string Description { get; set; }

    [Required]
    [StringLength(55, ErrorMessage="Please shorten the product title to 55 characters")]
    [RegularExpression(@"^([a-zA-Z0-9_\s\-]*)$",
    ErrorMessage = "Characters are not allowed.")]
    public string Title { get; set; }

    [Required]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public double Price { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public string UserId {get; set;}

    public string City {get; set;}

    public string ImagePath {get; set;}

    [Required]
    public ApplicationUser User { get; set; }

    [Required(ErrorMessage = "Please Select a Product Category")]
    [Display(Name="Product Category")]
    public int ProductTypeId { get; set; }

    public ProductType ProductType { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; }

  }
}
