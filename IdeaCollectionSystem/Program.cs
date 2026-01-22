using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.MVC.Areas.Identity.Data;
using IdeaCollectionSystem.MVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =======================
// DbContext (Business)
// =======================
builder.Services.AddDbContext<IdeaCollectionDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("IdeaCollectionDbContext")));

// =======================
// DbContext (Identity)
// =======================
builder.Services.AddDbContext<IdeaCollectionIdentityDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("IdeaIdentityConnection")));

// =======================
// Identity (CHUẨN)
// =======================
builder.Services.AddIdentity<IdeaUser, IdeaRole>(options =>
{
	// Password
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 8;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;

	// Lockout
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.AllowedForNewUsers = true;

	// User
	options.User.RequireUniqueEmail = true;

	// Sign in
	options.SignIn.RequireConfirmedAccount = false;
	options.SignIn.RequireConfirmedEmail = false;
	options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<IdeaCollectionIdentityDbContext>()
.AddDefaultTokenProviders();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Identity/Account/Login";
	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(60);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});


builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseSession();         
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
