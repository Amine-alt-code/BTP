using LocationBTP.Data;
using LocationBTP.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocationBTP.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Page Admin
            if (User.IsInRole("Admin"))
            {
                ViewBag.NbMachines = await _context.Machines.CountAsync();
                ViewBag.NbClients = await _context.Clients.CountAsync();
                ViewBag.NbReservations = await _context.Reservations.CountAsync();
                ViewBag.NbContrats = await _context.Contrats.CountAsync();
                ViewBag.NbMaintenances = await _context.Maintenances
                    .Where(m => m.Statut == "EnCours").CountAsync();
                ViewBag.DernieresReservations = await _context.Reservations
                    .Include(r => r.Client)
                    .Include(r => r.Machine)
                    .OrderByDescending(r => r.DateCreation)
                    .Take(5)
                    .ToListAsync();
                ViewBag.MachinesDisponibles = await _context.Machines
                    .Include(m => m.Categorie)
                    .Where(m => m.Statut == StatutMachine.Disponible)
                    .Take(6)
                    .ToListAsync();

                return View("IndexAdmin");
            }

            // Page Technicien
            if (User.IsInRole("Technicien"))
            {
                ViewBag.MachinesEnMaintenance = await _context.Machines
                    .Include(m => m.Categorie)
                    .Where(m => m.Statut == StatutMachine.EnMaintenance)
                    .ToListAsync();
                ViewBag.MaintenancesEnCours = await _context.Maintenances
                    .Include(m => m.Machine)
                    .Where(m => m.Statut == "EnCours")
                    .ToListAsync();
                ViewBag.EtatsDesLieux = await _context.EtatsDesLieux
                    .Include(e => e.Contrat)
                    .OrderByDescending(e => e.Date)
                    .Take(5)
                    .ToListAsync();

                // ← NOUVEAU : Contrats sans état des lieux départ
                ViewBag.ContratsATraiter = await _context.Contrats
                    .Include(c => c.Reservation)
                        .ThenInclude(r => r.Machine)
                    .Include(c => c.Reservation)
                        .ThenInclude(r => r.Client)
                    .Include(c => c.EtatsDesLieux)
                    .Where(c => c.Statut == StatutContrat.Signe
                        && !c.EtatsDesLieux.Any(e => e.Type == TypeEtatDesLieux.Depart))
                    .ToListAsync();

                // ← NOUVEAU : Contrats avec départ mais sans retour
                ViewBag.ContratsEnCours = await _context.Contrats
                    .Include(c => c.Reservation)
                        .ThenInclude(r => r.Machine)
                    .Include(c => c.Reservation)
                        .ThenInclude(r => r.Client)
                    .Include(c => c.EtatsDesLieux)
                    .Where(c => c.Statut == StatutContrat.Signe
                        && c.EtatsDesLieux.Any(e => e.Type == TypeEtatDesLieux.Depart)
                        && !c.EtatsDesLieux.Any(e => e.Type == TypeEtatDesLieux.Retour))
                    .ToListAsync();

                return View("IndexTechnicien");
            }

            // Page Client
            ViewBag.MachinesDisponibles = await _context.Machines
                .Include(m => m.Categorie)
                .Where(m => m.Statut == StatutMachine.Disponible)
                .ToListAsync();

            return View("IndexClient");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}