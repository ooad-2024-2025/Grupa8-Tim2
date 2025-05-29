using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class Hrana
    {
        [Key]
        public int Id { get; set; }
        public Velicina velicina { get; set; }
        public VrstaHrane vrsta { get; set; }

    }
}