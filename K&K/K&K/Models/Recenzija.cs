using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class Recenzija
    {
        public int id { get; set; }
        public int proizvodId { get; set; }
        public int korisnikId { get; set; }
        public int narudzbaId { get; set; }
        public int ocjena { get; set; }
        public Proizvod proizvod { get; set; }
        public Osoba korisnik { get; set; }
        public Narudzba narudzba { get; set; }

    }
}