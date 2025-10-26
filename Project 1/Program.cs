using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Project_1.Data;
using Project_1.Data.Services;
using Project_1.Hubs;
using Project_1.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// ----------------- Database -----------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ----------------- Identity -----------------
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ----------------- SignalR -----------------
builder.Services.AddSignalR();

// ----------------- Application Services -----------------
builder.Services.AddScoped<IListingsService, ListingsService>();
builder.Services.AddScoped<IBidsService, BidsService>();
builder.Services.AddScoped<ICommentsService, CommentsService>();

builder.Services.AddHostedService<BidExpirationService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddControllersWithViews();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ----------------- Authorization -----------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        // Read the ClientID and ClientSecret from the secrets.json file
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

// ----------------- Stripe -----------------
var stripeSettings = builder.Configuration.GetSection("Stripe");
StripeConfiguration.ApiKey = stripeSettings["SecretKey"];



// ----------------- Build App -----------------
var app = builder.Build();

// ----------------- Seed Admin -----------------
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// ----------------- Middleware -----------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ----------------- SignalR Hub -----------------
app.MapHub<BidHub>("/bidHub");

// ----------------- Routes -----------------
// ----------------- Routes -----------------
app.MapControllerRoute(
    name: "admin_route", // Give a specific name for the admin route
    pattern: "Admin/{action=Dashboard}/{id?}", // Match URLs starting with /Admin/ followed by an action (defaulting to Dashboard)
    defaults: new { controller = "Admin" }); // Specify that this route uses the AdminController

app.MapControllerRoute(
    name: "default", // Keep the default route for everything else
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Keep this for Identity pages

app.Run();

// ----------------- Fake Email Sender -----------------
public class NoEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }
}

// ----------------- SignalR Hub -----------------
namespace Project_1.Hubs
{
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    public class BidHub : Hub
    {
        public async Task UpdateHighestBid(int listingId, string newHighestBidAmount)
        {
            await Clients.Group($"listing-{listingId}")
                .SendAsync("ReceiveNewHighestBid", newHighestBidAmount);
        }

        public async Task JoinListingGroup(int listingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"listing-{listingId}");
        }

        public async Task NotifyAuctionEnded(int listingId, string winner, decimal finalPrice)
        {
            await Clients.Group($"listing-{listingId}")
                .SendAsync("AuctionEnded", new { listingId, winner, finalPrice });
        }

        public async Task AdminBroadcast(string message)
        {
            await Clients.All.SendAsync("ReceiveAdminMessage", message);
        }
    }
}
