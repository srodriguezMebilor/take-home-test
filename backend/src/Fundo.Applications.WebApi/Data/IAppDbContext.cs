using Fundo.Applications.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Data
{
    public interface IAppDbContext
    {
        DbSet<Loan> Loans { get; set; }

        void Migrate();

        Task SaveChangesAsync();
    }
}