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
        public int Id { get; set; }

        [ForeignKey("Osoba")]
        public String KorisnikId { get; set; }
        public int StatusNarudzbeId { get; set; }
        public Osoba Korisnik { get; set; }
        public String Sadrzaj { get; set; }
        public DateTime Datum { get; set; }
        public StatusNarudzbe StatusNarudzbe { get; set; }
    }
}