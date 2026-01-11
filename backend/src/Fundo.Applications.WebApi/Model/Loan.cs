using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.Models
{
    public class Loan
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal CurrentBalance { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string Status { get; set; } = "active"; // "active" or "paid"

        // Propiedad para manejo de concurrencia
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}