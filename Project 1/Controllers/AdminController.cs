using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_1.Data;
using Project_1.Models; // Assuming your models are here
using System.Linq;
using System.Threading.Tasks;

namespace Project_1.Controllers
{
    // Restrict access to only users in the "Admin" role
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AdminController> _logger; // Optional: for logging errors

        // Constructor to inject services
        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // Action for the main Admin Dashboard
        public IActionResult Dashboard()
        {
            // You could pass summary data here if needed
            return View(); // Assumes view is in Views/Admin/Dashboard.cshtml
        }

        // Action to display Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users); // Assumes view is in Views/Admin/Users.cshtml
        }

        // --- User Management Actions ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Set LockoutEnd to a future date to deactivate
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                _logger.LogInformation($"User {user.UserName} deactivated.");
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Set LockoutEnd to null or a past date to activate
                await _userManager.SetLockoutEndDateAsync(user, null);
                _logger.LogInformation($"User {user.UserName} activated.");
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Prevent deleting the currently logged-in admin (optional safety check)
                if (user.UserName == User.Identity.Name)
                {
                    TempData["Error"] = "Cannot delete the currently logged-in administrator.";
                    return RedirectToAction(nameof(Users));
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.UserName} deleted.");
                }
                else
                {
                    TempData["Error"] = $"Error deleting user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }
            return RedirectToAction(nameof(Users));
        }

        // --- Listing Management Action ---
        public async Task<IActionResult> Listings()
        {
            // Include user info if needed, show ALL listings (active and sold)
            var listings = await _context.Listings
                                    .Include(l => l.User) // Load user data
                                    .OrderByDescending(l => l.Id)
                                    .ToListAsync();
            return View(listings); // Assumes view is in Views/Admin/Listings.cshtml
        }

        // Note: The DeleteListing action is likely already in your ListingsController.
        // You might want to move it here or ensure it has [Authorize(Roles = "Admin")]

        // --- Bid Management Action ---
        public async Task<IActionResult> Bids()
        {
            var bids = await _context.Bids
                                   .Include(b => b.Listing) // Load listing data
                                   .Include(b => b.User)    // Load user data
                                   .OrderByDescending(b => b.Id)
                                   .ToListAsync();
            return View(bids); // Assumes view is in Views/Admin/Bids.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBid(int bidId)
        {
            var bid = await _context.Bids.FindAsync(bidId);
            if (bid != null)
            {
                bid.IsApproved = true; // Assuming Bid model has IsApproved property
                _context.Update(bid);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Bid {bidId} approved.");
            }
            return RedirectToAction(nameof(Bids));
        }


        // --- Payment Management Action ---
        public async Task<IActionResult> Payments()
        {
            var payments = await _context.Payments
                                    .Include(p => p.Listing) // Load listing data if needed
                                    .OrderByDescending(p => p.PaymentDate)
                                    .ToListAsync();
            return View(payments); // Assumes view is in Views/Admin/Payments.cshtml
        }
    }
}