using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models

{
    public class Narudzba
    {
        public int id { get; set; }
        public int korisnikId { get; set; }
        public int radnikId { get; set; }
        public int statusNarudzbeId { get; set; }
        public int karticnoPlacanjeId { get; set; }
        public Osoba korisnik { get; set; }
        public Osoba radnik { get; set; }
        public StatusNarudzbe statusNarudzbe { get; set; }
        public VrstaPlacanja nacinPlacanja { get; set; }
        public VrstaPreuzimanja nacinPreuzimanja { get; set; }
        public DateTime datumNarudzbe { get; set; }
        public String? adresaDostave { get; set; }
        public KarticnoPlacanje? karticnoPlacanje { get; set; }

    }
}