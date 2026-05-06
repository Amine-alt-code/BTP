using System.ComponentModel.DataAnnotations;
using LocationBTP.Models.Enums;

namespace LocationBTP.Models.Entities
{
    public class EtatDesLieux
    {
        public int Id { get; set; }

        public TypeEtatDesLieux Type { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string Observations { get; set; }

        [Range(0, 100)]
        public int NiveauCarburant { get; set; }

        public bool EstConforme { get; set; } = true;

        // Clés étrangères
        public int ContratId { get; set; }
        public int TechnicienId { get; set; }

        // Propriétés de navigation
        public Contrat Contrat { get; set; }
        public Technicien Technicien { get; set; }
        public ICollection<PhotoEtatLieux> Photos { get; set; }

        // Méthode métier
        public void AjouterPhoto(string chemin)
        {
            if (Photos == null)
                Photos = new List<PhotoEtatLieux>();

            Photos.Add(new PhotoEtatLieux
            {
                CheminFichier = chemin,
                EtatDesLieuxId = Id
            });
        }
    }
}