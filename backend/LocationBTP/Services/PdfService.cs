using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LocationBTP.Models.Entities;

namespace LocationBTP.Services
{
    public class PdfService
    {
        public byte[] GenererContrat(Contrat contrat)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var reservation = contrat.Reservation;
            var client = reservation?.Client;
            var machine = reservation?.Machine;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("LocationBTP").FontSize(24).Bold();
                            col.Item().Text("Location de machines de chantier").FontSize(11).FontColor("#666666");
                        });
                        row.ConstantItem(150).AlignRight().Column(col =>
                        {
                            col.Item().Text("CONTRAT").FontSize(18).Bold();
                            col.Item().Text($"N° {contrat.Numero}").FontSize(11);
                            col.Item().Text($"Date : {contrat.DateSignature:dd/MM/yyyy}").FontSize(11);
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().BorderBottom(2).BorderColor("#000000").PaddingBottom(10).Text("");

                        col.Item().PaddingTop(20).Text("INFORMATIONS CLIENT").Bold().FontSize(13);
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Nom complet").Bold();
                            table.Cell().Padding(5).Text($"{client?.Nom} {client?.Prenom}");
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Email").Bold();
                            table.Cell().Padding(5).Text(client?.Email ?? "-");
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Téléphone").Bold();
                            table.Cell().Padding(5).Text(client?.Telephone ?? "-");
                        });

                        col.Item().PaddingTop(20).Text("MACHINE LOUÉE").Bold().FontSize(13);
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Machine").Bold();
                            table.Cell().Padding(5).Text(machine?.Nom ?? "-");
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Marque / Modèle").Bold();
                            table.Cell().Padding(5).Text($"{machine?.Marque} — {machine?.Modele}");
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Référence").Bold();
                            table.Cell().Padding(5).Text(machine?.Reference ?? "-");
                        });

                        col.Item().PaddingTop(20).Text("DÉTAILS DE LA LOCATION").Bold().FontSize(13);
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Date début").Bold();
                            table.Cell().Padding(5).Text(reservation?.DateDebut.ToString("dd/MM/yyyy") ?? "-");
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Date fin").Bold();
                            table.Cell().Padding(5).Text(reservation?.DateFin.ToString("dd/MM/yyyy") ?? "-");
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Durée").Bold();
                            table.Cell().Padding(5).Text($"{reservation?.CalculerDuree()} jours");
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Tarif journalier").Bold();
                            table.Cell().Padding(5).Text($"{machine?.TarifJournalier:N2} DH");
                        });

                        col.Item().PaddingTop(20).Text("RÉCAPITULATIF FINANCIER").Bold().FontSize(13);
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(150); });
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Montant HT").Bold();
                            table.Cell().Padding(5).AlignRight().Text($"{contrat.MontantHT:N2} DH");
                            table.Cell().Padding(5).Background("#f5f5f5").Text("TVA (20%)").Bold();
                            table.Cell().Padding(5).AlignRight().Text($"{contrat.TVA:N2} DH");
                            table.Cell().Padding(5).Background("#000000").Text("TOTAL TTC").Bold().FontColor("#ffffff");
                            table.Cell().Padding(5).AlignRight().Background("#000000").Text($"{contrat.MontantTotal:N2} DH").Bold().FontColor("#ffffff");
                            table.Cell().Padding(5).Background("#f5f5f5").Text("Caution").Bold();
                            table.Cell().Padding(5).AlignRight().Text($"{machine?.MontantCaution:N2} DH");
                        });

                        col.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Signature Client").Bold();
                                c.Item().PaddingTop(40).BorderBottom(1).BorderColor("#000000").Text("");
                            });
                            row.ConstantItem(50);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Signature LocationBTP").Bold();
                                c.Item().PaddingTop(40).BorderBottom(1).BorderColor("#000000").Text("");
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("LocationBTP — Contrat généré le ").FontColor("#666666");
                        text.Span(DateTime.Now.ToString("dd/MM/yyyy à HH:mm")).FontColor("#666666");
                    });
                });
            }).GeneratePdf();
        }
    }
}