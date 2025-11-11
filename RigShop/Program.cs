using Microsoft.EntityFrameworkCore;
using RigShop.Web.Models;   

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession();


// add MVC
builder.Services.AddControllersWithViews();

// add EF Core + SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// rest of the pipeline…
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// DbContext definition (can be moved to its own file, but keeping here for clarity in Step 2)
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // DbSets = tables
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();
    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    base.OnModelCreating(modelBuilder);

    //    // HasData requires FIXED primary keys you control
    //    modelBuilder.Entity<Category>().HasData(
    //        new Category { Id = 1, Name = "PC Parts" },
    //        new Category { Id = 2, Name = "GPUs" },
    //        new Category { Id = 3, Name = "CPUs" },
    //        new Category { Id = 4, Name = "Motherboards" },
    //        new Category { Id = 5, Name = "Memory (RAM)" },
    //        new Category { Id = 6, Name = "Storage" },
    //        new Category { Id = 7, Name = "Peripherals" },
    //        new Category { Id = 8, Name = "Monitors" }
    //    );
    //}
}