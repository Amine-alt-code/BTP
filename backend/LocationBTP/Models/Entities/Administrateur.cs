using System.ComponentModel.DataAnnotations;

namespace LocationBTP.Models.Entities
{
    public class Administrateur
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Login { get; set; }

        [Required]
        [StringLength(200)]
        public string PasswordHash { get; set; }

        // Propriétés de navigation
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Contrat> Contrats { get; set; }
        public ICollection<Machine> Machines { get; set; }
    }
}
