namespace Fundo.Applications.WebApi.Controllers
{
    public record LoanDto
    {
        public int Id { get; set; }
        public string ApplicantName { get; set; }
        public decimal Amount { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Status { get; set; }
    }
}
