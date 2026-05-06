using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LocationBTP.Data;
using LocationBTP.Models.Entities;

namespace LocationBTP.Controllers
{
    public class CautionsController : Controller
    {
        private readonly AppDbContext _context;

        public CautionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cautions
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Cautions.Include(c => c.Contrat);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Cautions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caution = await _context.Cautions
                .Include(c => c.Contrat)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (caution == null)
            {
                return NotFound();
            }

            return View(caution);
        }

        // GET: Cautions/Create
        public IActionResult Create()
        {
            ViewData["ContratId"] = new SelectList(_context.Contrats, "Id", "Numero");
            return View();
        }

        // POST: Cautions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Montant,DateVersement,DateRemboursement,Statut,ModePaiement,Notes,ContratId")] Caution caution)
        {
            if (ModelState.IsValid)
            {
                _context.Add(caution);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ContratId"] = new SelectList(_context.Contrats, "Id", "Numero", caution.ContratId);
            return View(caution);
        }

        // GET: Cautions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caution = await _context.Cautions.FindAsync(id);
            if (caution == null)
            {
                return NotFound();
            }
            ViewData["ContratId"] = new SelectList(_context.Contrats, "Id", "Numero", caution.ContratId);
            return View(caution);
        }

        // POST: Cautions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Montant,DateVersement,DateRemboursement,Statut,ModePaiement,Notes,ContratId")] Caution caution)
        {
            if (id != caution.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(caution);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CautionExists(caution.Id))
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
            ViewData["ContratId"] = new SelectList(_context.Contrats, "Id", "Numero", caution.ContratId);
            return View(caution);
        }

        // GET: Cautions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caution = await _context.Cautions
                .Include(c => c.Contrat)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (caution == null)
            {
                return NotFound();
            }

            return View(caution);
        }

        // POST: Cautions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var caution = await _context.Cautions.FindAsync(id);
            if (caution != null)
            {
                _context.Cautions.Remove(caution);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CautionExists(int id)
        {
            return _context.Cautions.Any(e => e.Id == id);
        }
    }
}
