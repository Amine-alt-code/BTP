using LocationBTP.Data;
using LocationBTP.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocationBTP.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        // Envoyer une notification
        public async Task EnvoyerNotification(int clientId, int reservationId, string type, string message, string canal)
        {
            var notification = new Notification
            {
                ClientId = clientId,
                ReservationId = reservationId,
                Type = type,
                Message = message,
                Canal = canal,
                DateEnvoi = DateTime.Now,
                EstLue = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // Marquer une notification comme lue
        public async Task MarquerLue(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                throw new Exception("Notification introuvable !");

            notification.EstLue = true;
            await _context.SaveChangesAsync();
        }

        // Obtenir les notifications d'un client
        public async Task<List<Notification>> GetNotificationsClient(int clientId)
        {
            return await _context.Notifications
                .Include(n => n.Reservation)
                .Where(n => n.ClientId == clientId)
                .OrderByDescending(n => n.DateEnvoi)
                .ToListAsync();
        }

        // Obtenir toutes les notifications non lues
        public async Task<List<Notification>> GetNotificationsNonLues()
        {
            return await _context.Notifications
                .Include(n => n.Client)
                .Include(n => n.Reservation)
                .Where(n => !n.EstLue)
                .OrderByDescending(n => n.DateEnvoi)
                .ToListAsync();
        }
    }
}