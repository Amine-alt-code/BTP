using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LocationBTP.Data;
using LocationBTP.Models.Entities;

namespace LocationBTP.Controllers
{
    public class TechniciensController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TechniciensController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Techniciens
        public async Task<IActionResult> Index()
        {
            return View(await _context.Techniciens.ToListAsync());
        }

        // GET: Techniciens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var technicien = await _context.Techniciens
                .FirstOrDefaultAsync(m => m.Id == id);
            if (technicien == null) return NotFound();

            return View(technicien);
        }

        // GET: Techniciens/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Techniciens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Prenom,Email,Telephone,Login,PasswordHash")] Technicien technicien)
        {
            if (ModelState.IsValid)
            {
                // 1. Créer le compte Identity
                var user = new ApplicationUser
                {
                    UserName = technicien.Email,
                    Email = technicien.Email,
                    Nom = technicien.Nom,
                    Prenom = technicien.Prenom,
                    EmailConfirmed = true
                };

                // Le mot de passe saisi dans PasswordHash est utilisé en clair
                var result = await _userManager.CreateAsync(user, technicien.PasswordHash);

                if (result.Succeeded)
                {
                    // 2. Créer le rôle Technicien s'il n'existe pas
                    if (!await _roleManager.RoleExistsAsync("Technicien"))
                        await _roleManager.CreateAsync(new IdentityRole("Technicien"));

                    // 3. Assigner le rôle
                    await _userManager.AddToRoleAsync(user, "Technicien");

                    // 4. Hasher le mot de passe pour la table Techniciens
                    technicien.PasswordHash = _userManager.PasswordHasher
                        .HashPassword(user, technicien.PasswordHash);

                    // 5. Sauvegarder dans la table Techniciens
                    _context.Add(technicien);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                // Afficher les erreurs Identity
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(technicien);
        }

        // GET: Techniciens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var technicien = await _context.Techniciens.FindAsync(id);
            if (technicien == null) return NotFound();

            return View(technicien);
        }

        // POST: Techniciens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,Email,Telephone,Login,PasswordHash")] Technicien technicien)
        {
            if (id != technicien.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Mettre à jour aussi le compte Identity
                    var user = await _userManager.FindByEmailAsync(technicien.Email);
                    if (user != null)
                    {
                        user.Nom = technicien.Nom;
                        user.Prenom = technicien.Prenom;
                        await _userManager.UpdateAsync(user);
                    }

                    _context.Update(technicien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TechnicienExists(technicien.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(technicien);
        }

        // GET: Techniciens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var technicien = await _context.Techniciens
                .FirstOrDefaultAsync(m => m.Id == id);
            if (technicien == null) return NotFound();

            return View(technicien);
        }

        // POST: Techniciens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var technicien = await _context.Techniciens.FindAsync(id);
            if (technicien != null)
            {
                // Supprimer aussi le compte Identity
                var user = await _userManager.FindByEmailAsync(technicien.Email);
                if (user != null)
                    await _userManager.DeleteAsync(user);

                _context.Techniciens.Remove(technicien);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TechnicienExists(int id)
        {
            return _context.Techniciens.Any(e => e.Id == id);
        }
    }
}