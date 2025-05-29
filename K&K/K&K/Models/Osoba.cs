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
        public int id { get; set; }
        public String ime { get; set; }
        public String prezime { get; set; }
        public String email { get; set; }
        public String korisnickoIme { get; set; }
        public String lozinka { get; set; }
        Uloga uloga { get; set; }

    }
}