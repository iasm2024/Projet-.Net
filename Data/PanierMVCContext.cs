using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PanierMVC.Models;

namespace PanierMVC.Data
{
    public class PanierMVCContext : DbContext
    {
        public PanierMVCContext (DbContextOptions<PanierMVCContext> options)
            : base(options)
        {
        }

        public DbSet<PanierMVC.Models.Produit> Produit { get; set; } = default!;

        public DbSet<PanierMVC.Models.User> User { get; set; } = default!;
    }
}
