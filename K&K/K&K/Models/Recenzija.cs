using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class Recenzija
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("Proizvod")]
        public int proizvodId { get; set; }

        [ForeignKey("Osoba")]
        public int korisnikId { get; set; }

        [ForeignKey("Narudzba")]
        public int narudzbaId { get; set; }
        public int ocjena { get; set; }
        public Proizvod proizvod { get; set; }
        public Osoba korisnik { get; set; }
        public Narudzba narudzba { get; set; }

    }
}