using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocationBTP.Models.Enums;

namespace LocationBTP.Models.Entities
{
    public class Caution
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        public DateTime? DateVersement { get; set; }

        public DateTime? DateRemboursement { get; set; }

        public StatutCaution Statut { get; set; } = StatutCaution.EnAttente;

        [StringLength(100)]
        public string ModePaiement { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        // Clé étrangère
        public int ContratId { get; set; }

        // Propriété de navigation
        public Contrat Contrat { get; set; }

        // Méthodes métier
        public void Rembourser()
        {
            Statut = StatutCaution.Remboursee;
            DateRemboursement = DateTime.Now;
        }

        public void Retenir()
        {
            Statut = StatutCaution.Retenue;
        }
    }
}