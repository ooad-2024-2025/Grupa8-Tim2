using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K_K.Models
{
    public class Korpa
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Korisnik")]
        public int KorisnikId { get; set; }
        public Osoba Korisnik { get; set; }
        
        public int Kolicina { get; set; }
        public double Cijena { get; set; }
        public Boolean Kupljeno { get; set; }

        public ICollection<StavkaKorpe> Stavke { get; set; }
        public Korpa() { }
    }
}
