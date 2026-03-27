using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdeaCollectionSystem.Datalayer 
{
	public static class RoleSeeder
	{
	
		public static async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var roleManager = serviceProvider.GetRequiredService<RoleManager<IdeaRole>>();
			var userManager = serviceProvider.GetRequiredService<UserManager<IdeaUser>>();
			var context = serviceProvider.GetRequiredService<IdeaCollectionDbContext>();

			await SeedRolesAsync(roleManager);
			await SeedDepartmentsAsync(context);
			await SeedDemoUsersAsync(userManager, context);
		}

		private static async Task SeedRolesAsync(RoleManager<IdeaRole> roleManager)
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

		private static async Task SeedDepartmentsAsync(IdeaCollectionDbContext context)
		{
			if (!await context.Departments.AnyAsync())
			{
				await context.Departments.AddRangeAsync(
					new Department { Id = Guid.NewGuid(), Name = "Computer Science", Description = "CS Department" },
					new Department { Id = Guid.NewGuid(), Name = "Business", Description = "Business Department" },
					new Department { Id = Guid.NewGuid(), Name = "Engineering", Description = "Engineering Department" }
				);
				await context.SaveChangesAsync();
			}
		}

		private static async Task SeedDemoUsersAsync(UserManager<IdeaUser> userManager, IdeaCollectionDbContext context)
		{
			var defaultPassword = "Admin@123";
		
			var firstDept = await context.Departments.FirstOrDefaultAsync();

			async Task Create(string email, string role, string name)
			{
				if (await userManager.FindByEmailAsync(email) != null) return;
				var user = new IdeaUser
				{
					UserName = email,
					Email = email,
					EmailConfirmed = true,
					Name = name,
					DepartmentId = firstDept?.Id
				};
				var result = await userManager.CreateAsync(user, defaultPassword);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(user, role);
				}
			}

			await Create("admin@university.edu", RoleConstants.Administrator, "System Administrator");
			await Create("qamanager@university.edu", RoleConstants.QAManager, "QA Manager");
			await Create("qacoordinator@university.edu", RoleConstants.QACoordinator, "QA Coordinator");
			await Create("staff@university.edu", RoleConstants.Staff, "Staff Member");
		}
	}
}