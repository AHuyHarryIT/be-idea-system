
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;  
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdeaCollectionSystem.MVC.Areas.Identity.Pages.Account
{
	[AllowAnonymous]
	public class LogoutModel : PageModel
	{
		private readonly SignInManager<IdeaUser> _signInManager;  
		private readonly ILogger<LogoutModel> _logger;

		public LogoutModel(SignInManager<IdeaUser> signInManager, ILogger<LogoutModel> logger)  
		{
			_signInManager = signInManager;
			_logger = logger;
		}

		public async Task<IActionResult> OnPost(string returnUrl = null)
		{
			await _signInManager.SignOutAsync();
			_logger.LogInformation("User logged out.");

			if (returnUrl != null)
			{
				return LocalRedirect(returnUrl);
			}
			else
			{
				return RedirectToPage();
			}
		}
	}
}