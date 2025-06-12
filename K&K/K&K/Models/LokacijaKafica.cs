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
        public int Id { get; set; }
        public String Adresa { get; set; }
        public String Grad { get; set; }

        [Display(Name = "Geografska širina")]
        public double GeografskaSirina { get; set; }

        [Display(Name = "Geografska dužina")]
        public double GeografskaDuzina { get; set; }
    }
}