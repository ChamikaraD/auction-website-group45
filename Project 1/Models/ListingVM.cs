using System.ComponentModel.DataAnnotations;

namespace Project_1.Models
{
    // We are using your existing ListingVM class
    public class ListingVM
    {
        [Required]
        [Display(Name = "Coin Name")]
        public string CoinName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Starting Price")]
        [DataType(DataType.Currency)]
        public decimal StartingPrice { get; set; }

        [Required]
        [Display(Name = "Upload Image")]
        public IFormFile ImageFile { get; set; }

        // --- Coin Specific Fields ---
        [Required]
        public int Year { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Denomination { get; set; } // e.g., "$1", "50 Cents"

        [Required]
        public string Grade { get; set; } // e.g., "MS-65", "VF-20"

        [Display(Name = "Mint Mark")]
        public string? MintMark { get; set; } // e.g., "P", "D", "S"

        [Display(Name = "Metal Composition")]
        public string? Composition { get; set; } // e.g., "90% Silver"

        // --- Auction Duration ---
        [Required]
        [Display(Name = "Days")]
        [Range(0, 30)]
        public int DurationDays { get; set; }

        [Required]
        [Display(Name = "Hours")]
        [Range(0, 23)]
        public int DurationHours { get; set; }
    }
}