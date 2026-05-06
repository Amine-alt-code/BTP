using System.ComponentModel.DataAnnotations;

namespace LocationBTP.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        public DateTime DateEnvoi { get; set; } = DateTime.Now;

        public bool EstLue { get; set; } = false;

        [StringLength(50)]
        public string Canal { get; set; }

        // Clés étrangères
        public int ClientId { get; set; }
        public int ReservationId { get; set; }

        // Propriétés de navigation
        public Client Client { get; set; }
        public Reservation Reservation { get; set; }

        // Méthodes métier
        public void Envoyer()
        {
            DateEnvoi = DateTime.Now;
        }

        public void MarquerLue()
        {
            EstLue = true;
        }
    }
}