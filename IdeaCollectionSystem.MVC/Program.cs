using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connections
builder.Services.AddDbContext<IdeaCollectionDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("IdeaCollectionDbContext")));

builder.Services.AddDbContext<IdeaCollectionIdentityDbContext>(options =>
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("IdeaCollectionIdentityDbContext"),
		x => x.MigrationsHistoryTable("__EFMigrationsHistory_Identity")
	));

// 2. Identity Configuration
builder.Services.AddIdentity<IdeaUser, IdeaRole>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 8;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;
	options.User.RequireUniqueEmail = true;
	options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<IdeaCollectionIdentityDbContext>()
.AddDefaultTokenProviders();

// 3. Cookie & Auth Configuration
builder.Services.ConfigureApplicationCookie(options =>
{
	// Đảm bảo các đường dẫn này tồn tại trong AccountController của bạn
	options.LoginPath = "/Account/Login";
	options.AccessDeniedPath = "/Account/AccessDenied";
	options.SlidingExpiration = true;
	options.ExpireTimeSpan = TimeSpan.FromHours(2);
});

// 4. Policy Configuration
builder.Services.AddAuthorization(options =>
{
	// Lưu ý: Kiểm tra lại xem trong RoleConstants của bạn là "Admin" hay "Administrator"
	options.AddPolicy(PolicyConstants.AdminOnly, p => p.RequireRole(RoleConstants.Administrator));
	options.AddPolicy(PolicyConstants.QAManagerOnly, p => p.RequireRole(RoleConstants.QAManager));
	options.AddPolicy(PolicyConstants.QACoordinatorOnly, p => p.RequireRole(RoleConstants.QACoordinator));
	options.AddPolicy(PolicyConstants.StaffOnly, p => p.RequireRole(RoleConstants.Staff));
	options.AddPolicy(PolicyConstants.AllStaff, p => p.RequireAuthenticatedUser());
	options.AddPolicy(PolicyConstants.CanManageCategories, p => p.RequireRole(RoleConstants.Administrator, RoleConstants.QAManager));
	options.AddPolicy(PolicyConstants.CanExportData, p => p.RequireRole(RoleConstants.Administrator, RoleConstants.QAManager));
	options.AddPolicy(PolicyConstants.CanManageUsers, p => p.RequireRole(RoleConstants.Administrator));
	options.AddPolicy(PolicyConstants.CanSetClosureDates, p => p.RequireRole(RoleConstants.Administrator, RoleConstants.QAManager));
});

// 5. Services Injection (QUAN TRỌNG: Phải có IEmailService)
builder.Services.AddScoped<IIdeaService, IdeaService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IEmailService, EmailService>(); // THÊM DÒNG NÀY ĐỂ KHÔNG LỖI

// 6. Caching & Session
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

// 7. Seeding Data
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		await RoleSeeder.InitializeAsync(services);
		Console.WriteLine("Database seeding completed successfully!");
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while seeding the database.");
	}
}

// 8. HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// UseSession PHẢI nằm sau UseRouting và trước UseAuthorization
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// 9. Routing (Đã tối ưu thứ tự)
app.MapControllerRoute(
	name: "areas",
	pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();