

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.Controllers
{
    public record LoanDto: IValidatableObject
    {
        [Range(0, 0, ErrorMessage = "The Id of the loan must be zero")]
        public int Id { get; init; }

        [Required(ErrorMessage = "The applicant name can not be empty")]
        public string ApplicantName { get; init; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "The amount must be greater than zero")]
        public decimal Amount { get; init; }

        public decimal CurrentBalance { get; init; }

        [RegularExpression("active", ErrorMessage = "The status must be active")]
        public string Status { get; init; } = "active";

        // Lógica para validaciones cruzadas (comparar dos propiedades)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CurrentBalance != Amount)
            {
                yield return new ValidationResult(
                    "The amount must be equals to current balance",
                    new[] { nameof(CurrentBalance) }
                );
            }
        }
    }

    
}
