using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
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

        public IdeaController(IIdeaService ideaService)
        {
            _ideaService = ideaService;
        }

		//  1. Ceate Idea
		[HttpPost]
        public async Task<IActionResult> CreateIdea([FromBody] IdeaCreateDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _ideaService.CreateIdeaAsync(request, userId);
            if (success) return Ok(new { message = "The idea has been submitted successfully." });

            return BadRequest(new { message = "The submitted idea failed. Please double-check the closure date." });
        }

        //  Get IDea
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

		//  GET: api/idea/my-ideas
		[HttpGet("my-ideas")]
        public async Task<IActionResult> GetMyIdeas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myIdeas = await _ideaService.GetIdeasByStaffAsync(userId!);
            return Ok(myIdeas);
        }

		//   GetIdeaDetails
		[HttpGet("{id}")]
        public async Task<IActionResult> GetIdeaDetails([FromRoute] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ideaDetail = await _ideaService.GetIdeaDetailAsync(id, userId!);
            if (ideaDetail == null) return NotFound(new { message = "No ideas found." });
            return Ok(ideaDetail);
        }

		//   comments 
		[HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment([FromRoute] Guid id, [FromBody] CommentCreateRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(request.Text)) return BadRequest(new { message = "The comment section cannot be left blank." });

            var success = await _ideaService.AddCommentAsync(id, userId!, request.Text, request.IsAnonymous);
            if (success) return Ok(new { message = "Comment added successfully." });
            return BadRequest(new { message = "KUnable to comment (The idea does not exist or is outdated)." });
        }

        //  VOTE (THUMBS UP / DOWN) 
        [HttpPost("{id}/vote")]
        public async Task<IActionResult> Vote([FromRoute] Guid id, [FromBody] VoteRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _ideaService.VoteIdeaAsync(id, userId!, request.IsThumbsUp);
            if (result) return Ok(new { success = true, message = "The votes have been recorded." });
            return BadRequest(new { success = false, message = "The vote was a failure." });
        }
    }
}