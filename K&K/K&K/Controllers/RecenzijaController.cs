// K_K/Controllers/RecenzijaController.cs

using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims; // Potrebno za dohvat User ID-a
using System.Threading.Tasks;

namespace K_K.Controllers
{
    public class RecenzijaController : Controller
    {
        private readonly ApplicationDbContext _dataContext;
        private readonly UserManager<IdentityUser> _userManager;

        public RecenzijaController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _dataContext = context;
            _userManager = userManager;
        }

        // GET: Recenzija/PrikaziRecenziju?proizvodId=X
        // Akcija za prikaz SVIH recenzija za jedan proizvod na posebnoj stranici (ako je imate)
        public async Task<IActionResult> PrikaziRecenziju(int proizvodId)
        {
            var proizvod = await _dataContext.Proizvod.FindAsync(proizvodId);
            if (proizvod == null)
            {
                return NotFound();
            }

            /*var recenzije = await _dataContext.Recenzija
                .Include(r => r.Korisnik)
                .Where(r => r.ProizvodId == proizvodId)
                .OrderByDescending(r => r.DatumDodavanja)
                .ToListAsync();*/ //nije ispravno dohvatalo iz baze
            var recenzije = await _dataContext.Recenzija
                          .Where(r => r.ProizvodId == proizvodId)
                           .ToListAsync();


            ViewBag.ProizvodNaziv = proizvod.Naziv; // Za prikaz naziva proizvoda na RecenzijeView
            ViewData["ProizvodId"] = proizvodId; // Za povratak na Details view
            return View("RecenzijeView", recenzije); // Pretpostavljam da imate RecenzijeView.cshtml
        }

        [HttpGet]
        public async Task<IActionResult> MojeRecenzije()
        {
            /*var recenzije = await _dataContext.Recenzija
                .Include(r => r.Korisnik)
                .Where(r => r.ProizvodId == proizvodId)
                .OrderByDescending(r => r.DatumDodavanja)
                .ToListAsync();*/
            var recenzije = await _dataContext.Recenzija
                .ToListAsync();

            return View(recenzije); // šaljemo listu u view
        }
        // GET: Recenzija/OstaviRecenziju?proizvodId=X&narudzbaId=Y
        // Akcija za prikaz forme za ostavljanje recenzije
        //int proizvodId, int narudzbaId
        public async Task<IActionResult> OstaviRecenziju(int proizvodId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                //return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("OstaviRecenziju", new { proizvodId = proizvodId, narudzbaId = narudzbaId }) });
                return Redirect("/Identity/Account/Register");
            }

            // --- Logika provjere da li korisnik SMIJE ostaviti recenziju (ponavlja se zbog sigurnosti) ---
            // 1. Provjeri da li je korisnik kupio ovaj proizvod putem ove narudžbe
           // bool kupioProizvod = await _dataContext.Narudzba
             //   .Where(n => n.Id == narudzbaId && n.KorisnikId == korisnik.Id).AnyAsync(); // Provjerava da li postoji takva narudžba za korisnika

           /* if (!kupioProizvod) // Ako korisnik NEMA tu narudžbu (ili nije njegova)
            {
                TempData["ErrorMessage"] = "Ne možete ostaviti recenziju putem nevažeće narudžbe.";
                return RedirectToAction("Details", "Proizvod", new { id = proizvodId });
            }*/ // zakomentarisala sam samo da mogu pristupiti bez narudzbe

            // 2. Provjeri da li je korisnik već ostavio recenziju za ovaj proizvod (općenito)
            //var vecPostojiRecenzija = await _dataContext.Recenzija
              //  .AnyAsync(r => r.ProizvodId == proizvodId && r.KorisnikId == korisnik.Id);

            /*if (vecPostojiRecenzija)
            {
                TempData["ErrorMessage"] = "Već ste ostavili recenziju za ovaj proizvod.";
                return RedirectToAction("Details", "Proizvod", new { id = proizvodId });
            }*/
            // --- Kraj logike provjere ---

            // Kreiraj novu instancu Recenzije i popuni samo ProizvodId i NarudzbaId (korisnik unosi ostalo)
            var recenzija = new Recenzija
            {
                //ProizvodId = proizvodId,
                ProizvodId = proizvodId,
                NarudzbaId = 2,
                KorisnikId = korisnik.Id // Popuni korisnikId odmah
            };

            // Dohvati naziv proizvoda za prikaz na formi (koristeći ViewData)
            var proizvod = await _dataContext.Proizvod.FindAsync(proizvodId);
           /* if (proizvod != null)
            {
                ViewData["ProizvodNaziv"] = proizvod.Naziv;
            }*/

            return View(recenzija); // Proslijedi djelomično popunjen model u View
        }

        // POST: Recenzija/OstaviRecenziju
        // Akcija za spremanje recenzije poslane s forme
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bindirajte samo polja koja korisnik unosi. KorisnikId, DatumDodavanja postavljamo mi.
        //ProizvodId,NarudzbaId,Ocjena,Tekst,DatumDodavanja,KorisnikI
        public async Task<IActionResult> OstaviRecenziju([Bind("ProizvodId,NarudzbaId,Ocjena,Tekst,DatumDodavanja")] Recenzija recenzija)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
                return Unauthorized();

            // Obavezno ručno postavi polja
            recenzija.KorisnikId = korisnik.Id;
            recenzija.DatumDodavanja = DateTime.Now;
            recenzija.NarudzbaId = 2;
            


            // Ukloni validaciju za polja koja korisnik ne unosi
            ModelState.Remove("KorisnikId");
            ModelState.Remove("Korisnik");
            ModelState.Remove("Narudzba");
            ModelState.Remove("NarudzbaId");
            ModelState.Remove("Proizvod");
            ModelState.Remove("DatumDodavanja");


            // --- Ponovna sigurnosna provjera (uvijek ponoviti na POST akcijama) ---
            /*bool kupioProizvod = await _dataContext.Narudzba
                                    .Where(n => n.Id == recenzija.NarudzbaId && n.KorisnikId == korisnik.Id)
                                    .AnyAsync();
            if (!kupioProizvod)
            {
                TempData["ErrorMessage"] = "Pokušaj dodavanja recenzije putem nevažeće narudžbe.";
                return RedirectToAction("Details", "Proizvod", new { id = recenzija.ProizvodId });
            }

            var vecPostoji = await _dataContext.Recenzija
                .AnyAsync(r => r.ProizvodId == recenzija.ProizvodId && r.KorisnikId == korisnik.Id);

            if (vecPostoji)
            {
                TempData["ErrorMessage"] = "Već ste ostavili recenziju za ovaj proizvod.";
                return RedirectToAction("Details", "Proizvod", new { id = recenzija.ProizvodId });
            }
            // --- Kraj sigurnosne provjere ---
            */

            if (ModelState.IsValid)
            {
                _dataContext.Recenzija.Add(recenzija);
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Vaša recenzija je uspješno poslana!";
                // Nakon dodavanja, preusmjeri na Detalje proizvoda
                return RedirectToAction("Details", "Proizvod", new { id = recenzija.ProizvodId });
            }

            // Ako validacija ne prođe, vratite se na formu s greškama
            var proizvod = await _dataContext.Proizvod.FindAsync(recenzija.ProizvodId);
            if (proizvod != null)
            {
                ViewData["ProizvodNaziv"] = proizvod.Naziv;
            }
            return View(recenzija); // Vratite model sa greškama validacije
        }

        // GET: Recenzija/UrediRecenziju/5
        public async Task<IActionResult> UrediRecenziju(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recenzija = await _dataContext.Recenzija.FindAsync(id);

            if (recenzija == null)
            {
                return NotFound();
            }

            // Dozvoli uređivanje samo sopstvene recenzije
            if (recenzija.KorisnikId != korisnik.Id)
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za uređivanje ove recenzije.";
                return RedirectToAction("Details", "Proizvod", new { id = recenzija.ProizvodId });
            }

            var proizvod = await _dataContext.Proizvod.FindAsync(recenzija.ProizvodId);
            if (proizvod != null)
            {
                ViewData["ProizvodNaziv"] = proizvod.Naziv;
            }

            return View(recenzija);
        }

        // POST: Recenzija/UrediRecenziju/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UrediRecenziju(int id, [Bind("Id,ProizvodId,Ocjena,Tekst,NarudzbaId")] Recenzija recenzija)
        {
            if (id != recenzija.Id)
            {
                return NotFound();
            }

            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return Unauthorized();
            }

            var originalRecenzija = await _dataContext.Recenzija.FindAsync(id);

            if (originalRecenzija == null)
            {
                return NotFound();
            }

            if (originalRecenzija.KorisnikId != korisnik.Id)
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za uređivanje ove recenzije.";
                return RedirectToAction("Details", "Proizvod", new { id = originalRecenzija.ProizvodId });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    originalRecenzija.Ocjena = recenzija.Ocjena;
                    originalRecenzija.Tekst = recenzija.Tekst; // Koristi Tekst
                    originalRecenzija.DatumDodavanja = System.DateTime.Now; // Ažuriraj datum promjene

                    _dataContext.Update(originalRecenzija);
                    await _dataContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecenzijaExists(recenzija.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Recenzija je uspješno ažurirana!";
                return RedirectToAction("Details", "Proizvod", new { id = originalRecenzija.ProizvodId });
            }

            var proizvod = await _dataContext.Proizvod.FindAsync(recenzija.ProizvodId);
            if (proizvod != null)
            {
                ViewData["ProizvodNaziv"] = proizvod.Naziv;
            }
            return View(recenzija);
        }

        // POST: Recenzija/IzbrisiRecenziju/5
        [HttpPost, ActionName("IzbrisiRecenziju")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IzbrisiRecenzijuConfirmed(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return Unauthorized();
            }

            var recenzija = await _dataContext.Recenzija.FindAsync(id);

            if (recenzija == null)
            {
                return NotFound();
            }

            if (recenzija.KorisnikId != korisnik.Id && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za brisanje ove recenzije.";
                return RedirectToAction("Details", "Proizvod", new { id = recenzija.ProizvodId });
            }

            var proizvodId = recenzija.ProizvodId;

            _dataContext.Recenzija.Remove(recenzija);
            await _dataContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Recenzija je uspješno obrisana!";
            return RedirectToAction("Details", "Proizvod", new { id = proizvodId });
        }

        private bool RecenzijaExists(int id)
        {
            return _dataContext.Recenzija.Any(e => e.Id == id);
        }
    }
}