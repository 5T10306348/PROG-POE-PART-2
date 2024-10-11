using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CMCS.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

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
                    var (fileName, fileUrl) = await _fileService.UploadFileAsync(file);
                    fileData.Add($"{fileName}|{fileUrl}"); // Store fileName|fileUrl format
                }

                await _tableService.SubmitClaimAsync(userId, model.HoursWorked, model.HourlyRate, model.ExtraNotes, string.Join(",", fileData));
            }
            else
            {
                await _tableService.SubmitClaimAsync(userId, model.HoursWorked, model.HourlyRate, model.ExtraNotes, null);
            }

            return RedirectToAction("ViewClaims");
        }

        [HttpGet]
        public async Task<IActionResult> TrackClaims()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch claims for the logged-in user
            var claims = await _tableService.GetClaimsByUserAsync(userId);

            // Return the "TrackClaims" view
            return View("TrackClaims", claims);
        }

        [HttpGet]
        public async Task<IActionResult> ViewClaims()
        {
            var role = HttpContext.Session.GetString("Role");

            // Check if the user is a lecturer
            if (role == null || role != "Lecturer") // Adjust based on how you're storing roles (e.g., exact case of "Lecturer")
            {
                return RedirectToAction("TrackClaims"); // Redirect to user's own claims if they are not a lecturer
            }

            // Get all claims since the user is a lecturer
            var allClaims = await _tableService.GetAllClaimsAsync();
            return View("ViewClaims", allClaims);
        }

        [HttpGet]
        public IActionResult ViewDocument(string fileUrl)
        {
            return View((object)fileUrl);
        }
    }
}
