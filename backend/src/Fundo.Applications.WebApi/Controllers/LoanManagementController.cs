using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Controllers
{


    [ApiController]
    [Route("loan/")]
    public class LoanManagementController : ControllerBase
    {
        // El Controlador no depende de la implementación AppDbContext, sino de la abstracción IAppDbContext (uso de inyección de dependencia)
        private readonly IAppDbContext _context;
        private readonly IConfiguration _configuration;

        public LoanManagementController(IAppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET:loan/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans()
        {
            // mostrar todos los préstamos
            var loans = await _context.Loans.ToListAsync();
            return Ok(loans.Select(l => new LoanDto
            {
                Id = l.Id,
                ApplicantName = l.ApplicantName,
                Amount = l.Amount,
                CurrentBalance = l.CurrentBalance,
                Status = l.Status
            }));
        }

        // GET: loan/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LoanDto>> GetLoan(int id)
        {
            // Recuperar detalles del préstamo por ID
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound(new { message = $"Loan with ID {id} not found." });
            }

            return Ok(new LoanDto
            {
                Id = loan.Id,
                ApplicantName = loan.ApplicantName,
                Amount = loan.Amount,
                CurrentBalance = loan.CurrentBalance,
                Status = loan.Status
            });
        }

        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(LoanDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EndpointDescription("Crea un nuevo préstamo validando que el monto coincida con el balance actual.")]
        [HttpPost]
        public async Task<ActionResult<LoanDto>> CreateLoan(LoanDto loan)
        {


            // Creo la entidad en una variable local
            var newLoanEntity = new Loan
            {
                ApplicantName = loan.ApplicantName,
                Amount = loan.Amount,
                CurrentBalance = loan.CurrentBalance,
                Status = loan.Status
            };

            // Agrego la entidad al contexto
            _context.Loans.Add(newLoanEntity);

            // Al hacer SaveChanges, EF Core viaja a la DB, 
            // inserta el registro y SQL Server le devuelve el ID generado.
            // EF Core lo inyecta automáticamente en 'newLoanEntity.Id'.
            await _context.SaveChangesAsync();

            // Actualizo el DTO con el ID real antes de devolverlo
            var finalDto = loan with { Id = newLoanEntity.Id };

            return CreatedAtAction(nameof(GetLoan), new { newLoanEntity.Id }, finalDto);
        }

        [HttpPost("{id}/payment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointDescription("Registra un pago para un préstamo específico, deduciéndolo del balance actual y actualizando el estado a 'paid' si el balance llega a cero.")]
        public async Task<IActionResult> MakePayment(int id, [FromBody] decimal paymentAmount)
        {
            try
            {
                // Para deducir el pago del balance actual
                var loan = await _context.Loans.FindAsync(id);

                if (loan == null)
                    return NotFound(new { message = "Loan not found." });

                if (paymentAmount <= 0)
                    return BadRequest(new { message = "Payment amount must be greater than zero." });

                if (paymentAmount > loan.CurrentBalance)
                    return BadRequest(new { message = "Payment exceeds current balance." });

                // Lógica de negocio solicitada
                loan.CurrentBalance -= paymentAmount;

                // Si el balance llega a 0 o menos, actualizamos el estado
                if (loan.CurrentBalance <= 0)
                {
                    loan.CurrentBalance = 0;
                    loan.Status = "paid";
                }

                // Para probar el control de la concurrencia.
                // Simulación de concurrencia configurable
                var testDelayEnabled = _configuration.GetValue<bool>("FeatureManagement:EnableConcurrencyTestDelay");
                if (testDelayEnabled)
                {
                    var delay = _configuration.GetValue<int>("FeatureManagement:ConcurrencyDelayMs");
                    await Task.Delay(delay);
                }

                await _context.SaveChangesAsync();

                return Ok(new LoanDto
                {
                    Id = loan.Id,
                    ApplicantName = loan.ApplicantName,
                    Amount = loan.Amount,
                    CurrentBalance = loan.CurrentBalance,
                    Status = loan.Status
                });

            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new
                {
                    message = "The loan was modified by other process. Please refresh de data..."
                });
            }
        }
    }
}
