using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class Osoba
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public String Ime { get; set; }
        
        [Required]
        public String Prezime { get; set; }

        [Required]
        public String Email { get; set; }
        
        [Required] 
        [RegularExpression(@"[0-9| |a-z|A-Z|\.]+", ErrorMessage = "Korisnicko ime smije sadržavati samo slova, brojeve, razmake i ta?ku!")]
        public String KorisnickoIme { get; set; }
        
        [Required]
        [StringLength(maximumLength: 30, MinimumLength = 5, ErrorMessage = "Lozinka mora imati najmanje 5 znakova, a najviše 30")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)$", ErrorMessage = "Lozinka mora sadržavati najmanje jedno veliko slovo, mala slova i broj!")]
        public String Lozinka { get; set; }
        public Uloga Uloga { get; set; }

    }
}