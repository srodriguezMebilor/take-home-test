using Fundo.Applications.WebApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Data
{
    public class AppDbContext : DbContext, IAppDbContext
    {


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Loan> Loans { get; set; }

        public void Migrate()
        {
            this.Database.Migrate();
        }

        public Task SaveChangesAsync() { 
            return base.SaveChangesAsync(); 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Loan>().Property(l => l.Amount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Loan>().Property(l => l.CurrentBalance).HasColumnType("decimal(18,2)");

            // Seed Data: 
            modelBuilder.Entity<Loan>().HasData(
                new Loan { Id = 1, Amount = 1500.00m, CurrentBalance = 500.00m, ApplicantName = "Maria Silva", Status = "active" },
                new Loan { Id = 2, Amount = 2000.00m, CurrentBalance = 0.00m, ApplicantName = "Juan Perez", Status = "paid" }
            );
        }
    }
}