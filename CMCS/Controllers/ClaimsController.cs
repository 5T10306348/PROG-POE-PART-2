using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CMCS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMCS.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly TableService _tableService;
        private readonly FileService _fileService;

        public ClaimsController(TableService tableService, FileService fileService)
        {
            _tableService = tableService;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> SubmitClaim()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(ClaimModel model, List<IFormFile> UploadedFiles)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<string> fileData = new List<string>();

            if (UploadedFiles != null && UploadedFiles.Count > 0)
            {
                foreach (var file in UploadedFiles)
                {
                    // Upload the file and get both the file name and URL
                    var (fileName, fileUrl) = await _fileService.UploadFileAsync(file);
                    fileData.Add($"{fileName}|{fileUrl}"); // Store fileName|fileUrl format
                }

                // Store the claim details, including the file names and URLs
                await _tableService.SubmitClaimAsync(userId, model.HoursWorked, model.HourlyRate, model.ExtraNotes, string.Join(",", fileData));
            }
            else
            {
                await _tableService.SubmitClaimAsync(userId, model.HoursWorked, model.HourlyRate, model.ExtraNotes, null);
            }

            return RedirectToAction("ViewClaims");
        }

        [HttpGet]
        public async Task<IActionResult> ViewClaims()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var claims = await _tableService.GetClaimsByUserAsync(userId);
            return View(claims);
        }

        // New action to view a single document
        [HttpGet]
        public IActionResult ViewDocument(string fileUrl)
        {
            return View((object)fileUrl);
        }
    }
}
