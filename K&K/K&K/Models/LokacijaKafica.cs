using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class LokacijaKafica
    {
        [Key]
        public int id { get; set; }
        public String adresa { get; set; }
        public String grad { get; set; }
        public double geografskaSirina { get; set; }
        public double geografskaDuzina { get; set; }
    }
}