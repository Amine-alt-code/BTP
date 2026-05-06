using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationBTP.Models.Entities
{
    public class Maintenance
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; }

        [Required]
        public DateTime DateDebut { get; set; }

        public DateTime? DateFin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cout { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Statut { get; set; } = "EnCours";

        // Clés étrangères
        public int MachineId { get; set; }
        public int TechnicienId { get; set; }

        // Propriétés de navigation
        public Machine Machine { get; set; }
        public Technicien Technicien { get; set; }

        // Méthode métier
        public void Terminer()
        {
            Statut = "Terminee";
            DateFin = DateTime.Now;
        }
    }
}