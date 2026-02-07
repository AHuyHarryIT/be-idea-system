
namespace IdeaCollectionIdea.Common.Constants
{
	public static class RoleConstants
	{
		public const string Administrator = "Administrator";
		public const string QAManager = "QA Manager";
		public const string QACoordinator = "QA Coordinator";
		public const string Staff = "Staff";

		public static readonly Dictionary<string, string> RoleDescriptions = new()
		{
			{ Administrator, "System administrator with full access" },
			{ QAManager, "Quality Assurance Manager overseeing entire process" },
			{ QACoordinator, "Department QA Coordinator" },
			{ Staff, "Academic and support staff" }
		};

		// Dashboard routes for each role
		public static readonly Dictionary<string, string> RoleDashboards = new()
		{
			{ Administrator, "/Admin/Dashboard" },
			{ QAManager, "/QAManager/Dashboard" },
			{ QACoordinator, "/QACoordinator/Dashboard" },
			{ Staff, "/Home/Index" }
		};

		public static List<string> GetAllRoles() => new()
		{
			Administrator,
			QAManager,
			QACoordinator,
			Staff
		};

		public static string GetDashboardUrl(string role)
		{
			return RoleDashboards.TryGetValue(role, out var url) ? url : "/Home/Index";
		}
	}
}