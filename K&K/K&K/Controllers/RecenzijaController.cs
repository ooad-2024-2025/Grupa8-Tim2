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

        //treba se zvat prikaziRecenzije
        // GET: Akcija za prikaz SVIH recenzija za određeni proizvod (za donji card)
        [HttpGet]
        public async Task<IActionResult> PrikaziRecenziju(int proizvodId)
        {
            var recenzije = await _dataContext.Recenzija
                                        .Include(r => r.Korisnik) // Učitajte Korisnik objekt
                                        .Where(r => r.ProizvodId == proizvodId)
                                        .OrderByDescending(r => r.DatumDodavanja) // Koristim DatumDodavanja
                                        .ToListAsync();

            return PartialView("PrikaziRecenziju", recenzije);
        }

        // NOVA GET akcija za dohvat SAMO sažetka recenzija (za gornji desni card)
        [HttpGet]
        public async Task<IActionResult> GetReviewSummary(int proizvodId)
        {
            var recenzije = await _dataContext.Recenzija
                                        .Include(r => r.Korisnik) // Učitajte Korisnik objekt ako je potrebno
                                        .Where(r => r.ProizvodId == proizvodId)
                                        .OrderByDescending(r => r.DatumDodavanja)
                                        .ToListAsync();

            var brojRecenzija = recenzije.Count;
            var prosjecnaOcjena = brojRecenzija > 0 ? recenzije.Average(r => r.Ocjena) : 0.0;

            // Proslijedite podatke u PartialView
            ViewBag.BrojRecenzija = brojRecenzija;
            ViewBag.ProsjecnaOcjena = prosjecnaOcjena;
            ViewBag.Recenzije = recenzije.Take(3).ToList(); // Samo prve 3 za summary

            return PartialView("_RecenzijeSummaryPartial"); // Vraća novi partial view
        }
    }


// GET: Recenzija/OstaviRecenziju?proizvodId=X&narudzbaId=Y
// Akcija za prikaz forme za ostavljanje recenzije
public async Task<IActionResult> OstaviRecenziju(int proizvodId, int narudzbaId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("OstaviRecenziju", new { proizvodId = proizvodId, narudzbaId = narudzbaId }) });
            }

            // --- Logika provjere da li korisnik SMIJE ostaviti recenziju (ponavlja se zbog sigurnosti) ---
            // 1. Provjeri da li je korisnik kupio ovaj proizvod putem ove narudžbe
            bool kupioProizvod = await _dataContext.Narudzba
                .Where(n => n.Id == narudzbaId && n.KorisnikId == korisnik.Id)
                // OVDJE JE PROBLEM BEZ STAVKE NARUDŽBE: ne možemo provjeriti je li proizvod bio u toj narudžbi direktno.
                // Ako pretpostavljamo da je 'narudzbaId' validan ID narudžbe korisnika,
                // i da je samo prisustvo te narudžbe dovoljno, onda je ovo ok.
                // Bolja provjera bi uključivala `StavkaNarudzbe`.
                .AnyAsync(); // Provjerava da li postoji takva narudžba za korisnika

            if (!kupioProizvod) // Ako korisnik NEMA tu narudžbu (ili nije njegova)
            {
                TempData["ErrorMessage"] = "Ne možete ostaviti recenziju putem nevažeće narudžbe.";
                return RedirectToAction("Details", "Proizvod", new { id = proizvodId });
            }

            // 2. Provjeri da li je korisnik već ostavio recenziju za ovaj proizvod (općenito)
            var vecPostojiRecenzija = await _dataContext.Recenzija
                .AnyAsync(r => r.ProizvodId == proizvodId && r.KorisnikId == korisnik.Id);

            if (vecPostojiRecenzija)
            {
                TempData["ErrorMessage"] = "Već ste ostavili recenziju za ovaj proizvod.";
                return RedirectToAction("Details", "Proizvod", new { id = proizvodId });
            }
            // --- Kraj logike provjere ---

            // Kreiraj novu instancu Recenzije i popuni samo ProizvodId i NarudzbaId (korisnik unosi ostalo)
            var recenzija = new Recenzija
            {
                ProizvodId = proizvodId,
                NarudzbaId = narudzbaId,
                KorisnikId = korisnik.Id // Popuni korisnikId odmah
            };

            // Dohvati naziv proizvoda za prikaz na formi (koristeći ViewData)
            var proizvod = await _dataContext.Proizvod.FindAsync(proizvodId);
            if (proizvod != null)
            {
                ViewData["ProizvodNaziv"] = proizvod.Naziv;
            }

            return View(recenzija); // Proslijedi djelomično popunjen model u View
        }

        // POST: Recenzija/OstaviRecenziju
        // Akcija za spremanje recenzije poslane s forme
        // POST: Akcija za obradu recenzije (AJAX poziv)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OstaviRecenziju([FromBody] Recenzija recenzija)
        {
            // Ovdje možete dodati provjeru da li ProizvodId zaista postoji
            var proizvodExists = await _dataContext.Proizvod.AnyAsync(p => p.Id == recenzija.ProizvodId);
            if (!proizvodExists)
            {
                return Json(new { success = false, message = "Navedeni proizvod ne postoji." });
            }

            // Manually add ModelState error if required field is empty for [FromBody]
            if (recenzija.Ocjena == 0) // Pretpostavljam da je 0 default za int i da je obavezna
            {
                ModelState.AddModelError("Ocjena", "Ocjena je obavezna.");
            }
            if (string.IsNullOrWhiteSpace(recenzija.Tekst)) // Pretpostavljam da je tekst obavezan
            {
                ModelState.AddModelError("Tekst", "Tekst recenzije je obavezan.");
            }


            if (ModelState.IsValid)
            {
                // Postavite trenutni datum i vrijeme
                recenzija.DatumDodavanja = DateTime.Now;

                // Pretpostavka: Ako koristite ASP.NET Core Identity za korisnike,
                // možete dohvatiti trenutnog korisnika ovako:
                // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // recenzija.KorisnikId = userId;
                // Inače, KorisnikId može biti null ili ga morate postaviti na neki anonimni ID.
                // Morate se pobrinuti da je KorisnikId setiran ispravno, ako je obavezan.

                _dataContext.Add(recenzija);
                await _dataContext.SaveChangesAsync();

                // Da bi se username prikazao u PartialViewu, moramo ga dohvatiti
                // nakon što je recenzija spremljena i prije slanja JSON-a.
                // U ovom slučaju, bolje je da PartialView sam dohvati korisnika.

                return Json(new { success = true, message = "Recenzija uspješno dodana!" });
            }

            var errors = ModelState.Values
                                   .SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage)
                                   .ToList();
            return Json(new { success = false, message = "Validacija neuspješna.", errors = errors });
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