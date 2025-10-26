using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project_1.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_1.Models
{
    public class Listing
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // --- CHANGED THIS FROM 'double' TO 'decimal' ---
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public string ImagePath { get; set; }
        public bool IsSold { get; set; } = false;

        [Required]
        public string? IdentityUserId { get; set; }
        [ForeignKey("IdentityUserId")]
        public IdentityUser? User { get; set; }

        public List<Bid>? Bids { get; set; }
        public List<Comment>? Comments { get; set; }

        public DateTime ClosingTime { get; set; }

        [DataType(DataType.Currency)]
        public decimal CurrentBid { get; set; } // Also ensured this is decimal

        public string Status
        {
            get
            {
                if (IsSold) return "Sold";
                if (DateTime.UtcNow > ClosingTime) return "Closed";
                return "Active"; // default
            }
        }

        // --- ADD ALL THESE NEW COIN PROPERTIES ---
        [Required]
        public int Year { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Denomination { get; set; }

        [Required]
        public string Grade { get; set; }

        public string? MintMark { get; set; }

        public string? Composition { get; set; }
    }
}