using System.ComponentModel.DataAnnotations;

namespace LocationBTP.Models.Enums
{
    public enum StatutMachine
    {
        [Display(Name = "Disponible")]
        Disponible,

        [Display(Name = "Réservée")]
        Reservee,

        [Display(Name = "En maintenance")]
        EnMaintenance,

        [Display(Name = "Hors service")]
        HorsService
    }

    public enum StatutReservation
    {
        EnAttente,
        Confirmee,
        EnCours,
        Terminee,
        Annulee
    }

    public enum StatutContrat
    {
        Brouillon,
        Signe,
        EnCours,
        Termine,
        Annule
    }

    public enum StatutCaution
    {
        EnAttente,
        Versee,
        ARemborser,
        Remboursee,
        Retenue
    }

    public enum TypeEtatDesLieux
    {
        Depart,
        Retour
    }

    public enum TypeClient
    {
        Particulier,
        Entreprise
    }
}