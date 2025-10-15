namespace FirstWebApplication.Models
{
    /// <summary>
    /// View model for feilsiden (Error.cshtml).
    /// Brukes til å vise feilmeldinger og request ID ved tekniske feil.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Unik ID for HTTP-requesten som feilet.
        /// Brukes til å spore feil i logger.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indikerer om RequestId skal vises på feilsiden.
        /// Returnerer true hvis RequestId har en verdi.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}