namespace FirstWebApplication.Models
{
    public class AdviceDto //Gir default verdier (advice = 0, string = null, description null)
    {
        public int AdviceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

    }
}
