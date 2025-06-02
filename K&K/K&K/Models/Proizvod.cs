using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class Proizvod
    {
        [Key]
        public int Id { get; set; }
        public String Naziv { get; set; }
        public String Opis { get; set; }
        public String Slika { get; set; }
        public double Cijena { get; set; }
    }
}