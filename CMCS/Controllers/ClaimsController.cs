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
            var fullName = HttpContext.Session.GetString("FullName");

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

                await _tableService.SubmitClaimAsync(userId, model.HoursWorked, model.HourlyRate, model.ExtraNotes, string.Join(",", fileData), fullName);
            }
            else
            {
                await _tableService.SubmitClaimAsync(userId, model.HoursWorked, model.HourlyRate, model.ExtraNotes, null, fullName);
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

            // Convert submission time from UTC to South African time for each claim
            foreach (var claim in claims)
            {
                if (claim.ContainsKey("SubmissionTime"))
                {
                    DateTimeOffset submissionTime = DateTimeOffset.Parse(claim["SubmissionTime"].ToString());
                    TimeZoneInfo southAfricaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
                    claim["SubmissionTime"] = TimeZoneInfo.ConvertTime(submissionTime, southAfricaTimeZone).ToString("dd/MM/yyyy HH:mm");
                }
            }

            return View("TrackClaims", claims);
        }

        [HttpGet]
        public async Task<IActionResult> ViewClaims()
        {
            var role = HttpContext.Session.GetString("Role");

            // Only ProgrammeCoordinator and AcademicManager can access this page
            if (role == "ProgrammeCoordinator" || role == "AcademicManager")
            {
                var allClaims = await _tableService.GetAllClaimsAsync();

                // Convert submission time from UTC to South African time for each claim
                TimeZoneInfo southAfricaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");

                foreach (var claim in allClaims)
                {
                    if (claim.ContainsKey("SubmissionTime"))
                    {
                        DateTimeOffset submissionTime = DateTimeOffset.Parse(claim["SubmissionTime"].ToString());
                        claim["SubmissionTime"] = TimeZoneInfo.ConvertTime(submissionTime, southAfricaTimeZone).ToString("dd/MM/yyyy HH:mm");
                    }
                }

                return View("ViewClaims", allClaims);
            }

            // If the user is not an admin, redirect to their own claims page
            return RedirectToAction("TrackClaims");
        }

        [HttpGet]
        public IActionResult ViewDocument(string fileUrl)
        {
            return View((object)fileUrl);
        }
        [HttpPost]
        public async Task<IActionResult> ApproveClaim(string claimId)
        {
            var role = HttpContext.Session.GetString("Role");

            // Ensure only admins can approve/deny claims
            if (role == "ProgrammeCoordinator" || role == "AcademicManager")
            {
                await _tableService.UpdateClaimStatusAsync(claimId, "Approved");
            }

            return RedirectToAction("ViewClaims");
        }

        [HttpPost]
        public async Task<IActionResult> DenyClaim(string claimId)
        {
            var role = HttpContext.Session.GetString("Role");

            // Ensure only admins can approve/deny claims
            if (role == "ProgrammeCoordinator" || role == "AcademicManager")
            {
                await _tableService.UpdateClaimStatusAsync(claimId, "Denied");
            }

            return RedirectToAction("ViewClaims");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateClaimStatus(string claimId, string status)
        {
            var role = HttpContext.Session.GetString("Role");

            // Only ProgrammeCoordinator and AcademicManager can update claim statuses
            if (role == "ProgrammeCoordinator" || role == "AcademicManager")
            {
                // Fetch the claim by ID (RowKey)
                var claim = await _tableService.GetClaimByIdAsync(claimId);

                if (claim != null)
                {
                    // Update the claim status
                    claim["Status"] = status;

                    // Save the updated claim back to the Azure Table
                    await _tableService.UpdateClaimAsync(claim);
                }
            }

            return RedirectToAction("ViewClaims");
        }
    }
}
