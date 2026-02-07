// IdeaCollectionIdea.Common/Constants/ReactionConstants.cs
namespace IdeaCollectionIdea.Common.Constants
{
	public static class ReactionConstants
	{
		public const string ThumbsUp = "thumbs_up";
		public const string ThumbsDown = "thumbs_down";

		public static readonly Dictionary<string, int> ReactionScores = new()
		{
			{ ThumbsUp, 1 },
			{ ThumbsDown, -1 }
		};

		public static bool IsValidReaction(string reaction)
		{
			return ReactionScores.ContainsKey(reaction);
		}

		public static int GetScore(string reaction)
		{
			return ReactionScores.TryGetValue(reaction, out var score) ? score : 0;
		}
	}
}