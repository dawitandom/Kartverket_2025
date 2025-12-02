namespace FirstWebApplication.Models
{
    /// <summary>
    /// ViewModel for feilsiden i applikasjonen (Error.cshtml).
    /// Brukes til å vise feilmeldinger og en unik request-ID ved tekniske feil eller unntak.
    /// Lar brukere og administratorer spore feil i logger ved hjelp av request-ID-en.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Unik identifikator for HTTP-requesten som feilet eller forårsaket en feil.
        /// Genereres automatisk av ASP.NET Core når en feil oppstår.
        /// Brukes til å spore og finne feilen i logger og feilsøking.
        /// Kan være null hvis request-ID ikke er tilgjengelig.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Beregnet egenskap som indikerer om RequestId skal vises på feilsiden.
        /// Returnerer true hvis RequestId har en verdi, ellers false.
        /// Brukes i feilsiden for å bestemme om request-ID skal vises til brukeren.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}