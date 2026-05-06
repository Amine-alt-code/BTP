using LocationBTP.Data;
using LocationBTP.Models.Entities;
using LocationBTP.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LocationBTP.Services
{
    public class ContratService
    {
        private readonly AppDbContext _context;
        private readonly PdfService _pdfService;
        private readonly EmailService _emailService;

        public ContratService(AppDbContext context, PdfService pdfService, EmailService emailService)
        {
            _context = context;
            _pdfService = pdfService;
            _emailService = emailService;
        }

        public async Task<Contrat> GenererContrat(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Machine)
                .Include(r => r.Client)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                throw new Exception("Réservation introuvable !");

            int duree = (reservation.DateFin - reservation.DateDebut).Days;
            decimal montantHT = duree * reservation.Machine.TarifJournalier;
            decimal tva = montantHT * 0.20m;
            decimal montantTotal = montantHT + tva;

            var contrat = new Contrat
            {
                ReservationId = reservationId,
                DateSignature = DateTime.Now,
                MontantHT = montantHT,
                TVA = tva,
                MontantTotal = montantTotal,
                Statut = StatutContrat.Signe,
                Numero = $"CTR-{DateTime.Now.Year}-{reservationId:D4}"
            };

            var caution = new Caution
            {
                Montant = reservation.Machine.MontantCaution,
                Statut = StatutCaution.EnAttente
            };

            contrat.Caution = caution;
            _context.Contrats.Add(contrat);
            await _context.SaveChangesAsync();

            reservation.Statut = StatutReservation.Confirmee;
            await _context.SaveChangesAsync();

            // Recharger avec toutes les relations
            var contratComplet = await _context.Contrats
                .Include(c => c.Reservation).ThenInclude(r => r.Client)
                .Include(c => c.Reservation).ThenInclude(r => r.Machine)
                .Include(c => c.Caution)
                .FirstOrDefaultAsync(c => c.Id == contrat.Id);

            // Générer et sauvegarder le PDF
            var pdfBytes = _pdfService.GenererContrat(contratComplet);
            var dossier = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contrats");
            Directory.CreateDirectory(dossier);
            var nomFichier = $"contrat_{contrat.Numero}.pdf";
            await File.WriteAllBytesAsync(Path.Combine(dossier, nomFichier), pdfBytes);
            contratComplet.FichierPDF = $"/contrats/{nomFichier}";
            await _context.SaveChangesAsync();

            // Envoyer email au client
            var corpsEmail = $@"
                <div style='font-family:Arial;max-width:600px;margin:auto;'>
                    <div style='background:#000;color:#fff;padding:20px;'>
                        <h2>LocationBTP</h2>
                    </div>
                    <div style='padding:20px;'>
                        <h3>Votre réservation est confirmée !</h3>
                        <p>Bonjour <strong>{reservation.Client.Prenom} {reservation.Client.Nom}</strong>,</p>
                        <p>Votre réservation a été confirmée. Veuillez trouver ci-joint votre contrat.</p>
                        <table style='width:100%;border-collapse:collapse;'>
                            <tr><td style='padding:8px;background:#f5f5f5;'><strong>Machine</strong></td>
                                <td style='padding:8px;'>{reservation.Machine.Nom}</td></tr>
                            <tr><td style='padding:8px;background:#f5f5f5;'><strong>Du</strong></td>
                                <td style='padding:8px;'>{reservation.DateDebut:dd/MM/yyyy}</td></tr>
                            <tr><td style='padding:8px;background:#f5f5f5;'><strong>Au</strong></td>
                                <td style='padding:8px;'>{reservation.DateFin:dd/MM/yyyy}</td></tr>
                            <tr><td style='padding:8px;background:#f5f5f5;'><strong>Montant total</strong></td>
                                <td style='padding:8px;'><strong>{montantTotal:N2} DH</strong></td></tr>
                            <tr><td style='padding:8px;background:#f5f5f5;'><strong>Caution à verser</strong></td>
                                <td style='padding:8px;color:red;'><strong>{reservation.Machine.MontantCaution:N2} DH</strong></td></tr>
                        </table>
                        <p style='margin-top:20px;'>Merci de verser la caution pour finaliser votre location.</p>
                        <p>Cordialement,<br/><strong>L'équipe LocationBTP</strong></p>
                    </div>
                </div>";

            await _emailService.EnvoyerAsync(
                reservation.Client.Email,
                $"Confirmation réservation — {reservation.Machine.Nom}",
                corpsEmail,
                pdfBytes,
                nomFichier
            );

            return contratComplet;
        }

        public async Task SignerContrat(int contratId)
        {
            var contrat = await _context.Contrats.FindAsync(contratId);
            if (contrat == null) throw new Exception("Contrat introuvable !");
            contrat.Statut = StatutContrat.Signe;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Contrat>> GetContrats()
        {
            return await _context.Contrats
                .Include(c => c.Reservation).ThenInclude(r => r.Client)
                .Include(c => c.Reservation).ThenInclude(r => r.Machine)
                .Include(c => c.Caution)
                .ToListAsync();
        }
    }
}