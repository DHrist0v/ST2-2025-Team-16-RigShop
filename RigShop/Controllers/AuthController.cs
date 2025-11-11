using Microsoft.AspNetCore.Mvc;
using RigShop.Web.Models;

namespace RigShop.Web.Controllers;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _db;

    public AuthController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Auth/Register
    [HttpGet]
    public IActionResult Register() => View();

    // POST: /Auth/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // check if email already exists
        if (_db.Users.Any(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email already exists");
            return View(model);
        }

        _db.Users.Add(model);
        await _db.SaveChangesAsync();

        // log them in by saving session info
        HttpContext.Session.SetInt32("UserId", model.Id);
        HttpContext.Session.SetString("UserEmail", model.Email);
        HttpContext.Session.SetString("IsAdmin", model.IsAdmin.ToString());

        return RedirectToAction("Index", "Home");
    }

    // GET: /Auth/Login
    [HttpGet]
    public IActionResult Login() => View();

    // POST: /Auth/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
        if (user == null)
        {
            ViewBag.Error = "Invalid credentials.";
            return View();
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

        return RedirectToAction("Index", "Home");
    }

    // GET: /Auth/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}