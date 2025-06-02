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
        public String Ime { get; set; }
        public String Prezime { get; set; }
        public String Email { get; set; }
        public String KorisnickoIme { get; set; }
        public String Lozinka { get; set; }
        public Uloga Uloga { get; set; }

    }
}