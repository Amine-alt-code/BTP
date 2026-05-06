using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocationBTP.Models.Enums;

namespace LocationBTP.Models.Entities
{
    public class Machine
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required]
        [StringLength(50)]
        public string Reference { get; set; }

        [StringLength(100)]
        public string Marque { get; set; }

        [StringLength(100)]
        public string Modele { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TarifJournalier { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]

        public decimal MontantCaution { get; set; }


        public StatutMachine Statut { get; set; } = StatutMachine.Disponible;

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        // Clé étrangère
        public int CategorieId { get; set; }

        // Propriétés de navigation
        public Categorie Categorie { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Maintenance> Maintenances { get; set; }
    }
}