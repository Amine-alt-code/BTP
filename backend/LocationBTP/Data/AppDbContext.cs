using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LocationBTP.Models.Entities;

namespace LocationBTP.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Contrat> Contrats { get; set; }
        public DbSet<Caution> Cautions { get; set; }
        public DbSet<EtatDesLieux> EtatsDesLieux { get; set; }
        public DbSet<PhotoEtatLieux> PhotosEtatLieux { get; set; }
        public DbSet<Technicien> Techniciens { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Administrateur> Administrateurs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Client)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Reservation)
                .WithMany(r => r.Notifications)
                .HasForeignKey(n => n.ReservationId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}