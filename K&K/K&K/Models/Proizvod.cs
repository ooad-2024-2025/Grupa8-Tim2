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
        public int id { get; set; }
        public String naziv { get; set; }
        public String opis { get; set; }
        public String slika { get; set; }
        double cijena { get; set; }
    }
}