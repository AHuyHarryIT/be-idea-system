namespace IdeaCollectionIdea.Common.Constants
{
	public static class RoleConstants
	{
		public const string Administrator = "Administrator";
		public const string QAManager = "QAManager";
		public const string QACoordinator = "QACoordinator";
		public const string Staff = "Staff";

		public static readonly Dictionary<string, string> RoleDescriptions = new()
		{
			{ Administrator, "System administrator with full access" },
			{ QAManager, "Quality Assurance Manager overseeing entire process" },
			{ QACoordinator, "Department QA Coordinator" },
			{ Staff, "Academic and support staff" }
		};

		public static List<string> GetAllRoles() => new()
		{
			Administrator,
			QAManager,
			QACoordinator,
			Staff
		};
	}
}