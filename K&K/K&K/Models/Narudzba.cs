using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models

{
    public class Narudzba
    {

        [Key]
        public int id { get; set; }

        [ForeignKey("Osoba")]
        public int korisnikId { get; set; }

        [ForeignKey("Osoba")]
        public int radnikId { get; set; }

        [ForeignKey("KarticnoPlacanje")]
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