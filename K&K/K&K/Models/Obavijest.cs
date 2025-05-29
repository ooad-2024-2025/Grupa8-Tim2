using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class Obavijest
    {
        public int id { get; set; }
        public int korisnikId { get; set; }
        public int statusNarudzbeId { get; set; }
        public Osoba korisnik { get; set; }
        public String sadrzaj { get; set; }
        public DateTime datum { get; set; }
        public StatusNarudzbe statusNarudzbe { get; set; }
    }
}