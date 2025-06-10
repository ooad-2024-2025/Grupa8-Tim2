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
        private readonly UserManager<Osoba> _userManager;
        private readonly SignInManager<Osoba> _signInManager; // Dodana injekcija

        public RecenzijaController(ApplicationDbContext context, UserManager<Osoba> userManager, SignInManager<Osoba> signInManager)
        {
            _dataContext = context;
            _userManager = userManager;
            _signInManager = signInManager; // Dodana inicijalizacija
        }

        [HttpGet]
        public async Task<IActionResult> PrikaziRecenziju(int proizvodId)
        {
            
            var recenzije = await _dataContext.Recenzija
                          .Where(r => r.ProizvodId == proizvodId)
                           .ToListAsync();
            var proizvod = await _dataContext.Proizvod
          .Where(r => r.Id == proizvodId)
           .FirstOrDefaultAsync();


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
            recenzija.NarudzbaId = 30;



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

            // --- Kraj sigurnosne provjere ---
            */

            if (ModelState.IsValid)
            {
                try
                {
                    _dataContext.Add(recenzija);
                    await _dataContext.SaveChangesAsync();

                    TempData["PorukaUspjeha"] = "Vaša recenzija je uspješno dodana!";
                    TempData["ProizvodIdZaPovratak"] = recenzija.ProizvodId; // ProizvodId za povratak na Details view

                    return RedirectToAction("UspjehRecenzije"); // <--- OVO JE KLJUČNO!
                }
                catch (Exception ex)
                {
                    // U slučaju greške pri spremanju u bazu
                    ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom spremanja recenzije: " + ex.Message);
                    // Dohvati proizvod naziv ponovo ako je potrebno za view
                    var proizvodZaView = await _dataContext.Proizvod.FindAsync(recenzija.ProizvodId);
                    ViewBag.ProizvodNaziv = proizvodZaView?.Naziv;
                    return View(recenzija);
                }
            }

            // Ako ModelState.IsValid nije true (i iznad try-catch bloka), vratite view sa greškama
            var proizvod = await _dataContext.Proizvod.FindAsync(recenzija.ProizvodId);
            ViewBag.ProizvodNaziv = proizvod?.Naziv;
            return View(recenzija);
        }

        [HttpGet]
        public async Task<IActionResult> DetaljiRecenzije(int id)
        {
            var recenzija = await _dataContext.Recenzija
                                            .Include(r => r.Korisnik) // Učitajte korisnika za prikaz detalja
                                            .Include(r => r.Proizvod) // Učitajte proizvod za pristup nazivu
                                            .FirstOrDefaultAsync(m => m.Id == id);

            if (recenzija == null)
            {
                return NotFound();
            }

            // Provjeri trenutnog korisnika za uvjetno prikazivanje dugmadi "Uredi" i "Obriši"
            string currentUserId = null;
            bool isAdmin = false;

            if (_signInManager.IsSignedIn(User))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    currentUserId = currentUser.Id;
                    isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
                }
            }

            // Proslijedi podatke u ViewData za view
            ViewData["CurrentUserId"] = currentUserId;
            ViewData["IsAdmin"] = isAdmin;
            ViewData["ProizvodNaziv"] = recenzija.Proizvod?.Naziv; // Naziv proizvoda za prikaz u viewu

            return View("DetaljiRecenzije", recenzija); // Vraća view "DetaljiRecenzije.cshtml"
        }

        [HttpGet]
        public async Task<IActionResult> UrediRecenziju(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                TempData["ErrorMessage"] = "Morate biti prijavljeni da biste uredili recenziju.";
                return Redirect("/Identity/Account/Login");
            }

            var recenzija = await _dataContext.Recenzija.FindAsync(id);

            if (recenzija == null)
            {
                TempData["ErrorMessage"] = "Recenzija nije pronađena.";
                return RedirectToAction("Index", "Proizvod");
            }

            // SIGURNOSNA PROVJERA: Dozvoli uređivanje samo sopstvene recenzije (ili ako je korisnik Admin)
            if (recenzija.KorisnikId != korisnik.Id && !await _userManager.IsInRoleAsync(korisnik, "Admin"))
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za uređivanje ove recenzije.";
                return RedirectToAction("DetaljiRecenzije", new { id = recenzija.Id }); // Vratite korisnika na detalje recenzije
            }

            var proizvod = await _dataContext.Proizvod.FindAsync(recenzija.ProizvodId);
            if (proizvod != null)
            {
                ViewData["ProizvodNaziv"] = proizvod.Naziv;
            }

            // Ispravno: Vraća view "UrediRecenziju.cshtml" sa modelom recenzije
            return View("UrediRecenziju", recenzija);
        }

        // POST: Recenzija/UrediRecenziju/5
        // Prima podatke iz forme za uređivanje i sprema ih u bazu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UrediRecenziju(int id, [Bind("Id,ProizvodId,Ocjena,Tekst,NarudzbaId")] Recenzija recenzija)
        {
            if (id != recenzija.Id)
            {
                TempData["ErrorMessage"] = "Greška: ID recenzije se ne podudara.";
                return NotFound();
            }

            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                TempData["ErrorMessage"] = "Niste autorizovani za ovu akciju.";
                return Unauthorized();
            }

            var originalRecenzija = await _dataContext.Recenzija.FindAsync(id);

            if (originalRecenzija == null)
            {
                TempData["ErrorMessage"] = "Recenzija za uređivanje nije pronađena.";
                return NotFound();
            }

            if (originalRecenzija.KorisnikId != korisnik.Id && !await _userManager.IsInRoleAsync(korisnik, "Admin"))
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za ažuriranje ove recenzije.";
                return RedirectToAction("DetaljiRecenzije", new { id = originalRecenzija.Id });
            }

            // Popunite polja koja dolaze iz hidden inputa ili koja se ne mijenjaju
            recenzija.KorisnikId = originalRecenzija.KorisnikId;
            recenzija.DatumDodavanja = System.DateTime.Now; // Ažuriraj datum promjene

            // Provjeri validaciju modela (npr. Required, Range atributi)
            if (ModelState.IsValid)
            {
                try
                {
                    // Update entiteta u DbContextu
                    _dataContext.Entry(originalRecenzija).CurrentValues.SetValues(recenzija);
                    await _dataContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Recenzija je uspješno ažurirana!";
                    // Preusmjeri korisnika nazad na stranicu detalja recenzije
                    return RedirectToAction("DetaljiRecenzije", new { id = originalRecenzija.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecenzijaExists(recenzija.Id))
                    {
                        TempData["ErrorMessage"] = "Recenzija je obrisana od strane drugog korisnika.";
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom ažuriranja recenzije: " + ex.Message);
                }
            }

            // Ako ModelState nije validan ili je došlo do greške, ponovo prikaži formu
            var proizvod = await _dataContext.Proizvod.FindAsync(originalRecenzija.ProizvodId);
            ViewData["ProizvodNaziv"] = proizvod?.Naziv;
            return View("UrediRecenziju", originalRecenzija);
        }


        // Dodajte ovu metodu negdje u kontroler (ako je već nemate)
        private bool RecenzijaExists(int id)
        {
            return _dataContext.Recenzija.Any(e => e.Id == id);
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

            if (recenzija.KorisnikId != korisnik.Id && !await _userManager.IsInRoleAsync(korisnik, "Admin"))
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


        public IActionResult UspjehRecenzije()
        {
            // Provjeri da li TempData sadrži ključeve
            if (TempData.ContainsKey("PorukaUspjeha"))
            {
                ViewBag.Poruka = TempData["PorukaUspjeha"];
            }
            else
            {
                ViewBag.Poruka = "Recenzija je uspješno dodana! (Nema detalja o poruci)"; // Default poruka
            }

            if (TempData.ContainsKey("ProizvodIdZaPovratak"))
            {
                ViewBag.ProizvodId = TempData["ProizvodIdZaPovratak"];
            }
            else
            {
                ViewBag.ProizvodId = null; // Ili neka defaultna vrijednost ako želite
            }

            return View();
        }

    }
}


