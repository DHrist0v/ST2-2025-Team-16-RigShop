using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RigShop.Web.Helpers;
using RigShop.Web.Models;

namespace RigShop.Web.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _db;
    private const string CartKey = "CART";

    public CartController(ApplicationDbContext db) => _db = db;

    private List<CartItem> GetCart()
        => HttpContext.Session.GetObject<List<CartItem>>(CartKey) ?? new List<CartItem>();

    private void SaveCart(List<CartItem> items)
        => HttpContext.Session.SetObject(CartKey, items);

    private int? CurrentUserId() => HttpContext.Session.GetInt32("UserId");
    private bool IsAdmin()
        => bool.TryParse(HttpContext.Session.GetString("IsAdmin"), out var a) && a;

    // GET: /Cart
    public IActionResult Index()
    {
        var cart = GetCart();
        ViewBag.Total = cart.Sum(i => i.LineTotal);
        return View(cart);
    }

    // POST: /Cart/Add/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id, int qty = 1)
    {
        if (IsAdmin())
        {
            // admins manage products, not buy — optional rule
            TempData["Error"] = "Admins cannot add items to cart.";
            return RedirectToAction("Index", "Products");
        }

        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        var cart = GetCart();
        var line = cart.FirstOrDefault(c => c.ProductId == id);
        if (line is null)
        {
            cart.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                UnitPrice = product.Price,
                Quantity = Math.Max(1, qty),
                ImageUrl = product.ImageUrl
            });
        }
        else
        {
            line.Quantity += Math.Max(1, qty);
        }

        SaveCart(cart);
        TempData["Info"] = $"{product.Name} added to cart.";
        return RedirectToAction("Index", "Products");
    }

    // POST: /Cart/Update
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, int qty)
    {
        var cart = GetCart();
        var line = cart.FirstOrDefault(c => c.ProductId == productId);
        if (line != null)
        {
            if (qty <= 0)
            {
                cart.Remove(line);
                TempData["Info"] = $"{line.Name} removed from cart.";
            }
            else
            {
                line.Quantity = qty;
                TempData["Info"] = $"{line.Name} quantity updated to {qty}.";
            }
            SaveCart(cart);
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/Remove/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int id)
    {
        var cart = GetCart();
        cart.RemoveAll(c => c.ProductId == id);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/Checkout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            TempData["Error"] = "Please log in to place an order.";
            return RedirectToAction("Login", "Auth");
        }

        var cart = GetCart();
        if (!cart.Any())
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        // create order + items (snapshot prices)
        var order = new Order { UserId = userId.Value, CreatedAt = DateTime.UtcNow };
        foreach (var line in cart)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice
            });

            // optional: decrement stock
            var p = await _db.Products.FindAsync(line.ProductId);
            if (p != null) p.Stock = Math.Max(0, p.Stock - line.Quantity);
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // clear cart
        SaveCart(new List<CartItem>());

        TempData["Info"] = $"Order #{order.Id} placed!";
        return RedirectToAction("Details", "Orders", new { id = order.Id });
    }
}