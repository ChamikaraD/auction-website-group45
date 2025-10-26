using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_1.Data;
using Project_1.Models;
using Microsoft.AspNetCore.Identity.UI.Services; // Needed for IEmailSender
using System.Threading.Tasks; // Needed for Task

namespace Project_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender; // Added for Contact form
        private readonly ILogger<HomeController> _logger; // Added for logging

        // Updated Constructor
        public HomeController(ApplicationDbContext context, IEmailSender emailSender, ILogger<HomeController> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Get top 3 featured listings (only active ones)
            var featuredListings = await _context.Listings
                .Include(l => l.Bids)
                .Where(l => !l.IsSold && l.ClosingTime > DateTime.Now) // Ensure they haven't expired
                .OrderByDescending(l => l.Id) // Or order by something else? e.g., ClosingTime
                .Take(3)
                .ToListAsync();

            return View(featuredListings);
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        // --- NEW: Contact Us (GET) ---
        public IActionResult ContactUs()
        {
            return View(); // Returns Views/Home/ContactUs.cshtml
        }

        // --- NEW: Contact Us (POST - Handles Form Submission) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Define parameters matching the 'name' attributes in your form
        public async Task<IActionResult> ContactUs(string Name, string Email, string Subject, string Message)
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Message))
            {
                TempData["Error"] = "Please fill out all fields in the contact form.";
                return View(); // Return the view with an error
            }

            // --- Option 1: Log the message (Simple) ---
            _logger.LogInformation("Contact Form Submitted: Name: {Name}, Email: {Email}, Subject: {Subject}, Message: {Message}", Name, Email, Subject, Message);

            // --- Option 2: Send the message via Email (More Advanced) ---
            try
            {
                string emailSubject = $"Contact Form: {Subject}";
                string emailBody = $"<p>You received a new message from the contact form:</p>" +
                                   $"<p><strong>Name:</strong> {Name}<br>" +
                                   $"<strong>Email:</strong> {Email}</p>" +
                                   $"<p><strong>Message:</strong><br>{Message.Replace("\n", "<br>")}</p>"; // Replace newlines with <br> for HTML email

                // Send email to your admin/support address
                await _emailSender.SendEmailAsync("your_admin_email@example.com", emailSubject, emailBody); // <<< REPLACE with your actual admin email

                TempData["Success"] = "Your message has been sent successfully! We will get back to you soon.";
                return RedirectToAction("Index"); // Redirect to home page after success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contact form email.");
                TempData["Error"] = "There was an error sending your message. Please try again later.";
                return View(); // Return the view with an error
            }

            // --- End Option 2 ---

            // If only logging (Option 1), you can just redirect after logging:
            // TempData["Success"] = "Your message has been received.";
            // return RedirectToAction("Index");
        }

    }
}