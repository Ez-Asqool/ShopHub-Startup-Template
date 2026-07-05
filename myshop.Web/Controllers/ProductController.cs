using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using myshop.Domain.Entities;
using myshop.Entities.Models;
using myshop.Infrastructure.Data;
using myshop.Web.ViewModels;

namespace myshop.Web.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetData()
        {
            var products = _context.Products
                .Include(x => x.Category)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    description = x.Description,
                    price = x.Price,
                    categoryName = x.Category.Name
                })
                .ToList();

            return Json(new { data = products });
        }

        [HttpGet]
        public IActionResult Create()
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _context.Categories.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Create(ProductVM productVM,IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString();
                    var Upload = Path.Combine(RootPath, @"Images\Products");
                    var ext = Path.GetExtension(file.FileName);

                    using (var filestream = new FileStream(Path.Combine(Upload,filename+ext),FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    productVM.Product.Img = @"Images\Products\" + filename + ext;
                }

                _context.Products.Add(productVM.Product);
                _context.SaveChanges();
                TempData["Create"] = "Item has Created Successfully";
                return RedirectToAction("Index");
            }
            return View(productVM.Product);
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            ProductVM productVM = new ProductVM()
            {
                Product = _context.Products.FirstOrDefault(x => x.Id == id),
                CategoryList = _context.Categories.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            return View(productVM);
        }
        
        [HttpPost]
        public IActionResult Edit(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString();
                    var Upload = Path.Combine(RootPath, @"Images\Products");
                    var ext = Path.GetExtension(file.FileName);

                    if (productVM.Product.Img != null)
                    {
                        var oldimg = Path.Combine(RootPath, productVM.Product.Img.TrimStart('\\'));

                        if (System.IO.File.Exists(oldimg))
                        {
                            System.IO.File.Delete(oldimg);
                        }
                    }

                    using (var filestream = new FileStream(Path.Combine(Upload, filename + ext), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }

                    productVM.Product.Img = @"Images\Products\" + filename + ext;
                }

                _context.Products.Update(productVM.Product);
                _context.SaveChanges();

                TempData["Update"] = "Data has Updated Successfully";
                return RedirectToAction("Index");
            }

            return View(productVM.Product);
        }
        
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productIndb = _context.Products.FirstOrDefault(x => x.Id == id);

            if (productIndb == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }

            _context.Products.Remove(productIndb);

            var oldimg = Path.Combine(_webHostEnvironment.WebRootPath, productIndb.Img.TrimStart('\\'));

            if (System.IO.File.Exists(oldimg))
            {
                System.IO.File.Delete(oldimg);
            }

            _context.SaveChanges();

            return Json(new { success = true, message = "file has been Deleted" });
        }

    }
}
