using Microsoft.AspNetCore.Mvc;
using myshop.Domain.Entities;
using myshop.Infrastructure.Data;

namespace myshop.Web.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["Create"] = "Item has Created Successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
            {
                NotFound();
            }
            var categoryIndb = _context.Categories.Find(id);

            return View(categoryIndb);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);

                _context.SaveChanges();
                TempData["Update"] = "Data has Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null | id == 0)
            {
                NotFound();
            }
            var categoryIndb = _context.Categories.Where(x => x.Id == id).FirstOrDefault();

            return View(categoryIndb);
        }

        [HttpPost]
        public IActionResult DeleteCategory(int? id)
        {
            var categoryIndb = _context.Categories.FirstOrDefault(x => x.Id == id);
            if (categoryIndb == null)
            {
                NotFound();
            }
            _context.Categories.Remove(categoryIndb);
            _context.SaveChanges();
            TempData["Delete"] = "Item has Deleted Successfully";
            return RedirectToAction("Index");
        }
    }
}
