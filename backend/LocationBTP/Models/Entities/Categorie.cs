using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;

namespace LocationBTP.Models.Entities
{
    public class Categorie
    {
        public int Id { get; set; }

        [Required]              /*Champ obligatoire — ne peut pas être vide*/
        [StringLength(100)]    /*limité à 100 caractères dans la base de donnée*/
        public string Nom { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Icone { get; set; }

        // Propriété de navigation
        public ICollection<Machine> Machines { get; set; }
    }
}