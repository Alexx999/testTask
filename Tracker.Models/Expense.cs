using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Tracker.Models
{
    public class Expense
    {
        public Expense()
        {
            ExpenseId = Guid.NewGuid();
        }

        [Key]
        public Guid ExpenseId { get; set; }

        public DateTime Date { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Description { get; set; }

        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Ammount can't be negative")]
        public Decimal Amount { get; set; }

        [StringLength(200)]
        public string Comment { get; set; }

        [JsonIgnore]
        public string ApplicationUserID { get; set; }
    }
}
