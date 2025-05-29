using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class Obavijest
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("Osoba")]
        public int korisnikId { get; set; }
        public int statusNarudzbeId { get; set; }
        public Osoba korisnik { get; set; }
        public String sadrzaj { get; set; }
        public DateTime datum { get; set; }
        public StatusNarudzbe statusNarudzbe { get; set; }
    }
}