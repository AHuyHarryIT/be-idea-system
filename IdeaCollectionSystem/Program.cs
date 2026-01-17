using IdeaCollectionSystem.Datalayer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<IdeaCollectionDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("IdeaCollectionDbContext"))
);
//builder.Services.AddDbContext<VideoIdentityDbContext>(options =>
//	options.UseSqlServer(builder.Configuration.GetConnectionString("VideoIdentityConnection"))
//);

//builder.Services.AddDefaultIdentity<VideoUser>(options =>
//builder.Services.AddIdentity<VideoUser, VideoRole>(options =>
//{
//	options.SignIn.RequireConfirmedAccount = true;
//	// Password settings 
//	options.Password.RequireDigit = true;
//	options.Password.RequiredLength = 8;
//	options.Password.RequireNonAlphanumeric = false;
//	options.Password.RequireUppercase = true;
//	options.Password.RequireLowercase = true;
//	// Lockout settings 
//	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
//	options.Lockout.MaxFailedAccessAttempts = 5;
//	options.Lockout.AllowedForNewUsers = true;
//	// User settings 
//	options.User.RequireUniqueEmail = true;
//	// Sign-in settings 
//	options.SignIn.RequireConfirmedEmail = false;
//	options.SignIn.RequireConfirmedPhoneNumber = false;
//})
//	.AddEntityFrameworkStores<VideoIdentityDbContext>()
//	.AddDefaultTokenProviders();

//builder.Services.ConfigureApplicationCookie(options =>
//{
//	options.LoginPath = "/Identity/Account/Login";
//	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
//});

// 1. Memory Cache (bắt buộc cho Session)
//builder.Services.AddDistributedMemoryCache();
// 2. Add Session
//builder.Services.AddSession(options =>
//{
//	options.IdleTimeout = TimeSpan.FromMinutes(60);
//	options.Cookie.HttpOnly = true;
//	options.Cookie.IsEssential = true;
//});


builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

// 3. Use Session (phải trước MapControllers)
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
