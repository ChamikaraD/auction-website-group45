using Microsoft.AspNetCore.Identity; // Make sure this 'using' is here
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project_1.Models;

namespace Project_1.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> // Changed from IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Listing> Listings { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- THIS IS THE FIX ---
            // We are changing DeleteBehavior.Cascade to DeleteBehavior.NoAction
            // to prevent the multiple cascade paths error.

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Listing)
                .WithMany(l => l.Bids)
                .HasForeignKey(b => b.ListingId)
                .OnDelete(DeleteBehavior.NoAction); // CHANGED

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Listing)
                .WithMany(l => l.Comments)
                .HasForeignKey(c => c.ListingId)
                .OnDelete(DeleteBehavior.NoAction); // CHANGED

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Listing)
                .WithMany()
                .HasForeignKey(p => p.ListingId)
                .OnDelete(DeleteBehavior.NoAction); // CHANGED
        }
    }
}