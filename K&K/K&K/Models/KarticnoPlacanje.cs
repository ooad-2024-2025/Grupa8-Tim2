using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Models
{
    public class KarticnoPlacanje
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("Narudzba")]
        public int narudzbaId { get; set; }
        public Narudzba narudzba { get; set; }
        public String imeNaKartici { get; set; }
        public String brojKartice { get; set; }
        public String CVV { get; set; }
        public DateTime datumIsteka { get; set; }
        public DateTime vrijemePlacanja { get; set; }
        public bool uspjesno { get; set; }
    }
}