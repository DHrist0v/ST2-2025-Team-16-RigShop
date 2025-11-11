using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace RigShop.Web.Controllers;

public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;
    public OrdersController(ApplicationDbContext db) => _db = db;

    private int? CurrentUserId() => HttpContext.Session.GetInt32("UserId");
    private bool IsAdmin()
        => bool.TryParse(HttpContext.Session.GetString("IsAdmin"), out var a) && a;

    // GET: /Orders
    public async Task<IActionResult> Index()
    {
        var userId = CurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Auth");

        // non-admins see only their orders, admins could see all
        var query = _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!IsAdmin())
            query = query.Where(o => o.UserId == userId.Value);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    // GET: /Orders/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = CurrentUserId();
        if (userId is null) return RedirectToAction("Login", "Auth");

        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        // security: users can only see their own order; admin can see any
        if (!IsAdmin() && order.UserId != userId.Value)
            return Forbid();

        return View(order);
    }
}