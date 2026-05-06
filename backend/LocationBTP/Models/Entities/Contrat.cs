using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocationBTP.Models.Enums;

namespace LocationBTP.Models.Entities
{
    public class Contrat
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Numero { get; set; }

        public DateTime DateSignature { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantHT { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TVA { get; set; }

        public StatutContrat Statut { get; set; } = StatutContrat.Brouillon;

        [StringLength(500)]
        public string FichierPDF { get; set; }

        // Clé étrangère
        public int ReservationId { get; set; }

        // Propriétés de navigation
        public Reservation Reservation { get; set; }
        public Caution Caution { get; set; }
        public ICollection<EtatDesLieux> EtatsDesLieux { get; set; }

        // Méthodes métier
        public string GenererNumero()
        {
            return $"CTR-{DateTime.Now.Year}-{Id:D4}";
        }

        public string GenererPDF()
        {
            return $"contrat_{Numero}.pdf";
        }
    }
}