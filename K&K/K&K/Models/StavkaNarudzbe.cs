using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class StavkaNarudzbe
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("Proizvod")]
        public int proizvodId { get; set; }

        [ForeignKey("Narudzba")]
        public int narudzbaId { get; set; }
        public int kolicina { get; set; }
        public Proizvod proizvod { get; set; }
        public Double cijena { get; set; }
        public Narudzba narudzba { get; set; }

    }
}