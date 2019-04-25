﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Bangazon.Models.ProductViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Bangazon.Controllers
{
    public class ProductsController : Controller
    {
        //setting private reference to the I.D.F usermanager
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _context;

        //Getting the current user in the system (whoever is logged in)
        public Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public ProductsController(ApplicationDbContext context,
                                  UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Products
        public async Task<IActionResult> Index(string searchString, string citySearch)
        {
            if (string.IsNullOrEmpty(searchString) && string.IsNullOrEmpty(citySearch)) {

                var products = await _context.Product
                    .Include(p => p.ProductType)
                    .Include(p => p.User)
                    .OrderByDescending(p => p.DateCreated)
                    .Take(20)
                    .ToListAsync();

                return View(products);

            }

            if (string.IsNullOrEmpty(citySearch) && !string.IsNullOrEmpty(searchString)) {

                var products = await _context.Product
                   .Include(p => p.ProductType)
                   .Include(p => p.User)
                   .OrderByDescending(p => p.DateCreated)
                   .Where(p => p.Title.Contains(searchString))
                   .ToListAsync();
                return View(products);

            }

            if (string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(citySearch)) {

                var products = await _context.Product
                    .Include(p => p.ProductType)
                    .Include(p => p.User)
                    .OrderByDescending(p => p.DateCreated)
                    .Where(p => p.City.Contains(citySearch))
                    .ToListAsync();
                return View(products);
            }

            return View();
        }
        public async Task<IActionResult> DeleteDenied()
        {
            return View();
        }
        //METHOD gets the current user
        [Authorize]
        public async Task<IActionResult> YourProductIndex()
        {
            //setting the user obj to this variable
            var user = await GetCurrentUserAsync();
            //create a userId var that stores the current user Id
            var userId = user.Id;
            //return a view
            return View(await _context.Product.Where(p => p.UserId == userId).ToListAsync());
        }
        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
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

        public async Task<IActionResult> ProductTypes(int id) {

            var ProductsByType = await _context.ProductType
                .Include(pt => pt.Products)
                .FirstOrDefaultAsync(pt => pt.ProductTypeId == id);

            return View(ProductsByType);
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

        // GET: Products/Create
        public IActionResult Create()
        {
            var productTypesComplete = _context.ProductType;

            List<SelectListItem> productTypes = new List<SelectListItem>();

            productTypes.Insert(0, new SelectListItem
            {
                Text = "Assign a Product Category",
                Value = ""
            });

            foreach (var pt in productTypesComplete)
            {
                SelectListItem li = new SelectListItem
                {
                    Value = pt.ProductTypeId.ToString(),
                    Text = pt.Label
                };
                productTypes.Add(li);
            }

            ProductCreateViewModel viewModel = new ProductCreateViewModel();

            // Assign productTypes select list item to the product Types in ProductCreateViewModel
            viewModel.ProductTypeList = productTypes;

            // View Data for dropdowns
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label");
            return View(viewModel);
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel viewModel, IFormFile image)
        {
            ModelState.Remove("Product.User");
            ModelState.Remove("Product.UserId");

            ApplicationUser user = await GetCurrentUserAsync();

            viewModel.Product.User = user;
            viewModel.Product.UserId = user.Id;

            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0) {
                    var fileName = Path.GetFileName(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ProductPhotos", fileName);
                    using (var fileSteam = new FileStream(filePath, FileMode.Create)) {
                        await image.CopyToAsync(fileSteam);
                    }
                    viewModel.Product.ImagePath = fileName;
                }

                _context.Add(viewModel.Product);

                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = viewModel.Product.ProductId.ToString() });
            }

            var productTypesComplete = _context.ProductType;

            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", viewModel.Product.ProductTypeId);


            List<SelectListItem> productTypes = new List<SelectListItem>();

            productTypes.Insert(0, new SelectListItem
            {
                Text = "Assign a Product Category",
                Value = null
            });

            foreach (var pt in productTypesComplete)
            {
                SelectListItem li = new SelectListItem
                {
                    Value = pt.ProductTypeId.ToString(),
                    Text = pt.Label
                };
                productTypes.Add(li);
            }

            viewModel.ProductTypeList = productTypes;

            return View(viewModel);

        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,DateCreated,Description,Title,Price,Quantity,UserId,City,ImagePath,ProductTypeId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }
            ModelState.Remove("User");
            ModelState.Remove("UserId");


            ApplicationUser user = await GetCurrentUserAsync();

            product.User = user;
            product.UserId = user.Id;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(YourProductIndex));
            }
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product
            .Include(p => p.OrderProducts)
            .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product.OrderProducts.Count == 0)
            {
                _context.Product.Remove(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction(nameof(DeleteDenied));
            }

        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}
