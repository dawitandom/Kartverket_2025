namespace FirstWebApplication.Models
{
    public class ErrorViewModel //View model for error siden (Error.cshmtl)
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
