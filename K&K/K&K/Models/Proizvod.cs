using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class Proizvod
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Morate izabrati velicinu!")]
        public Velicina Velicina { get; set; }

        [Required(ErrorMessage = "Naziv proizvoda je obavezan!")]
        [StringLength(maximumLength: 30, MinimumLength = 3, ErrorMessage = "Naziv mora imati 3 karaktera.")]
        [RegularExpression(@"[A-Z| |a-z|]+", ErrorMessage = "Naziv smije sadržavati samo slova i razmake!")]
        public String Naziv { get; set; }

        [Required(ErrorMessage = "Opis proizvoda je obavezan!")]
        public String Opis { get; set; }

        [Required(ErrorMessage = "Slika proizvoda je obavezna!")]
        public String Slika { get; set; }

        [Required(ErrorMessage = "Cijena proizvoda je obavezna!")]
        [Range(0.0, 100.0, ErrorMessage = "Cijena mora biti validna!")]
        public double Cijena { get; set; }
        
    }
}