
using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.MVC.Areas.Identity.Data;
using IdeaCollectionSystem.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);


// DbContext (Business)
var ideaCollectionConnectionString = builder.Configuration.GetConnectionString("IdeaCollectionDbContext");
var ideaIdentityConnectionString = builder.Configuration.GetConnectionString("IdeaIdentityConnection");

builder.Services.AddDbContext<IdeaCollectionDbContext>(options =>
	ConfigureDbContext(options, ideaCollectionConnectionString));

// DbContext (Identity)
builder.Services.AddDbContext<IdeaCollectionIdentityDbContext>(options =>
	ConfigureDbContext(options, ideaIdentityConnectionString));


// Identity 

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
	options.SlidingExpiration = true;
	options.ExpireTimeSpan = TimeSpan.FromHours(2);
});


// AUTHORIZATION POLICIES

builder.Services.AddAuthorizationBuilder()
							 // AUTHORIZATION POLICIES
							 .AddPolicy(PolicyConstants.AdminOnly, policy =>
		policy.RequireRole(RoleConstants.Administrator))
							 // AUTHORIZATION POLICIES
							 .AddPolicy(PolicyConstants.QAManagerOnly, policy =>
		policy.RequireRole(RoleConstants.QAManager))
							 // AUTHORIZATION POLICIES
							 .AddPolicy(PolicyConstants.QACoordinatorOnly, policy =>
		policy.RequireRole(RoleConstants.QACoordinator))
							 // AUTHORIZATION POLICIES
							 .AddPolicy(PolicyConstants.StaffOnly, policy =>
		policy.RequireRole(RoleConstants.Staff))
							 // AUTHORIZATION POLICIES
							 .AddPolicy(PolicyConstants.QAManagement, policy =>
		policy.RequireRole(RoleConstants.QAManager, RoleConstants.QACoordinator))
							 // AUTHORIZATION POLICIES
							 .AddPolicy(PolicyConstants.AllStaff, policy =>
		policy.RequireAuthenticatedUser())
							 // AUTHORIZATION POLICIES
							 .AddPolicy(PolicyConstants.CanManageCategories, policy =>
		policy.RequireRole(RoleConstants.Administrator, RoleConstants.QAManager))
							 // AUTHORIZATION POLICIES
							 .AddPolicy(PolicyConstants.CanExportData, policy =>
		policy.RequireRole(RoleConstants.Administrator, RoleConstants.QAManager))
							 // AUTHORIZATION POLICIES
							 .AddPolicy(PolicyConstants.CanManageUsers, policy =>
		policy.RequireRole(RoleConstants.Administrator))
							 // AUTHORIZATION POLICIES
							 .AddPolicy(PolicyConstants.CanSetClosureDates, policy =>
		policy.RequireRole(RoleConstants.Administrator, RoleConstants.QAManager));

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


// SEED DATA

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;

	try
	{
		var roleManager = services.GetRequiredService<RoleManager<IdeaRole>>();
		var userManager = services.GetRequiredService<UserManager<IdeaUser>>();
		var dbContext = services.GetRequiredService<IdeaCollectionDbContext>();
		var identityDbContext = services.GetRequiredService<IdeaCollectionIdentityDbContext>();

		// Ensure databases are created
		await EnsureDatabaseAsync(dbContext);
		await EnsureDatabaseAsync(identityDbContext);

		// Seed Roles
		await SeedRolesAsync(roleManager);

		// Seed Departments (if needed)
		await SeedDepartmentsAsync(dbContext);

		// Seed Demo Users
		await SeedDemoUsersAsync(userManager, dbContext);

		Console.WriteLine("✅ Database seeding completed successfully!");
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "❌ An error occurred while seeding the database.");
	}
}


// MIDDLEWARE

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
	name: "admin",
	pattern: "Admin/{action=Dashboard}/{id?}",
	defaults: new { controller = "Admin" });

app.MapControllerRoute(
	name: "qamanager",
	pattern: "QAManager/{action=Dashboard}/{id?}",
	defaults: new { controller = "QAManager" });

app.MapControllerRoute(
	name: "qacoordinator",
	pattern: "QACoordinator/{action=Dashboard}/{id?}",
	defaults: new { controller = "QACoordinator" });

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();


// SEEDING METHODS

static void ConfigureDbContext(DbContextOptionsBuilder options, string? connectionString)
{
	if (string.IsNullOrWhiteSpace(connectionString))
	{
		throw new InvalidOperationException("Database connection string is not configured.");
	}

	if (IsSqliteConnectionString(connectionString))
	{
		options.UseSqlite(connectionString);
	}
	else
	{
		options.UseNpgsql(connectionString);
	}
}

static bool IsSqliteConnectionString(string connectionString)
{
	try
	{
		var npgsqlBuilder = new NpgsqlConnectionStringBuilder(connectionString);
		if (!string.IsNullOrWhiteSpace(npgsqlBuilder.Host))
		{
			return false;
		}
	}
	catch (ArgumentException)
	{
	}

	try
	{
		var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);
		return !string.IsNullOrWhiteSpace(sqliteBuilder.DataSource);
	}
	catch (ArgumentException)
	{
		return false;
	}
	catch (FormatException)
	{
		return false;
	}
}

static async Task EnsureDatabaseAsync(DbContext dbContext)
{
	var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
	if (pendingMigrations.Any())
	{
		await dbContext.Database.MigrateAsync();
		return;
	}

	var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
	if (appliedMigrations.Any())
	{
		return;
	}

	await dbContext.Database.EnsureCreatedAsync();
}

static async Task SeedRolesAsync(RoleManager<IdeaRole> roleManager)
{
	foreach (var roleName in RoleConstants.GetAllRoles())
	{
		if (!await roleManager.RoleExistsAsync(roleName))
		{
			var role = new IdeaRole
			{
				Name = roleName,
				Description = RoleConstants.RoleDescriptions[roleName]
			};

			await roleManager.CreateAsync(role);
		}
	}
}

static async Task SeedDepartmentsAsync(IdeaCollectionDbContext context)
{
	if (!context.Departments.Any())
	{
		var departments = new[]
		{
			new Department { Id = Guid.NewGuid(), Name = "Computer Science", Description = "CS Department" },
			new Department { Id = Guid.NewGuid(), Name = "Business", Description = "Business Department" },
			new Department { Id = Guid.NewGuid(), Name = "Engineering", Description = "Engineering Department" }
		};

		await context.Departments.AddRangeAsync(departments);
		await context.SaveChangesAsync();
	}
}

static async Task SeedDemoUsersAsync(UserManager<IdeaUser> userManager, IdeaCollectionDbContext context)
{
	var defaultPassword = "Admin@123";

	// Admin User
	if (await userManager.FindByEmailAsync("admin@university.edu") == null)
	{
		var admin = new IdeaUser
		{
			UserName = "admin@university.edu",
			Email = "admin@university.edu",
			EmailConfirmed = true,
			Name = "System Administrator",
			Avatar = "/images/default-avatar.png"
		};

		var result = await userManager.CreateAsync(admin, defaultPassword);
		if (result.Succeeded)
		{
			await userManager.AddToRoleAsync(admin, RoleConstants.Administrator);
		}
	}

	// QA Manager
	if (await userManager.FindByEmailAsync("qamanager@university.edu") == null)
	{
		var qaManager = new IdeaUser
		{
			UserName = "qamanager@university.edu",
			Email = "qamanager@university.edu",
			EmailConfirmed = true,
			Name = "QA Manager",
			Avatar = "/images/default-avatar.png"
		};

		var result = await userManager.CreateAsync(qaManager, defaultPassword);
		if (result.Succeeded)
		{
			await userManager.AddToRoleAsync(qaManager, RoleConstants.QAManager);
		}
	}

	// QA Coordinator
	if (await userManager.FindByEmailAsync("qacoordinator@university.edu") == null)
	{
		var qaCoordinator = new IdeaUser
		{
			UserName = "qacoordinator@university.edu",
			Email = "qacoordinator@university.edu",
			EmailConfirmed = true,
			Name = "QA Coordinator",
			Avatar = "/images/default-avatar.png"
		};

		var result = await userManager.CreateAsync(qaCoordinator, defaultPassword);
		if (result.Succeeded)
		{
			await userManager.AddToRoleAsync(qaCoordinator, RoleConstants.QACoordinator);
		}
	}

	// Staff User
	if (await userManager.FindByEmailAsync("staff@university.edu") == null)
	{
		var staff = new IdeaUser
		{
			UserName = "staff@university.edu",
			Email = "staff@university.edu",
			EmailConfirmed = true,
			Name = "Staff Member",
			Avatar = "/images/default-avatar.png"
		};

		var result = await userManager.CreateAsync(staff, defaultPassword);
		if (result.Succeeded)
		{
			await userManager.AddToRoleAsync(staff, RoleConstants.Staff);
		}
	}
}
