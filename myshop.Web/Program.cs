using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using myshop.Application.Contracts;
using myshop.Application.Mapping;
using myshop.Application.Services.Category;
using myshop.Application.Services.Product;
using myshop.Application.Services.User;
using myshop.Domain.Constants;
using myshop.Infrastructure.Data;
using myshop.Infrastructure.Identity;
using myshop.Infrastructure.Repositories;
using myshop.Web.Services;
using Stripe;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    )) ;

// Add repositories to the container.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();


// Add services to the container.
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, myshop.Application.Services.Product.ProductService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

builder.Services.AddScoped<IFileService, LocalFileService>();
builder.Services.AddScoped<ICartService, SessionCartService>();
builder.Services.AddAutoMapper(cfg => { }, typeof(CategoryProfile).Assembly);
builder.Services.AddMemoryCache();


builder.Services.AddIdentity<ApplicationUser,IdentityRole>(
    options=>options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(4)
    ).AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnlyPolicy", policy => policy.RequireRole(Roles.Admin));
});


builder.Services.AddHttpContextAccessor();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".ShopHub.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await IdentitySeeder.SeedRolesAndAdminAsync(roleManager, userManager, builder.Configuration);
}





// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

