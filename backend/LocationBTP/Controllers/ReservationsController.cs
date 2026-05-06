using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LocationBTP.Data;
using LocationBTP.Models.Entities;
using LocationBTP.Models.Enums;
using LocationBTP.Services;

namespace LocationBTP.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ContratService _contratService;
        private readonly NotificationService _notificationService;

        public ReservationsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ContratService contratService,
            NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _contratService = contratService;
            _notificationService = notificationService;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Machine)
                .OrderByDescending(r => r.DateCreation)
                .ToListAsync();
            return View(reservations);
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Machine)
                .Include(r => r.Contrat)
                    .ThenInclude(c => c.Caution)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null) return NotFound();
            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create(int? machineId)
        {
            if (machineId.HasValue)
            {
                var machine = _context.Machines.Find(machineId.Value);
                ViewBag.MachineId = machineId.Value;
                ViewBag.Machine = machine;
            }
            return View();
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string Nom, string Prenom, string Email, string Telephone,
            DateTime DateDebut, DateTime DateFin, string Notes, int MachineId)
        {
            // 1. Trouver ou créer le client
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == Email);

            if (client == null)
            {
                client = new Client
                {
                    Nom = Nom,
                    Prenom = Prenom,
                    Email = Email,
                    Telephone = Telephone,
                    DateInscription = DateTime.Now,
                    TypeClient = TypeClient.Particulier
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }

            // 2. Vérifier conflit de dates
            var conflit = await _context.Reservations
                .AnyAsync(r => r.MachineId == MachineId
                    && r.Statut != StatutReservation.Annulee
                    && r.DateDebut < DateFin
                    && r.DateFin > DateDebut);

            if (conflit)
            {
                var machine = await _context.Machines.FindAsync(MachineId);
                ViewBag.MachineId = MachineId;
                ViewBag.Machine = machine;
                ViewBag.Erreur = "Cette machine est déjà réservée sur ces dates.";
                return View();
            }

            // 3. Créer la réservation
            var reservation = new Reservation
            {
                ClientId = client.Id,
                MachineId = MachineId,
                DateDebut = DateDebut,
                DateFin = DateFin,
                Notes = Notes,
                Statut = StatutReservation.EnAttente,
                DateCreation = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var machineNotif = await _context.Machines.FindAsync(MachineId);
            await _notificationService.EnvoyerNotification(
                client.Id,
                reservation.Id,
                "DemandeReservation",
                $"Nouvelle demande de réservation pour {machineNotif?.Nom} du {DateDebut:dd/MM/yyyy} au {DateFin:dd/MM/yyyy}.",
                "Système"
            );

            return RedirectToAction("Confirmation", new { id = reservation.Id });
        }

        // GET: Reservations/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Machine)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();
            return View(reservation);
        }

        // POST: Reservations/Confirmer/5
        [HttpPost]
        public async Task<IActionResult> Confirmer(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Machine)
                .Include(r => r.Client)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            // Générer contrat + caution + PDF + email en une ligne
            await _contratService.GenererContrat(id);

            // Créer notification
            await _notificationService.EnvoyerNotification(
                reservation.ClientId,
                reservation.Id,
                "Confirmation",
                $"Votre réservation pour {reservation.Machine.Nom} a été confirmée. Le contrat a été envoyé par email.",
                "Email"
            );

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Reservations/Annuler/5
        [HttpPost]
        public async Task<IActionResult> Annuler(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Machine)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            reservation.Statut = StatutReservation.Annulee;

            if (reservation.Machine.Statut == StatutMachine.Reservee)
                reservation.Machine.Statut = StatutMachine.Disponible;

            await _context.SaveChangesAsync();

            // Notification annulation
            await _notificationService.EnvoyerNotification(
                reservation.ClientId,
                reservation.Id,
                "Annulation",
                $"Votre réservation pour {reservation.Machine.Nom} a été annulée.",
                "Système"
            );

            return RedirectToAction(nameof(Index));
        }

        // POST: Reservations/PayerCaution/5
        [HttpPost]
        public async Task<IActionResult> PayerCaution(int id, string modePaiement)
        {
            var caution = await _context.Cautions
                .Include(c => c.Contrat)
                    .ThenInclude(c => c.Reservation)
                        .ThenInclude(r => r.Machine)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caution == null) return NotFound();

            caution.Statut = StatutCaution.Versee;
            caution.DateVersement = DateTime.Now;
            caution.ModePaiement = modePaiement;

            if (caution.Contrat?.Reservation?.Machine != null)
                caution.Contrat.Reservation.Machine.Statut = StatutMachine.Reservee;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = caution.Contrat.ReservationId });
        }

        // POST: Reservations/RembourserCaution/5
        [HttpPost]
        public async Task<IActionResult> RembourserCaution(int id)
        {
            var caution = await _context.Cautions
                .Include(c => c.Contrat)
                    .ThenInclude(c => c.Reservation)
                        .ThenInclude(r => r.Machine)
                .Include(c => c.Contrat)
                    .ThenInclude(c => c.Reservation)
                        .ThenInclude(r => r.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caution == null) return NotFound();

            // Rembourser la caution
            caution.Rembourser();

            // Machine → Disponible automatiquement
            if (caution.Contrat?.Reservation?.Machine != null)
                caution.Contrat.Reservation.Machine.Statut = StatutMachine.Disponible;

            // Contrat → Terminé
            if (caution.Contrat != null)
                caution.Contrat.Statut = StatutContrat.Termine;

            // Réservation → Terminée
            if (caution.Contrat?.Reservation != null)
                caution.Contrat.Reservation.Statut = StatutReservation.Terminee;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = caution.Contrat.ReservationId });
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Email", reservation.ClientId);
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Nom", reservation.MachineId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DateDebut,DateFin,Statut,DateCreation,Notes,ClientId,MachineId")] Reservation reservation)
        {
            if (id != reservation.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Email", reservation.ClientId);
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Nom", reservation.MachineId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var reservation = await _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Machine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null) return NotFound();
            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
                _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}