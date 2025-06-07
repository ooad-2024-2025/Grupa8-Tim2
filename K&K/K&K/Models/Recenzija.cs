using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_K.Models
{
    public class Recenzija
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Proizvod")]
        public int ProizvodId { get; set; }

        [ForeignKey("Osoba")]
        public int KorisnikId { get; set; }

        [ForeignKey("Narudzba")]
        public int NarudzbaId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Ocjena mora biti izme?u 1 i 5.")]
        public int Ocjena { get; set; }

        [Required]
        [StringLength(1000)]
        public string Tekst { get; set; }

        public DateTime DatumDodavanja { get; set; } = DateTime.Now;

        public Proizvod Proizvod { get; set; }
        public Osoba Korisnik { get; set; }
        public Narudzba Narudzba { get; set; }
    }
}
