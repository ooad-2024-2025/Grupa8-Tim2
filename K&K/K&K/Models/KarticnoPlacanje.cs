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

        [Required]
        public String ImeNaKartici { get; set; }

        [Required]
        public String BrojKartice { get; set; }

        [Required]
        [StringLength(maximumLength: 3, MinimumLength = 3, ErrorMessage = "CVV mora sadržavati 3 cifre!")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV mora sadržavati tačno 3 cifre!")]

        public String CVV { get; set; }

        public DateTime DatumIsteka { get; set; }

        public DateTime VrijemePlacanja { get; set; }

        public bool Uspjesno { get; set; }
    }
}