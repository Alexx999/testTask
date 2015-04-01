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
            //Make it fit into DATETIME SQL type by default
            Date = new DateTime(1753, 1, 1);
        }

        [Key]
        public Guid ExpenseId { get; set; }

        [CustomValidation(typeof(Expense), "ValidateDate")]
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

        public static ValidationResult ValidateDate(DateTime date)
        {
            if (date.Year >= 1753)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Date invalid");
        }
    }
}
