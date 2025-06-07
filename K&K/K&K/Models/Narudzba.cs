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

        [ForeignKey(nameof(Korisnik))]
        public String KorisnikId { get; set; }


        [ForeignKey(nameof(Radnik))]
        public String RadnikId { get; set; }
        public Osoba Korisnik { get; set; }
        public Osoba Radnik { get; set; }
        public StatusNarudzbe StatusNarudzbe { get; set; }
         [EnumDataType(typeof(VrstaPlacanja))] 
        public VrstaPlacanja NacinPlacanja { get; set; }

        [EnumDataType(typeof(VrstaPreuzimanja))]
        public VrstaPreuzimanja NacinPreuzimanja { get; set; }
        public DateTime DatumNarudzbe { get; set; }
        public String? AdresaDostave { get; set; }

    }
}