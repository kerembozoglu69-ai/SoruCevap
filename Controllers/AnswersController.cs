using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoruCevap_forum_.Data;
using SoruCevap_forum_.Models;
using System.Security.Claims;

namespace SoruCevap_forum_.Controllers
{
    [Authorize]
    public class AnswersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnswersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int questionId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var answer = new Answer
            {
                Content = content,
                QuestionId = questionId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.Answers.Add(answer);
            
            // Award points to the answerer
            var user = await _context.Users.FindAsync(userId);
            if (user != null) user.Points += 10;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Questions", new { id = questionId });
        }

        [HttpPost]
        public async Task<IActionResult> Like(int answerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Json(new { success = false, redirect = Url.Action("Login", "Account") });

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.AnswerId == answerId && l.UserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                _context.Likes.Add(new Like { AnswerId = answerId, UserId = userId, CreatedAt = DateTime.Now });
                
                // Award points to the answer's owner
                var answerOwner = await _context.Answers
                    .Where(a => a.Id == answerId)
                    .Select(a => a.User)
                    .FirstOrDefaultAsync();
                
                if (answerOwner != null) answerOwner.Points += 5;
            }

            await _context.SaveChangesAsync();

            var count = await _context.Likes.CountAsync(l => l.AnswerId == answerId);
            return Json(new { success = true, count = count });
        }
    }
}
