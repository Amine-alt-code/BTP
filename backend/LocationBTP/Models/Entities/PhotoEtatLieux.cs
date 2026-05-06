using System.ComponentModel.DataAnnotations;

namespace LocationBTP.Models.Entities
{
    public class PhotoEtatLieux
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string CheminFichier { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        // Clé étrangère
        public int EtatDesLieuxId { get; set; }

        // Propriété de navigation
        public EtatDesLieux EtatDesLieux { get; set; }
    }
}