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
        public int Id { get; set; }

        [ForeignKey("Narudzba")]
        public int NarudzbaId { get; set; }
        public Narudzba Narudzba { get; set; }
        public String ImeNaKartici { get; set; }
        public String BrojKartice { get; set; }
        public String CVV { get; set; }
        public DateTime DatumIsteka { get; set; }
        public DateTime VrijemePlacanja { get; set; }
        public bool Uspjesno { get; set; }
    }
}