using System;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    public class Report // Klassen Report brukes til å lagre en rapport i systemet
    {
        // Primærnøkkel (unik id for hver rapport)
        public int Id { get; set; } 
           
        // Tidspunkt rapporten ble laget
        public DateTime CreatedAtUtc { get; set; }

        // Id for brukeren som har sendt inn rapporten
        [Required] 
        public int UserId { get; set; } 

        //Brukernavn må fylles ut, maks 100 tegn
        [Required, StringLength(100)] 
        public string Username { get; set; }

        // Selve meldingen/innholdet i rapporten, må fylles ut, maks 500 tegn
        [Required, StringLength(500)]
        public string Message { get; set; }

        //Valgfri lengdegrad (gps-posisjon)
        public double? Latitude { get; set; }

        //Valgfri breddegrad (gps-posisjon)
        public double? Longitude { get; set; }

        // Høyde i fot, må fylles ut, mellom 1 og 100000 fot
        [Required(ErrorMessage = "Altitude (feet) is required.")]
        [Range(1, 100000, ErrorMessage = "Altitude must be between {1} and {2} feet.")]
        public int? AltitudeFeet { get; set; }

        //type hindring , må fylles ut, kan ikke være ugyldig
        [Required(ErrorMessage = "Obstacle type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid obstacle type.")]
        public ObstacleType Type { get; set; }
    }
}