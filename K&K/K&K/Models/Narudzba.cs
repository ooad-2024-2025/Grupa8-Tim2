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
        public int Id { get; set; }

        [ForeignKey("Osoba")]
        public int KorisnikId { get; set; }

        [ForeignKey("Osoba")]
        public int RadnikId { get; set; }
        public Osoba Korisnik { get; set; }
        public Osoba Radnik { get; set; }
        public StatusNarudzbe StatusNarudzbe { get; set; }
        public VrstaPlacanja NacinPlacanja { get; set; }
        public VrstaPreuzimanja NacinPreuzimanja { get; set; }
        public DateTime DatumNarudzbe { get; set; }
        public String? AdresaDostave { get; set; }

    }
}