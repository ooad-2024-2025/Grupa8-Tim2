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

        public Velicina Velicina { get; set; }

        [Required]
        [StringLength(maximumLength: 30, MinimumLength = 3, ErrorMessage = "Naziv mora imati 3 karaktera.")]
        [RegularExpression(@"[A-Z| |a-z|]+", ErrorMessage = "Naziv smije sadržavati samo slova i razmake!")]
        public String Naziv { get; set; }

        [Required]
        public String Opis { get; set; }

        [Required]
        public String Slika { get; set; }

        [Required]
        [Range(0.0, 100.0, ErrorMessage = "Cijena mora biti validna!")]
        public double Cijena { get; set; }
    }
}