using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoruCevap_forum_.Data;
using SoruCevap_forum_.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SoruCevap_forum_.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalQuestions = await _context.Questions.CountAsync();
            ViewBag.TotalAnswers = await _context.Answers.CountAsync();

            // Chart Data: Questions per Category
            ViewBag.CategoryStats = await _context.Categories
                .Select(c => new { Name = c.Name, Count = c.Questions.Count })
                .ToListAsync();

            // All Users for Management
            ViewBag.AllUsers = await _context.Users.ToListAsync();

            // All Questions for Management
            var questions = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Category)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(questions);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserSuspension(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsSuspended = !user.IsSuspended;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportExcel()
        {
            var questions = await _context.Questions.Include(q => q.User).Include(q => q.Category).ToListAsync();
            
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sorular");
            
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Başlık";
            worksheet.Cell(1, 3).Value = "Kategori";
            worksheet.Cell(1, 4).Value = "Soran";
            worksheet.Cell(1, 5).Value = "Puan";
            worksheet.Cell(1, 6).Value = "Tarih";

            for (int i = 0; i < questions.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = questions[i].Id;
                worksheet.Cell(i + 2, 2).Value = questions[i].Title;
                worksheet.Cell(i + 2, 3).Value = questions[i].Category?.Name;
                worksheet.Cell(i + 2, 4).Value = questions[i].User?.FullName;
                worksheet.Cell(i + 2, 5).Value = questions[i].User?.Points;
                worksheet.Cell(i + 2, 6).Value = questions[i].CreatedAt.ToString("g");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Sorular.xlsx");
        }

        public async Task<IActionResult> ExportPdf()
        {
            var questions = await _context.Questions.Include(q => q.User).Include(q => q.Category).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Soru Raporu").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                    
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("ID");
                            header.Cell().Text("Başlık");
                            header.Cell().Text("Kategori");
                            header.Cell().Text("Tarih");
                        });

                        foreach (var q in questions)
                        {
                            table.Cell().Text(q.Id.ToString());
                            table.Cell().Text(q.Title);
                            table.Cell().Text(q.Category?.Name);
                            table.Cell().Text(q.CreatedAt.ToString("g"));
                        }
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "Sorular.pdf");
        }
    }
}
