using LocationBTP.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace LocationBTP.Models.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public DateTime DateDebut { get; set; }

        [Required]
        public DateTime DateFin { get; set; }

        public StatutReservation Statut { get; set; } = StatutReservation.EnAttente;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string Notes { get; set; }

        // Clés étrangères
        public int ClientId { get; set; }
        public int MachineId { get; set; }

        // Propriétés de navigation
        public Client Client { get; set; }
        public Machine Machine { get; set; }
        public Contrat Contrat { get; set; }
        public ICollection<Notification> Notifications { get; set; }

        // Méthodes métier
        public int CalculerDuree()
        {
            return (DateFin - DateDebut).Days;
        }

        public decimal CalculerMontant()
        {
            return CalculerDuree() * Machine.TarifJournalier;
        }
    }
}