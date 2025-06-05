using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class Hrana : Proizvod
    {
        [Required]
        public VrstaHrane VrstaHrane { get; set; }

    }
}