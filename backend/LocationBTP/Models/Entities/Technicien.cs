using System.ComponentModel.DataAnnotations;

namespace LocationBTP.Models.Entities
{
    public class Technicien
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

        [StringLength(20)]
        public string Telephone { get; set; }

        [Required]
        [StringLength(100)]
        public string Login { get; set; }

        [Required]
        [StringLength(200)]
        public string PasswordHash { get; set; }

        // Propriétés de navigation
        public ICollection<EtatDesLieux> EtatsDesLieux { get; set; }
        public ICollection<Maintenance> Maintenances { get; set; }
    }
}