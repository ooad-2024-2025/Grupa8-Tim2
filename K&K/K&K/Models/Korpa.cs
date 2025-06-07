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
        
        public int brojProizvoda { get; set; }
        public double ukupnaCijena { get; set; }
<<<<<<< HEAD

=======
>>>>>>> 9025f26dd545d127d7157d78934cb622ab279438
        public ICollection<StavkaKorpe> Stavke { get; set; }
        public Korpa() { }
    }
}
