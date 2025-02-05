using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CPCoded.Models;
using System.Collections.Generic;

namespace CPCoded.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }




        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanRepayment> LoanRepayments { get; set; }





    }
}
