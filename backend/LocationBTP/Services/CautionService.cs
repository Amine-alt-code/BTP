using LocationBTP.Data;
using LocationBTP.Models.Entities;
using LocationBTP.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LocationBTP.Services
{
    public class CautionService
    {
        private readonly AppDbContext _context;

        public CautionService(AppDbContext context)
        {
            _context = context;
        }

        // Payer une caution
        public async Task PayerCaution(int cautionId, string modePaiement)
        {
            var caution = await _context.Cautions.FindAsync(cautionId);
            if (caution == null)
                throw new Exception("Caution introuvable !");

            caution.Statut = StatutCaution.Versee;
            caution.DateVersement = DateTime.Now;
            caution.ModePaiement = modePaiement;

            await _context.SaveChangesAsync();
        }

        // Rembourser une caution
        public async Task RembourserCaution(int cautionId)
        {
            var caution = await _context.Cautions.FindAsync(cautionId);
            if (caution == null)
                throw new Exception("Caution introuvable !");

            caution.Statut = StatutCaution.Remboursee;
            caution.DateRemboursement = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        // Retenir une caution
        public async Task RetenirCaution(int cautionId, string notes)
        {
            var caution = await _context.Cautions.FindAsync(cautionId);
            if (caution == null)
                throw new Exception("Caution introuvable !");

            caution.Statut = StatutCaution.Retenue;
            caution.Notes = notes;

            await _context.SaveChangesAsync();
        }

        // Obtenir toutes les cautions
        public async Task<List<Caution>> GetCautions()
        {
            return await _context.Cautions
                .Include(c => c.Contrat)
                    .ThenInclude(c => c.Reservation)
                        .ThenInclude(r => r.Client)
                .ToListAsync();
        }
    }
}