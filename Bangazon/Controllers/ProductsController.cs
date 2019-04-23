using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Bangazon.Models.ProductViewModels;

namespace Bangazon.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context) {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index() {
            var applicationDbContext = _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) {
                return NotFound();
            }

            return View(product);
        }

        public async Task<IActionResult> Types() {


            var model = new ProductTypesViewModel();

            var GroupedProducts = await (
                from t in _context.ProductType
                join p in _context.Product
                on t.ProductTypeId equals p.ProductTypeId
                group new { t, p } by new { t.ProductTypeId, t.Label } into grouped
                select new GroupedProducts {
                    TypeId = grouped.Key.ProductTypeId,
                    TypeName = grouped.Key.Label,
                    ProductCount = grouped.Select(x => x.p.ProductId).Count(),
                    Products = grouped.Select(x => x.p).Take(3).ToList()
                }).ToListAsync();

            model.GroupedProducts = GroupedProducts;
            return View(model);
        }
    }
}
