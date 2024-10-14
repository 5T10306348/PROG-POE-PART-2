using Microsoft.AspNetCore.Mvc;
using CMCS.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using Azure;
using System.Diagnostics;

namespace CMCS.Controllers
{
    public class AccountController : Controller
    {
        private readonly TableService _tableService;
        private readonly FileService _fileService;

        public AccountController(TableService tableService, FileService fileService)
        {
            _tableService = tableService;
            _fileService = fileService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = HashPassword(model.Password); // Hash the password

                // Create a new user entity, including the full name
                var userEntity = new UserEntity(model.Email, "Lecturer", model.FullName)
                {
                    PasswordHash = hashedPassword,
                };

                try
                {
                    // Attempt to insert the user
                    var isUserInserted = await _tableService.InsertUserAsync(userEntity);
                    if (!isUserInserted)
                    {
                        // Show an error message in the email field
                        ModelState.AddModelError("Email", "This email is already registered.");
                        return View(model);
                    }

                    // Don't log the user in automatically. Redirect to login page instead.
                    return RedirectToAction("Login", "Account");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the user from Azure Table by email (RowKey)
                var userEntity = await _tableService.GetUserByEmailAsync(model.Email);

                if (userEntity != null)
                {
                    // Check if the user is either Programme Coordinator or Academic Manager
                    if (userEntity.PartitionKey == "ProgrammeCoordinator" || userEntity.PartitionKey == "AcademicManager")
                    {
                        // Skip hashing for admins and check if the password is the plain-text password
                        if (model.Password == userEntity.PasswordHash) // Compare directly
                        {
                            // Store the user's full name, email, and role in the session
                            HttpContext.Session.SetString("FullName", userEntity.FullName);
                            HttpContext.Session.SetString("UserId", model.Email);
                            HttpContext.Session.SetString("Role", userEntity.PartitionKey); // Store role in session

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt for admin.");
                        }
                    }
                    else
                    {
                        // For non-admin users, hash the password and verify
                        if (VerifyPassword(model.Password, userEntity.PasswordHash))
                        {
                            // Store the user's full name, email, and role in the session
                            HttpContext.Session.SetString("FullName", userEntity.FullName);
                            HttpContext.Session.SetString("UserId", model.Email);
                            HttpContext.Session.SetString("Role", userEntity.PartitionKey); // Store role in session correctly

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            return View(model);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var enteredPasswordHash = HashPassword(enteredPassword);
            return enteredPasswordHash == storedHash;
        }

        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                // Clear all session variables
                HttpContext.Session.Clear();

                // Redirect to the login page after logout
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                // Log the error if necessary
                // Return an error page or message
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // New Methods for Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _tableService.GetUserByEmailAsync(userId);

            // Create a view model for the profile page
            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.RowKey, // RowKey represents the user's email
                ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl)
                    ? Url.Content(user.ProfilePictureUrl)
                    : Url.Content("~/images/default-profile.jpg")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the user from the table
            var user = await _tableService.GetUserByEmailAsync(userId);

            // Update user's full name and email (RowKey)
            user.FullName = model.FullName;
            user.RowKey = model.Email;

            // Update the user in the table storage
            await _tableService.UpdateUserAsync(user);

            // Update session information
            HttpContext.Session.SetString("FullName", model.FullName);
            HttpContext.Session.SetString("UserId", model.Email);

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if profile picture file is valid
            if (profilePicture != null && profilePicture.Length > 0)
            {
                // Upload the profile picture and get the URL
                var (fileName, fileUrl) = await _fileService.UploadFileAsync(profilePicture);

                // Retrieve the user from the table
                var user = await _tableService.GetUserByEmailAsync(userId);

                // Set the profile picture URL
                user.ProfilePictureUrl = fileUrl; // Use dot notation to assign the URL

                // Update the user in the table storage
                await _tableService.UpdateUserAsync(user);

                // Update the session with the new profile picture URL
                HttpContext.Session.SetString("ProfilePictureUrl", fileUrl);
            }

            return RedirectToAction("Profile");
        }

    }
}

