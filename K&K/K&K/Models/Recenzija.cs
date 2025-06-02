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
        public int Id { get; set; }

        [ForeignKey("Proizvod")]
        public int ProizvodId { get; set; }

        [ForeignKey("Osoba")]
        public int KorisnikId { get; set; }

        [ForeignKey("Narudzba")]
        public int NarudzbaId { get; set; }
        public int Ocjena { get; set; }
        public Proizvod Proizvod { get; set; }
        public Osoba Korisnik { get; set; }
        public Narudzba Narudzba { get; set; }

    }
}