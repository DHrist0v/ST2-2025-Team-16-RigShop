using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RigShop.Web.Models;

namespace RigShop.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Load Categories for dropdowns
        private async Task LoadCategoriesAsync()
        {
            ViewBag.Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
        }

        public async Task<IActionResult> GetCategories()
        {
            var categories = await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return Json(categories);
        }

        // category filtering
        public async Task<IActionResult> Category(int id)
        {
            var products = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == id)
                .ToListAsync();

            ViewBag.CategoryName = await _db.Categories
                .Where(c => c.Id == id)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

            ViewBag.IsAdmin = IsUserAdmin();
            return View("Index", products);
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            var products = await _db.Products.Include(p => p.Category).ToListAsync();
            ViewBag.IsAdmin = IsUserAdmin();
            return View(products);
        }
        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            ViewBag.IsAdmin = IsUserAdmin();
            return View(product);
        }

        // GET: /Products/Create
        public async Task <IActionResult> Create()
        {
            if (!IsUserAdmin()) return RedirectToAction("Login", "Auth");
            await LoadCategoriesAsync();
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            if (!IsUserAdmin()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            _db.Products.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsUserAdmin()) return RedirectToAction("Login", "Auth");

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            await LoadCategoriesAsync();
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model)
        {
            if (!IsUserAdmin()) return RedirectToAction("Login", "Auth");

            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            _db.Update(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsUserAdmin()) return RedirectToAction("Login", "Auth");

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsUserAdmin()) return RedirectToAction("Login", "Auth");

            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool IsUserAdmin()
        {
            var adminStr = HttpContext.Session.GetString("IsAdmin");
            return bool.TryParse(adminStr, out var isAdmin) && isAdmin;
        }
    }
}