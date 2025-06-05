using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_K.Models
{
    public class StavkaKorpe
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Korpa")]
        public int KorpaId { get; set; }

        [ForeignKey("Proizvod")]
        public int ProizvodId { get; set; }
        public int Kolicina { get; set; }
        public double Cijena { get; set; }

        public Korpa Korpa { get; set; }
        public Proizvod Proizvod { get; set; }
    }
}
