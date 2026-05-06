using System.ComponentModel.DataAnnotations;
using LocationBTP.Models.Enums;

namespace LocationBTP.Models.Entities
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; }

        [Required]
        [EmailAddress]  /* verifie automatiquement si l'email est correcte*/
        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Telephone { get; set; }

        [StringLength(300)]
        public string Adresse { get; set; }

        [StringLength(50)]
        public string SIRET { get; set; }

        public TypeClient TypeClient { get; set; } = TypeClient.Particulier;

        public DateTime DateInscription { get; set; } = DateTime.Now;

        // Propriétés de navigation
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}