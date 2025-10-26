﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project_1.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        // --- ADD THIS LINE ---
        public DateTime CreatedAt { get; set; } // Property for the timestamp

        [Required]
        public string? IdentityUserId { get; set; }
        [ForeignKey("IdentityUserId")]
        public IdentityUser? User { get; set; }

        public int? ListingId { get; set; }
        [ForeignKey("ListingId")]
        public Listing? Listing { get; set; }
    }
}