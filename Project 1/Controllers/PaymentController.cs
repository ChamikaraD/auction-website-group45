using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options; // If using IOptions for Stripe keys
using Project_1.Data;
using Project_1.Models;
using Stripe;
using Stripe.Checkout; // Important for SessionService
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project_1.Controllers
{
    [Authorize] // Only logged-in users can access payment actions
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration; // To read Stripe keys

        public PaymentController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            // Configure Stripe API key globally (usually done in Program.cs, but can be set here too if needed)
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        // Action to initiate payment for a won listing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(int listingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var listing = await _context.Listings
                .Include(l => l.Bids.OrderByDescending(b => b.Price)) // Get bids to find winner
                .FirstOrDefaultAsync(l => l.Id == listingId);

            if (listing == null || !listing.IsSold)
            {
                TempData["Error"] = "Listing not found or auction not closed.";
                return RedirectToAction("MyBids", "Listings"); // Or wherever won items are shown
            }

            var winningBid = listing.Bids.FirstOrDefault(); // Highest bid is the first one

            // Check if the current user is the winner
            if (winningBid == null || winningBid.IdentityUserId != userId)
            {
                TempData["Error"] = "You are not the winning bidder for this item.";
                return RedirectToAction("MyBids", "Listings");
            }

            // Check if already paid (Add an IsPaid property to Listing model)
            // if (listing.IsPaid) { ... redirect, already paid ... }

            var domain = $"{Request.Scheme}://{Request.Host}"; // Your website's URL

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(winningBid.Price * 100), // Amount in cents
                            Currency = "lkr", // Sri Lankan Rupee
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = listing.Title,
                                Description = listing.Description,
                                // Images = new List<string> { $"{domain}/Images/{listing.ImagePath}" } // Optional image
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = domain + $"/Payment/PaymentSuccess?sessionId={{CHECKOUT_SESSION_ID}}&listingId={listing.Id}", // Redirect URL on success
                CancelUrl = domain + $"/Payment/PaymentCancel?listingId={listing.Id}",       // Redirect URL on cancel
                Metadata = new Dictionary<string, string> // Store useful info
                {
                    { "listing_id", listing.Id.ToString() },
                    { "user_id", userId }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            // Redirect user to Stripe Checkout page
            Response.Headers.Append("Location", session.Url);
            return new StatusCodeResult(303); // 303 See Other redirect
        }

        // Action called when payment is successful
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(string sessionId, int listingId)
        {
            var sessionService = new SessionService();
            Session session = await sessionService.GetAsync(sessionId);

            // Basic validation - check if payment was actually successful
            if (session.PaymentStatus == "paid")
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Or get from session metadata

                // Check if payment record already exists for this session to prevent duplicates
                bool paymentExists = await _context.Payments.AnyAsync(p => p.StripePaymentIntentId == session.PaymentIntentId);

                if (!paymentExists)
                {
                    // Record the payment in your database
                    var payment = new Payment
                    {
                        ListingId = listingId,
                        Amount = (decimal)session.AmountTotal / 100, // Convert from cents
                        PaymentDate = DateTime.UtcNow,
                        StripePaymentIntentId = session.PaymentIntentId, // Store Stripe ID
                        UserId = userId
                    };
                    _context.Payments.Add(payment);

                    // Optionally mark the Listing as Paid (add an IsPaid bool property to Listing.cs)
                    var listing = await _context.Listings.FindAsync(listingId);
                    if (listing != null)
                    {
                        // listing.IsPaid = true; // Add this property if needed
                        _context.Update(listing);
                    }

                    await _context.SaveChangesAsync();

                    // Optionally notify the seller
                    // var seller = await _userManager.FindByIdAsync(listing.IdentityUserId);
                    // await _emailSender.SendEmailAsync(seller.Email, "Payment Received", $"Payment of {payment.Amount} LKR received for {listing.Title}.");

                    ViewBag.ListingId = listingId;
                    ViewBag.Amount = payment.Amount;
                    return View("Confirm"); // Show your existing confirmation page
                }
                ViewBag.Message = "Payment already recorded.";
                return View("Confirm"); // Or redirect somewhere else
            }

            TempData["Error"] = "Payment verification failed.";
            return RedirectToAction("PaymentCancel", new { listingId });
        }

        // Action called when payment is canceled
        [HttpGet]
        public IActionResult PaymentCancel(int listingId)
        {
            TempData["Error"] = "Payment was canceled.";
            // Redirect back to the listing details or user's won items page
            return RedirectToAction("Details", "Listings", new { id = listingId });
        }
    }
}