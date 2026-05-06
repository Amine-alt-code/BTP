using LocationBTP.Data;
using LocationBTP.Models.Entities;
using LocationBTP.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LocationBTP.Services
{
    public class ReservationService
    {
        private readonly AppDbContext _context;

        public ReservationService(AppDbContext context)
        {
            _context = context;
        }

        // Vérifier si une machine est disponible
        public async Task<bool> IsMachineDisponible(int machineId, DateTime dateDebut, DateTime dateFin)
        {
            return !await _context.Reservations
                .AnyAsync(r => r.MachineId == machineId
                    && r.Statut != StatutReservation.Annulee
                    && r.DateDebut < dateFin
                    && r.DateFin > dateDebut);
        }

        // Créer une réservation
        public async Task<Reservation> CreerReservation(int clientId, int machineId, DateTime dateDebut, DateTime dateFin, string notes)
        {
            // Vérifier disponibilité
            if (!await IsMachineDisponible(machineId, dateDebut, dateFin))
                throw new Exception("La machine n'est pas disponible pour ces dates !");

            var machine = await _context.Machines.FindAsync(machineId);

            var reservation = new Reservation
            {
                ClientId = clientId,
                MachineId = machineId,
                DateDebut = dateDebut,
                DateFin = dateFin,
                Notes = notes,
                Statut = StatutReservation.EnAttente,
                DateCreation = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        // Confirmer une réservation
        public async Task ConfirmerReservation(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
                throw new Exception("Réservation introuvable !");

            reservation.Statut = StatutReservation.Confirmee;
            await _context.SaveChangesAsync();
        }

        // Annuler une réservation
        public async Task AnnulerReservation(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
                throw new Exception("Réservation introuvable !");

            reservation.Statut = StatutReservation.Annulee;
            await _context.SaveChangesAsync();
        }

        // Obtenir toutes les réservations
        public async Task<List<Reservation>> GetReservations()
        {
            return await _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Machine)
                .ToListAsync();
        }
    }
}