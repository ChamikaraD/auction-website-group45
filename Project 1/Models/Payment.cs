using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_1.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int ListingId { get; set; }
        [ForeignKey("ListingId")]
        public Listing? Listing { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        // --- ADD THESE ---
        [Required]
        public string UserId { get; set; } // ID of the user who paid
        // You might want a navigation property to IdentityUser here too

        public string? StripePaymentIntentId { get; set; } // Store Stripe's unique ID
        // ---------------
    }
}