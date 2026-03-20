using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using IdeaCollectionSystem.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeaCollectionSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IdeaController : ControllerBase
    {
        private readonly IIdeaService _ideaService;
        private readonly IEmailQueue _emailQueue;
        private readonly UserManager<IdeaUser> _userManager;

        // Inject IEmailQueue and UserManager
        public IdeaController(
            IIdeaService ideaService,
            IEmailQueue emailQueue,
            UserManager<IdeaUser> userManager)
        {
            _ideaService = ideaService;
            _emailQueue = emailQueue;
            _userManager = userManager;
        }

        // 1. Create Idea
        [HttpPost]
        public async Task<IActionResult> CreateIdea([FromBody] IdeaCreateDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _ideaService.CreateIdeaAsync(request, userId);

            if (success)
            {
                // --- FLOW 1: SEND EMAIL TO QA COORDINATOR WHEN A NEW IDEA IS CREATED ---
                try
                {
                    var creator = await _userManager.FindByIdAsync(userId);
                    if (creator != null && creator.DepartmentId != null)
                    {
                        var qaCoordinators = await _userManager.GetUsersInRoleAsync(RoleConstants.QACoordinator);
                        var targetQA = qaCoordinators.FirstOrDefault(qa => qa.DepartmentId == creator.DepartmentId);

                        if (targetQA != null && !string.IsNullOrEmpty(targetQA.Email))
                        {
                            await _emailQueue.QueueEmailAsync(new EmailMessage
                            {
                                ToEmail = targetQA.Email,
                                Subject = "[IdeaCollectionSystem] 💡 A new idea from your department",
                                Message = $@"
                                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                                    <h2>Hello {targetQA.Name ?? "QA Coordinator"},</h2>
                                    <p>Staff member <b>{creator.Name ?? creator.UserName}</b> has just submitted a new idea to the system.</p>
                                    <p>Please log in to the system to review, evaluate, and categorize it.</p>
                                    <br/>
                                    <a href='http://localhost:3000/ideas' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                        View Idea
                                    </a>
                                </div>"
                            });
                        }
                    }
                }
                catch { /* Ignore email sending errors to avoid affecting the API response */ }
                // ---------------------------------------------------------------

                return Ok(new { message = "The idea has been submitted successfully." });
            }

            return BadRequest(new { message = "The submitted idea failed. Please double-check the closure date." });
        }

        // 2. Get Ideas (All or by Department for QA Coordinator)
        [HttpGet]
        public async Task<IActionResult> GetIdeas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (User.IsInRole(RoleConstants.QACoordinator))
            {
                var deptIdeas = await _ideaService.GetIdeasByDepartmentAsync(userId);
                return Ok(deptIdeas);
            }

            var allIdeas = await _ideaService.GetAllIdeasAsync();
            return Ok(allIdeas);
        }

        // 3. Get My Ideas
        [HttpGet("my-ideas")]
        public async Task<IActionResult> GetMyIdeas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myIdeas = await _ideaService.GetIdeasByStaffAsync(userId!);
            return Ok(myIdeas);
        }

        // 4. Get Idea Details
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIdeaDetails([FromRoute] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ideaDetail = await _ideaService.GetIdeaDetailAsync(id, userId!);

            if (ideaDetail == null) return NotFound(new { message = "No ideas found." });

            return Ok(ideaDetail);
        }

        // 5. Add Comment
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment([FromRoute] Guid id, [FromBody] CommentDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { message = "The comment section cannot be left blank." });

            var success = await _ideaService.AddCommentAsync(id, userId!, request.Text, request.IsAnonymous);

            if (success)
            {
                // --- FLOW 2: SEND EMAIL TO AUTHOR WHEN A COMMENT IS ADDED ---
                try
                {
                    // Get idea details to find the author
                    var ideaDetail = await _ideaService.GetIdeaDetailAsync(id, userId!);
                    var commenter = await _userManager.FindByIdAsync(userId!);

                    // NOTE: Ensure the commenter is NOT the author before sending the email (prevent self-spam).
                    if (ideaDetail != null && ideaDetail.UserId != userId)
                    {
                        var author = await _userManager.FindByIdAsync(ideaDetail.UserId);
                        if (author != null && !string.IsNullOrEmpty(author.Email))
                        {
                            string commenterName = request.IsAnonymous ? "An anonymous user" : (commenter?.Name ?? commenter?.UserName);

                            await _emailQueue.QueueEmailAsync(new EmailMessage
                            {
                                ToEmail = author.Email,
                                Subject = "[IdeaCollectionSystem] 💬 Your idea has a new comment",
                                Message = $@"
                                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                                        <h2>Hello {author.Name ?? "Author"},</h2>
                                        <p><b>{commenterName}</b> just left a comment on your idea.</p>
                                        <p><i>'{request.Text}'</i></p>
                                        <br/>
                                        <a href='http://localhost:3000/ideas/{id}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                            View and Reply
                                        </a>
                                    </div>"
                            });
                        }
                    }
                }
                catch { /* Ignore email sending errors */ }
                // -----------------------------------------------------

                return Ok(new { message = "Comment added successfully." });
            }

            return BadRequest(new { message = "Unable to comment (The idea does not exist or is outdated)." });
        }

        // 6. Vote (Thumbs Up / Down)
        [HttpPost("{id}/vote")]
        public async Task<IActionResult> Vote([FromRoute] Guid id, [FromBody] VoteRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _ideaService.VoteIdeaAsync(id, userId!, request.IsThumbsUp);

            if (result) return Ok(new { success = true, message = "The votes have been recorded." });

            return BadRequest(new { success = false, message = "The vote was a failure." });
        }

        // GET: api/idea/paged?pageNumber=1&sortBy=popular
        [HttpGet("paged")]
        public async Task<IActionResult> GetIdeasPaged([FromQuery] IdeaQueryParameters parameters)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var pagedResult = await _ideaService.GetIdeasPagedAsync(parameters, userId);
            return Ok(pagedResult);
        }
    }
}