
namespace IdeaCollectionIdea.Common.Constants
{
	public static class PolicyConstants
	{
		// Role-based policies
		public const string AdminOnly = "AdminOnly";
		public const string QAManagerOnly = "QAManagerOnly";
		public const string QACoordinatorOnly = "QACoordinatorOnly";
		public const string StaffOnly = "StaffOnly";

		// Combined policies
		public const string QAManagement = "QAManagement";
		public const string AllStaff = "AllStaff";

		// Feature policies
		public const string CanManageCategories = "CanManageCategories";
		public const string CanExportData = "CanExportData";
		public const string CanManageUsers = "CanManageUsers";
		public const string CanSetClosureDates = "CanSetClosureDates";
		public const string MustAcceptTerms = "MustAcceptTerms";
	}
}