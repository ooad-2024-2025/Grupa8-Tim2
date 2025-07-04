﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_K.Models
{
    public class Korpa
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Korisnik")]
        public String? KorisnikId { get; set; }
        public Osoba? Korisnik { get; set; }
        
        public int brojProizvoda { get; set; }
        public double ukupnaCijena { get; set; }

        public ICollection<StavkaKorpe> Stavke { get; set; }
        public Korpa() { }
    }
}
