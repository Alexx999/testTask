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

        [MaxLength(200)]
        public string Description { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Ammount can't be negative")]
        public Decimal Amount { get; set; }

        [MaxLength(200)]
        public string Comment { get; set; }

        [JsonIgnore]
        public string ApplicationUserID { get; set; }
    }
}
