using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_1.Models;
using Project_1.Data.Services; // Assuming this is where your services are
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Project_1.Data;
using Microsoft.AspNetCore.Identity.UI.Services; // Needed for IEmailSender
using Microsoft.AspNetCore.SignalR;
using Project_1.Hubs;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace Project_1.Controllers
{
    public class ListingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ListingsController> _logger;
        private readonly IHubContext<BidHub> _hubContext;
        private readonly IListingsService _listingsService;
        private readonly IBidsService _bidsService;
        private readonly ICommentsService _commentsService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender; // Added field

        // Constructor updated to include IEmailSender
        public ListingsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender, // Added parameter
            ILogger<ListingsController> logger,
            IListingsService listingsService,
            IWebHostEnvironment webHostEnvironment,
            IBidsService bidsService,
            ICommentsService commentsService,
            IHubContext<BidHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender; // Assigned field
            _logger = logger;
            _listingsService = listingsService;
            _webHostEnvironment = webHostEnvironment;
            _bidsService = bidsService;
            _commentsService = commentsService;
            _hubContext = hubContext;
        }

        // ✅ Automatically close expired listings
        private async Task CheckAndCloseExpiredListings()
        {
            var expiredListings = await _context.Listings
                .Where(l => !l.IsSold && l.ClosingTime <= DateTime.Now)
                .Include(l => l.Bids)
                    .ThenInclude(b => b.User)
                .ToListAsync();

            if (expiredListings.Any())
            {
                foreach (var listing in expiredListings)
                {
                    listing.IsSold = true;

                    var highestBid = listing.Bids?.OrderByDescending(b => b.Price).FirstOrDefault();
                    if (highestBid != null && highestBid.User != null)
                    {
                        // Use the NotifyWinner helper method (defined below)
                        await NotifyWinner(listing, highestBid);
                    }

                    await _hubContext.Clients.All.SendAsync("BidClosed", listing.Id);
                }

                await _context.SaveChangesAsync();
            }
        }

        // Helper method to send winner notification
        private async Task NotifyWinner(Listing listing, Bid highestBid)
        {
            if (highestBid != null && highestBid.User != null)
            {
                string subject = "Congratulations! You won the auction!";
                // Using HTML for better formatting
                string message = $"<p>Dear {highestBid.User.UserName},</p>" +
                                 $"<p>Congratulations! You have won the bid for '{listing.Title}' with Rs {highestBid.Price:N2}.</p>" +
                                 "<p>Please proceed to payment.</p>" + // Consider adding a direct payment link here later
                                 "<p>Best regards,<br>NumisLive Team</p>";

                try
                {
                    await _emailSender.SendEmailAsync(highestBid.User.Email, subject, message);
                    _logger.LogInformation($"Winner notification email sent to {highestBid.User.Email} for listing {listing.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send winner notification email to {highestBid.User.Email} for listing {listing.Id}");
                }
            }
        }


        // GET: Listings
        public async Task<IActionResult> Index(int? pageNumber, string searchString)
        {
            await CheckAndCloseExpiredListings();
            var listings = _listingsService.GetAll();
            int pageSize = 3;

            if (!string.IsNullOrEmpty(searchString))
            {
                listings = listings.Where(a => a.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase)); // Case-insensitive search
            }

            return View(await PaginatedList<Listing>.CreateAsync(
                listings.Where(l => !l.IsSold).AsNoTracking(),
                pageNumber ?? 1, pageSize));
        }

        // GET: Listings/Browse
        public async Task<IActionResult> Browse(int? pageNumber, string searchString)
        {
            await CheckAndCloseExpiredListings();
            var listings = _listingsService.GetAll();
            int pageSize = 3;

            if (!string.IsNullOrEmpty(searchString))
            {
                listings = listings.Where(a => a.Title.ToLower().Contains(searchString.ToLower()));
            }

            return View("Browse", await PaginatedList<Listing>.CreateAsync(
                listings.AsNoTracking(),
                pageNumber ?? 1, pageSize));
        }


        public async Task<IActionResult> MyListings(int? pageNumber)
        {
            await CheckAndCloseExpiredListings();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var listings = _listingsService.GetAll().Where(l => l.IdentityUserId == userId);
            int pageSize = 3;

            return View("Index", await PaginatedList<Listing>.CreateAsync(
                listings.AsNoTracking(),
                pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> MyBids(int? pageNumber)
        {
            await CheckAndCloseExpiredListings();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bids = _bidsService.GetAll().Where(b => b.IdentityUserId == userId)
                                     .Include(b => b.Listing) // Ensure Listing is loaded
                                     .ThenInclude(l => l.User); // Ensure Listing's User is loaded

            int pageSize = 3;

            return View(await PaginatedList<Bid>.CreateAsync(
                bids.AsNoTracking(),
                pageNumber ?? 1, pageSize));
        }

        // GET: Listings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Check/close expired listings first
            await CheckAndCloseExpiredListings();

            if (id == null)
            {
                return NotFound();
            }

            // Explicitly include all related data needed by the view
            var listing = await _context.Listings
                .Include(l => l.User) // Include the seller
                .Include(l => l.Bids) // Include all bids
                    .ThenInclude(b => b.User) // Include the user for each bid
                .Include(l => l.Comments) // Include all comments
                    .ThenInclude(c => c.User) // Include the user for each comment
                .FirstOrDefaultAsync(l => l.Id == id); // Find the specific listing

            if (listing == null)
            {
                return NotFound();
            }

            // --- Sort the Bids HERE before sending to the view ---
            // Order for Bid History display (Oldest first as per your view logic)
            listing.Bids = listing.Bids?.OrderBy(b => b.Price).ToList();
            // Order comments if needed
            listing.Comments = listing.Comments?.OrderByDescending(c => c.CreatedAt).ToList();
            // -----------------------------------------------------

            // Pass the fully loaded and sorted listing object to the view
            return View(listing);
        }

        // GET: Listings/Create
        [Authorize]
        public IActionResult Create()
        {
            var model = new ListingVM();
            return View(model);
        }

        // POST: Listings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ListingVM model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Please upload an image for the coin.");
                    return View(model);
                }

                DateTime closingTime = DateTime.Now
                    .AddDays(model.DurationDays)
                    .AddHours(model.DurationHours);

                var listing = new Listing
                {
                    Title = model.CoinName,
                    Description = model.Description,
                    Price = model.StartingPrice,
                    ImagePath = uniqueFileName,
                    ClosingTime = closingTime,
                    IsSold = false,
                    IdentityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Year = model.Year,
                    Country = model.Country,
                    Denomination = model.Denomination,
                    Grade = model.Grade,
                    MintMark = model.MintMark,
                    Composition = model.Composition
                };

                await _listingsService.Add(listing);

                return RedirectToAction("Details", new { id = listing.Id });
            }
            return View(model);
        }



        [HttpPost]
        public async Task<ActionResult> AddBid([Bind("Id, Price, ListingId, IdentityUserId")] Bid bid)
        {
            if (ModelState.IsValid)
            {
                // Save the bid using the service
                await _bidsService.Add(bid);

                // Get the listing associated with the bid
                var listing = await _listingsService.GetById(bid.ListingId);

                // Check if listing exists and is not sold
                if (listing != null && !listing.IsSold)
                {
                    // Update the listing's price (consider if this should be CurrentBid instead)
                    listing.Price = bid.Price;
                    await _listingsService.SaveChanges();

                    // Notify via SignalR (only sends formatted price)
                    string formattedBid = bid.Price.ToString("N2");
                    await _hubContext.Clients.Group($"listing-{bid.ListingId}")
                        .SendAsync("ReceiveNewHighestBid", formattedBid);
                }
            }
            // Always redirect back to the details page, regardless of validation success
            return RedirectToAction("Details", new { id = bid.ListingId });
        }

        // POST: Close Bidding
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Ensure only authorized user (seller or admin) can close
        public async Task<IActionResult> CloseBidding(int id)
        {
            var listing = await _context.Listings.Include(l => l.Bids)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null) return NotFound();

            // Security Check: Ensure only the listing owner or an admin can close
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (listing.IdentityUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid(); // Or RedirectToAction("AccessDenied", "Account");
            }

            if (!listing.IsSold) // Only proceed if not already marked as sold
            {
                listing.IsSold = true;
                listing.ClosingTime = DateTime.Now; // Set closing time to now
                _context.Update(listing);
                await _context.SaveChangesAsync();

                var highestBid = listing.Bids?.OrderByDescending(b => b.Price).FirstOrDefault();
                await NotifyWinner(listing, highestBid); // Use helper to send email

                // Notify via SignalR that auction ended
                await _hubContext.Clients.Group($"listing-{id}")
                    .SendAsync("AuctionEnded", id, highestBid?.User?.UserName ?? "No Winner", highestBid?.Price ?? listing.Price);
            }

            return RedirectToAction("Details", new { id = listing.Id });
        }

        // POST: Delete Listing (Cancel without selling)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Ensure only owner or admin can delete
        public async Task<IActionResult> DeleteListing(int id)
        {
            var listing = await _context.Listings
                .Include(l => l.Bids) // Include bids to delete them
                .Include(l => l.Comments) // Include comments to delete them
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null) return NotFound();

            // Security Check: Ensure only the listing owner or an admin can delete
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (listing.IdentityUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (listing.IsSold)
            {
                TempData["Error"] = "Cannot delete a listing that has already been sold.";
                return RedirectToAction("Details", new { id = listing.Id });
            }

            // Manually delete related bids and comments because we set NoAction on delete
            if (listing.Bids != null && listing.Bids.Any())
                _context.Bids.RemoveRange(listing.Bids);
            if (listing.Comments != null && listing.Comments.Any())
                _context.Comments.RemoveRange(listing.Comments);

            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();

            // Notify via SignalR that listing was deleted (optional)
            await _hubContext.Clients.All.SendAsync("ListingDeleted", id);

            TempData["Success"] = "Listing deleted successfully.";
            // Redirect to appropriate page (MyListings for owner, Admin Listings for admin)
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Listings", "Admin");
            }
            return RedirectToAction("MyListings");
        }

        // POST: Delete an individual bid
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Ensure only bidder or admin can delete
        public async Task<IActionResult> DeleteBid(int id) // 'id' here is the Bid ID
        {
            var bid = await _context.Bids
                .Include(b => b.Listing) // Include listing to check status and owner
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bid == null) return NotFound();

            // Security Check: Allow only the bidder or an admin to delete
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (bid.IdentityUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (bid.Listing.IsSold || DateTime.Now >= bid.Listing.ClosingTime)
            {
                TempData["Error"] = "Cannot delete a bid for a closed auction.";
                return RedirectToAction("Details", "Listings", new { id = bid.ListingId });
            }


            _context.Bids.Remove(bid);
            await _context.SaveChangesAsync();

            // Notify via SignalR (optional)
            await _hubContext.Clients.Group($"listing-{bid.ListingId}")
                .SendAsync("BidDeleted", bid.Id);

            TempData["Success"] = "Bid deleted successfully.";
            // Redirect back to where the user initiated delete (MyBids or Admin Bids)
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Bids", "Admin");
            }
            return RedirectToAction("MyBids");
        }

        // POST: Add Comment
        [HttpPost]
        [Authorize] // Ensure user is logged in
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddComment([Bind("Content, ListingId")] Comment comment) // Only bind needed
        {
            comment.IdentityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Set user ID
            comment.CreatedAt = DateTime.UtcNow; // Set timestamp using UTC

            if (ModelState.IsValid)
            {
                await _commentsService.Add(comment);
                TempData["Success"] = "Comment added.";
            }
            else
            {
                TempData["Error"] = "Failed to add comment.";
                // Optionally pass ModelState errors back via TempData if needed
            }

            // Redirect back to details page
            return RedirectToAction("Details", new { id = comment.ListingId });
        }

        // GET: Won Bids (Should maybe be in a User Dashboard controller?)
        [Authorize]
        public async Task<IActionResult> WonBids()
        {
            await CheckAndCloseExpiredListings(); // Ensure auctions are closed first
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge(); // Should not happen if [Authorize] is working

            // Find listings where IsSold=true and the highest bid belongs to the current user
            var wonListings = await _context.Listings
                .Where(l => l.IsSold && l.Bids.Any() && l.Bids.OrderByDescending(b => b.Price).First().IdentityUserId == userId)
                .Include(l => l.Bids.OrderByDescending(b => b.Price)) // Include bids to get winning price
                                                                      // Include other details as needed for the view
                .ToListAsync();

            // You might want a specific ViewModel for this page
            return View(wonListings); // Assumes you have a Views/Listings/WonBids.cshtml
        }
    }
}