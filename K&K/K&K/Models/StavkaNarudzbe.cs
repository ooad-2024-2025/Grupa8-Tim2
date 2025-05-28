using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class StavkaNarudzbe
    {
        public int id { get; set; }
        public int proizvodId { get; set; }
        public int narudzbaId { get; set; }
        public int kolicina { get; set; }
        public Proizvod proizvod { get; set; }
        public Double cijena { get; set; }
        public Narudzba narudzba { get; set; }

    }
}