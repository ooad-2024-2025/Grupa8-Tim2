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

        [HttpGet]
        public async Task<IActionResult> PrikaziRecenziju(int proizvodId)
        {
            var recenzije = await _dataContext.Recenzija
                                        .Include(r => r.Korisnik)
                                        .Where(r => r.ProizvodId == proizvodId)
                                        .OrderByDescending(r => r.DatumDodavanja)
                                        .ToListAsync();

            // Provjeri da li je AJAX poziv
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("~/Views/Recenzija/_RecenzijeListaPartial.cshtml", recenzije);
            }

            return PartialView("~/Views/Recenzija/_RecenzijeListaPartial.cshtml", recenzije);
        }


        [HttpGet]
        public async Task<IActionResult> GetReviewSummary(int proizvodId)
        {
            var recenzije = await _dataContext.Recenzija
                                    .Where(r => r.ProizvodId == proizvodId)
                                    .ToListAsync();

            ViewBag.ProsjecnaOcjena = recenzije.Any() ? recenzije.Average(r => r.Ocjena) : 0;
            ViewBag.BrojRecenzija = recenzije.Count();

            ViewBag.Recenzije = await _dataContext.Recenzija
                                                .Include(r => r.Korisnik)
                                                .Where(r => r.ProizvodId == proizvodId)
                                                .OrderByDescending(r => r.DatumDodavanja)
                                                .Take(3)
                                                .ToListAsync();

            return PartialView("~/Views/Recenzija/_RecenzijeSummaryPartial.cshtml");
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OstaviRecenziju([FromBody] Recenzija recenzija)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Korisnik nije prijavljen." });
            }

            recenzija.KorisnikId = userId;
            recenzija.DatumDodavanja = DateTime.Now;

            // Ukloni validaciju za NarudzbaId ako nije obavezna
            ModelState.Remove("NarudzbaId");
           // ModelState.Remove("Korisnik");
           // ModelState.Remove("Proizvod");
            //ModelState.Remove("Narudzba");

            // Provjeri da li korisnik već ima recenziju za ovaj proizvod
            var postojiRecenzija = await _dataContext.Recenzija
                .AnyAsync(r => r.ProizvodId == recenzija.ProizvodId && r.KorisnikId == userId);

            if (postojiRecenzija)
            {
                return Json(new { success = false, message = "Već ste ostavili recenziju za ovaj proizvod." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                return Json(new { success = false, message = "Validacija neuspješna.", errors = errors });
            }

            try
            {
                _dataContext.Add(recenzija);
                await _dataContext.SaveChangesAsync();
                return Json(new { success = true, message = "Recenzija uspješno dodana!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Došlo je do greške prilikom spremanja recenzije.", error = ex.Message });
            }
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