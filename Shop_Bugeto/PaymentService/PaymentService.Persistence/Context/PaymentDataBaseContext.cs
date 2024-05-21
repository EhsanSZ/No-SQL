using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Contexts;
using PaymentService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Persistence.Context
{
    public class PaymentDataBaseContext : DbContext, IPaymentDataBaseContext
    {

        public PaymentDataBaseContext(DbContextOptions<PaymentDataBaseContext>  options) : base(options)
        {

        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }

 
    }
}
