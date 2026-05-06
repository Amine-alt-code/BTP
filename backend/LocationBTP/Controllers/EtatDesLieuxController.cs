using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LocationBTP.Data;
using LocationBTP.Models.Entities;
using LocationBTP.Models.Enums;

namespace LocationBTP.Controllers
{
    public class EtatDesLieuxController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EtatDesLieuxController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: EtatDesLieux
        public async Task<IActionResult> Index()
        {
            var etats = await _context.EtatsDesLieux
                .Include(e => e.Contrat)
                    .ThenInclude(c => c.Reservation)
                        .ThenInclude(r => r.Machine)
                .Include(e => e.Technicien)
                .Include(e => e.Photos)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
            return View(etats);
        }

        // GET: EtatDesLieux/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var etat = await _context.EtatsDesLieux
                .Include(e => e.Contrat)
                    .ThenInclude(c => c.Reservation)
                        .ThenInclude(r => r.Machine)
                .Include(e => e.Technicien)
                .Include(e => e.Photos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (etat == null) return NotFound();
            return View(etat);
        }

        // GET: EtatDesLieux/Create
        public IActionResult Create(int? contratId)
        {
            ViewData["ContratId"] = new SelectList(_context.Contrats
                .Include(c => c.Reservation)
                    .ThenInclude(r => r.Machine)
                .Select(c => new {
                    c.Id,
                    Libelle = c.Numero + " — " + c.Reservation.Machine.Nom
                }), "Id", "Libelle", contratId);
            ViewData["TechnicienId"] = new SelectList(_context.Techniciens
                .Select(t => new {
                    t.Id,
                    Libelle = t.Nom + " " + t.Prenom
                }), "Id", "Libelle");
            return View();
        }

        // POST: EtatDesLieux/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Type,Date,Observations,NiveauCarburant,EstConforme,ContratId,TechnicienId")] EtatDesLieux etat,
            List<IFormFile> Photos)
        {
            if (ModelState.IsValid)
            {
                _context.Add(etat);
                await _context.SaveChangesAsync();

                // Upload photos
                if (Photos != null && Photos.Any())
                {
                    var dossier = Path.Combine(_env.WebRootPath, "photos-etats-lieux");
                    Directory.CreateDirectory(dossier);

                    foreach (var photo in Photos)
                    {
                        if (photo.Length > 0)
                        {
                            var nomFichier = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                            var chemin = Path.Combine(dossier, nomFichier);
                            using (var stream = new FileStream(chemin, FileMode.Create))
                                await photo.CopyToAsync(stream);

                            _context.PhotosEtatLieux.Add(new PhotoEtatLieux
                            {
                                EtatDesLieuxId = etat.Id,
                                CheminFichier = $"/photos-etats-lieux/{nomFichier}",
                                Description = photo.FileName
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Si c'est un état des lieux RETOUR
                if (etat.Type == TypeEtatDesLieux.Retour)
                {
                    var contrat = await _context.Contrats
                        .Include(c => c.Caution)
                        .Include(c => c.Reservation)
                            .ThenInclude(r => r.Machine)
                        .Include(c => c.Reservation)
                            .ThenInclude(r => r.Client)
                        .FirstOrDefaultAsync(c => c.Id == etat.ContratId);

                    if (contrat != null)
                    {
                        if (etat.EstConforme)
                        {
                            // Caution → À rembourser
                            if (contrat.Caution != null)
                            {
                                contrat.Caution.Statut = StatutCaution.ARemborser;
                                contrat.Caution.Notes = "État des lieux retour conforme — remboursement à effectuer.";
                            }

                            // Notification admin
                            if (contrat.Reservation?.Client != null)
                            {
                                var notification = new Notification
                                {
                                    ClientId = contrat.Reservation.ClientId,
                                    ReservationId = contrat.ReservationId,
                                    Type = "RemboursementCaution",
                                    Message = $"État des lieux retour conforme pour {contrat.Reservation.Machine?.Nom} — Caution de {contrat.Caution?.Montant:N2} DH à rembourser au client {contrat.Reservation.Client?.Nom} {contrat.Reservation.Client?.Prenom}.",
                                    DateEnvoi = DateTime.Now,
                                    EstLue = false,
                                    Canal = "Système"
                                };
                                _context.Notifications.Add(notification);
                            }
                        }
                        else
                        {
                            // Non conforme → Caution retenue
                            if (contrat.Caution != null)
                            {
                                contrat.Caution.Statut = StatutCaution.Retenue;
                                contrat.Caution.Notes = "État des lieux retour non conforme — caution retenue.";
                            }

                            // Notification admin
                            if (contrat.Reservation?.Client != null)
                            {
                                var notification = new Notification
                                {
                                    ClientId = contrat.Reservation.ClientId,
                                    ReservationId = contrat.ReservationId,
                                    Type = "CautionRetenue",
                                    Message = $"État des lieux retour NON conforme pour {contrat.Reservation.Machine?.Nom} — Caution retenue.",
                                    DateEnvoi = DateTime.Now,
                                    EstLue = false,
                                    Canal = "Système"
                                };
                                _context.Notifications.Add(notification);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ContratId"] = new SelectList(_context.Contrats
                .Include(c => c.Reservation).ThenInclude(r => r.Machine)
                .Select(c => new { c.Id, Libelle = c.Numero + " — " + c.Reservation.Machine.Nom }),
                "Id", "Libelle", etat.ContratId);
            ViewData["TechnicienId"] = new SelectList(_context.Techniciens
                .Select(t => new { t.Id, Libelle = t.Nom + " " + t.Prenom }),
                "Id", "Libelle", etat.TechnicienId);
            return View(etat);
        }

        // GET: EtatDesLieux/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var etat = await _context.EtatsDesLieux.FindAsync(id);
            if (etat == null) return NotFound();

            ViewData["ContratId"] = new SelectList(_context.Contrats
                .Include(c => c.Reservation).ThenInclude(r => r.Machine)
                .Select(c => new { c.Id, Libelle = c.Numero + " — " + c.Reservation.Machine.Nom }),
                "Id", "Libelle", etat.ContratId);
            ViewData["TechnicienId"] = new SelectList(_context.Techniciens
                .Select(t => new { t.Id, Libelle = t.Nom + " " + t.Prenom }),
                "Id", "Libelle", etat.TechnicienId);
            return View(etat);
        }

        // POST: EtatDesLieux/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Type,Date,Observations,NiveauCarburant,EstConforme,ContratId,TechnicienId")] EtatDesLieux etat,
            List<IFormFile> Photos)
        {
            if (id != etat.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(etat);
                    await _context.SaveChangesAsync();

                    // Ajouter nouvelles photos
                    if (Photos != null && Photos.Any())
                    {
                        var dossier = Path.Combine(_env.WebRootPath, "photos-etats-lieux");
                        Directory.CreateDirectory(dossier);

                        foreach (var photo in Photos)
                        {
                            if (photo.Length > 0)
                            {
                                var nomFichier = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                                var chemin = Path.Combine(dossier, nomFichier);

                                using (var stream = new FileStream(chemin, FileMode.Create))
                                    await photo.CopyToAsync(stream);

                                _context.PhotosEtatLieux.Add(new PhotoEtatLieux
                                {
                                    EtatDesLieuxId = etat.Id,
                                    CheminFichier = $"/photos-etats-lieux/{nomFichier}",
                                    Description = photo.FileName
                                });
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EtatDesLieuxExists(etat.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ContratId"] = new SelectList(_context.Contrats
                .Include(c => c.Reservation).ThenInclude(r => r.Machine)
                .Select(c => new { c.Id, Libelle = c.Numero + " — " + c.Reservation.Machine.Nom }),
                "Id", "Libelle", etat.ContratId);
            ViewData["TechnicienId"] = new SelectList(_context.Techniciens
                .Select(t => new { t.Id, Libelle = t.Nom + " " + t.Prenom }),
                "Id", "Libelle", etat.TechnicienId);
            return View(etat);
        }

        // GET: EtatDesLieux/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var etat = await _context.EtatsDesLieux
                .Include(e => e.Contrat)
                .Include(e => e.Technicien)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (etat == null) return NotFound();
            return View(etat);
        }

        // POST: EtatDesLieux/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var etat = await _context.EtatsDesLieux
                .Include(e => e.Photos)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (etat != null)
            {
                // Supprimer les fichiers photos
                foreach (var photo in etat.Photos ?? new List<PhotoEtatLieux>())
                {
                    var chemin = Path.Combine(_env.WebRootPath, photo.CheminFichier.TrimStart('/'));
                    if (System.IO.File.Exists(chemin))
                        System.IO.File.Delete(chemin);
                }
                _context.EtatsDesLieux.Remove(etat);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EtatDesLieuxExists(int id)
        {
            return _context.EtatsDesLieux.Any(e => e.Id == id);
        }
    }
}