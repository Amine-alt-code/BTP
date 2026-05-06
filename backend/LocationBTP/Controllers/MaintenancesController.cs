using LocationBTP.Data;
using LocationBTP.Models.Entities;
using LocationBTP.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocationBTP.Controllers
{
    public class MaintenancesController : Controller
    {
        private readonly AppDbContext _context;

        public MaintenancesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Maintenances
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Maintenances.Include(m => m.Machine).Include(m => m.Technicien);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Maintenances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenance = await _context.Maintenances
                .Include(m => m.Machine)
                .Include(m => m.Technicien)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (maintenance == null)
            {
                return NotFound();
            }

            return View(maintenance);
        }

        // GET: Maintenances/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Nom");
            ViewData["TechnicienId"] = new SelectList(_context.Techniciens, "Id", "Email");
            return View();
        }

        // POST: Maintenances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,DateDebut,Cout,Description,Statut,MachineId,TechnicienId")] Maintenance maintenance)
        {
            // Forcer le statut EnCours
            maintenance.Statut = "EnCours";
            // DateFin optionnelle
            maintenance.DateFin = null;

            if (ModelState.IsValid)
            {
                _context.Add(maintenance);

                // Mettre la machine en EnMaintenance automatiquement
                var machine = await _context.Machines.FindAsync(maintenance.MachineId);
                if (machine != null)
                {
                    machine.Statut = StatutMachine.EnMaintenance;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Nom", maintenance.MachineId);
            ViewData["TechnicienId"] = new SelectList(_context.Techniciens, "Id", "Email", maintenance.TechnicienId);
            return View(maintenance);
        }

        // GET: Maintenances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance == null)
            {
                return NotFound();
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Nom", maintenance.MachineId);
            ViewData["TechnicienId"] = new SelectList(_context.Techniciens, "Id", "Email", maintenance.TechnicienId);
            return View(maintenance);
        }

        // POST: Maintenances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,DateDebut,DateFin,Cout,Description,Statut,MachineId,TechnicienId")] Maintenance maintenance)
        {
            if (id != maintenance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(maintenance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceExists(maintenance.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Nom", maintenance.MachineId);
            ViewData["TechnicienId"] = new SelectList(_context.Techniciens, "Id", "Email", maintenance.TechnicienId);
            return View(maintenance);
        }

        // GET: Maintenances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenance = await _context.Maintenances
                .Include(m => m.Machine)
                .Include(m => m.Technicien)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (maintenance == null)
            {
                return NotFound();
            }

            return View(maintenance);
        }

        // POST: Maintenances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance != null)
            {
                _context.Maintenances.Remove(maintenance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaintenanceExists(int id)
        {
            return _context.Maintenances.Any(e => e.Id == id);
        }

        // GET: Maintenances/Terminer/5
        public async Task<IActionResult> Terminer(int id)
        {
            var maintenance = await _context.Maintenances
                .Include(m => m.Machine)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (maintenance == null) return NotFound();

            // Terminer la maintenance
            maintenance.Terminer();

            // Remettre la machine en Disponible
            if (maintenance.Machine != null)
            {
                maintenance.Machine.Statut = StatutMachine.Disponible;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
