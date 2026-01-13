

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.Controllers
{
    public record LoanDto: IValidatableObject
    {

        /// <summary>
        /// El ID debe ser 0 para nuevos préstamos.
        /// </summary>
        /// <example>0</example>
        [Range(0, 0, ErrorMessage = "The Id of the loan must be zero")]
        public int Id { get; init; }

        /// <summary>
        /// Nombre completo del solicitante.
        /// </summary>
        /// <example>Sebastian Rodriguez</example>
        [Required(ErrorMessage = "The applicant name can not be empty")]
        public string ApplicantName { get; init; } = string.Empty;

        /// <summary>
        /// Monto solicitado del préstamo
        /// </summary>
        /// <example>5000</example>
        [Range(0.01, double.MaxValue, ErrorMessage = "The amount must be greater than zero")]
        public decimal Amount { get; init; }


        /// <summary>
        /// Balance actual, al dar de alta siempre tiene que ser igual al Amount
        /// </summary>
        /// <example>5000</example>
        public decimal CurrentBalance { get; init; }


        /// <summary>
        /// El status, toma solo dos valores: paied or active. Pero al dar de alta el préstamo, siempre tiene que ser active.
        /// </summary>
        /// <example>5000</example>
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
