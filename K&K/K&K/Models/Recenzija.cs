using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace K_K.Models
{
    public class Recenzija
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Proizvod")]
        public int ProizvodId { get; set; } 

        [ForeignKey(nameof(Korisnik))]
        public String KorisnikId { get; set; } 

        [ForeignKey("Narudzba")]
        public int NarudzbaId { get; set; } // Ovdje ne treba [Required] jer je int i nije nullable.

        [Required]
        [Range(1, 5, ErrorMessage = "Ocjena mora biti izme?u 1 i 5.")]
        public int Ocjena { get; set; }

        [Required]
        [StringLength(1000)]
        public string Tekst { get; set; }

        public DateTime DatumDodavanja { get; set; } = DateTime.Now;

        // NEMA [Required] na navigacijskim svojstvima!

        //ovo moze biti VELIKA GRESKA
        [ValidateNever] // Dodajte ovo!
        public Proizvod Proizvod { get; set; }

        [ValidateNever] // Dodajte ovo!
        public Osoba Korisnik { get; set; } // Pretpostavljam da je Osoba vaš korisni?ki model

        [ValidateNever] // Dodajte ovo!
        public Narudzba Narudzba { get; set; }
    }
}