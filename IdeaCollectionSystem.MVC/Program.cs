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

builder.Services.AddDbContext<IdeaCollectionDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("IdeaCollectionDbContext")));

builder.Services.AddDbContext<IdeaCollectionIdentityDbContext>(options =>
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("IdeaCollectionIdentityDbContext"),
		x => x.MigrationsHistoryTable("__EFMigrationsHistory_Identity") 
	));

builder.Services.AddIdentity<IdeaUser, IdeaRole>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 8;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.AllowedForNewUsers = true;
	options.User.RequireUniqueEmail = true;
	options.SignIn.RequireConfirmedAccount = false;
	options.SignIn.RequireConfirmedEmail = false;
	options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<IdeaCollectionIdentityDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Identity/Account/Login";
	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
	options.SlidingExpiration = true;
	options.ExpireTimeSpan = TimeSpan.FromHours(2);
});

builder.Services.AddScoped<IIdeaService, IdeaService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQAManagerService, QAManagerService>();

builder.Services.AddAuthorizationBuilder()
	.AddPolicy(PolicyConstants.AdminOnly, p => p.RequireRole(RoleConstants.Administrator))
	.AddPolicy(PolicyConstants.QAManagerOnly, p => p.RequireRole(RoleConstants.QAManager))
	.AddPolicy(PolicyConstants.QACoordinatorOnly, p => p.RequireRole(RoleConstants.QACoordinator))
	.AddPolicy(PolicyConstants.StaffOnly, p => p.RequireRole(RoleConstants.Staff))
	.AddPolicy(PolicyConstants.QAManagement, p => p.RequireRole(RoleConstants.QAManager, RoleConstants.QACoordinator))
	.AddPolicy(PolicyConstants.AllStaff, p => p.RequireAuthenticatedUser())
	.AddPolicy(PolicyConstants.CanManageCategories, p => p.RequireRole(RoleConstants.Administrator, RoleConstants.QAManager))
	.AddPolicy(PolicyConstants.CanExportData, p => p.RequireRole(RoleConstants.Administrator, RoleConstants.QAManager))
	.AddPolicy(PolicyConstants.CanManageUsers, p => p.RequireRole(RoleConstants.Administrator))
	.AddPolicy(PolicyConstants.CanSetClosureDates, p => p.RequireRole(RoleConstants.Administrator, RoleConstants.QAManager));

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

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var roleManager = services.GetRequiredService<RoleManager<IdeaRole>>();
		var userManager = services.GetRequiredService<UserManager<IdeaUser>>();
		var dbContext = services.GetRequiredService<IdeaCollectionDbContext>();
		var identityDbContext = services.GetRequiredService<IdeaCollectionIdentityDbContext>();

		//await dbContext.Database.MigrateAsync();
		//await identityDbContext.Database.MigrateAsync();

		await SeedRolesAsync(roleManager);
		await SeedDepartmentsAsync(dbContext);        
		await SeedDemoUsersAsync(userManager, dbContext);

		Console.WriteLine("Database seeding completed successfully!");
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while seeding the database.");
	}
}

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

app.MapControllerRoute(name: "admin", pattern: "Admin/{action=Dashboard}/{id?}", defaults: new { controller = "Admin" });
app.MapControllerRoute(name: "qamanager", pattern: "QAManager/{action=Dashboard}/{id?}", defaults: new { controller = "QAManager" });
app.MapControllerRoute(name: "qacoordinator", pattern: "QACoordinator/{action=Dashboard}/{id?}", defaults: new { controller = "QACoordinator" });
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "staff", pattern: "Staff/{action=Dashboard}/{id?}", defaults: new { controller = "Staff" });
app.MapRazorPages();
app.Run();


static async Task SeedRolesAsync(RoleManager<IdeaRole> roleManager)
{
	foreach (var roleName in RoleConstants.GetAllRoles())
	{
		if (!await roleManager.RoleExistsAsync(roleName))
		{
			await roleManager.CreateAsync(new IdeaRole
			{
				Name = roleName,
				Description = RoleConstants.RoleDescriptions[roleName]
			});
		}
	}
}

static async Task SeedDepartmentsAsync(IdeaCollectionDbContext context)
{
	if (!context.Departments.Any())
	{
		await context.Departments.AddRangeAsync(
			new Department { Id = Guid.NewGuid(), Name = "Computer Science", Description = "CS Department" },
			new Department { Id = Guid.NewGuid(), Name = "Business", Description = "Business Department" },
			new Department { Id = Guid.NewGuid(), Name = "Engineering", Description = "Engineering Department" }
		);
		await context.SaveChangesAsync();
	}
}

static async Task SeedDemoUsersAsync(UserManager<IdeaUser> userManager, IdeaCollectionDbContext context)
{
	var defaultPassword = "Admin@123";
	var firstDept = context.Departments.FirstOrDefault();

	async Task Create(string email, string role, string name)
	{
		if (await userManager.FindByEmailAsync(email) != null) return;
		var user = new IdeaUser
		{
			UserName = email,
			Email = email,
			EmailConfirmed = true,
			Name = name,
			Avatar = "/images/default-avatar.png",
			DepartmentId = firstDept?.Id   // ← gán DepartmentId ngay khi tạo user
		};
		var result = await userManager.CreateAsync(user, defaultPassword);
		if (result.Succeeded)
			await userManager.AddToRoleAsync(user, role);
	}

	await Create("admin@university.edu", RoleConstants.Administrator, "System Administrator");
	await Create("qamanager@university.edu", RoleConstants.QAManager, "QA Manager");
	await Create("qacoordinator@university.edu", RoleConstants.QACoordinator, "QA Coordinator");
	await Create("staff@university.edu", RoleConstants.Staff, "Staff Member");
}