using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace K_K.Models
{
    public class Osoba : IdentityUser
    {
        [Required]
        public String Ime { get; set; }
        
        [Required]
        public String Prezime { get; set; }
        public string? Adresa { get; set; }
        public Uloga Uloga { get; set; }
        public string PunoIme => $"{Ime} {Prezime}";

    }
}