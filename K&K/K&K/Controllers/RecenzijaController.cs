﻿// K_K/Controllers/RecenzijaController.cs

using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace K_K.Controllers
{
    public class RecenzijaController : Controller
    {
        private readonly ApplicationDbContext _dataContext;
        private readonly UserManager<Osoba> _userManager;
        private readonly SignInManager<Osoba> _signInManager;

        public RecenzijaController(ApplicationDbContext context, UserManager<Osoba> userManager, SignInManager<Osoba> signInManager)
        {
            _dataContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public async Task<IActionResult> PrikaziRecenziju(int proizvodId)
        {
            // Ovdje je ključno da se Korisnik učita da ne bi bio null u viewu
            var recenzije = await _dataContext.Recenzija
                                            .Include(r => r.Korisnik)
                                            .Where(r => r.ProizvodId == proizvodId)
                                            .OrderByDescending(r => r.DatumDodavanja)
                                            .ToListAsync();

            var proizvod = await _dataContext.Proizvod
                                            .Where(r => r.Id == proizvodId)
                                            .FirstOrDefaultAsync();

            ViewData["ProizvodId"] = proizvodId;
            ViewData["KorisnikId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["IsAdmin"] = User.IsInRole("Admin");

            ViewBag.ProizvodNaziv = proizvod?.Naziv;

            bool mozeLiRecenziju = false;
            if (User.Identity.IsAuthenticated)
            {
                var korisnik = await _userManager.GetUserAsync(User);
                if (korisnik != null)
                {

                    var imaLiNarudzbu = await _dataContext.StavkaNarudzbe
                        .Include(s => s.Narudzba)
                        .AnyAsync(s => s.ProizvodId == proizvodId &&
                                      s.Narudzba.KorisnikId == korisnik.Id &&
                                      s.Narudzba.StatusNarudzbe == StatusNarudzbe.Gotova);

                    var imaLiRecenziju = await _dataContext.Recenzija
                        .AnyAsync(r => r.ProizvodId == proizvodId && r.KorisnikId == korisnik.Id);

                    mozeLiRecenziju = imaLiNarudzbu && !imaLiRecenziju;
                }
            }
            ViewData["MozeLiRecenziju"] = mozeLiRecenziju;
            
            // Provjerimo da li je poziv AJAX-om
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("~/Views/Recenzija/_RecenzijeListaPartial.cshtml", recenzije);
            }
            else
            {
                // Ovo bi se dešavalo samo ako direktno pristupite URL-u /Recenzija/PrikaziRecenziju?proizvodId=X
                return View("RecenzijeView", recenzije);
            }
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
        [Authorize(Roles = "Korisnik")]
        public async Task<IActionResult> OstaviRecenziju(int proizvodId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                
                return Redirect("/Identity/Account/Register");
            }
            var postojiProizvod = await _dataContext.Proizvod.AnyAsync(p => p.Id == proizvodId);
            if (!postojiProizvod)
            {
                TempData["ProizvodNotFound"] = "Proizvod nije pronađen!";
                return RedirectToAction("Index", "Proizvod");
            }
            var imaLiNarudzbu = await _dataContext.StavkaNarudzbe.
                Include(s => s.Narudzba)
                .AnyAsync(s => s.ProizvodId == proizvodId &&
                            s.Narudzba.KorisnikId == korisnik.Id &&
                            s.Narudzba.StatusNarudzbe == StatusNarudzbe.Gotova);
            if (!imaLiNarudzbu)
            {
                TempData["RecenzijaGreska"] = "Možete ostaviti recenziju samo za proizvode koje ste naručili.";
                return RedirectToAction("Details", "Proizvod", new { id = proizvodId });
            }
            var imaLiRecenziju = await _dataContext.Recenzija.
                        AnyAsync(r => r.ProizvodId == proizvodId &&
                                 r.KorisnikId == korisnik.Id);
            if (imaLiRecenziju)
            {
                TempData["RecenzijaGreska"] = "Već ste ostavili recenziju za ovaj proizvod";
                return RedirectToAction("Details", "Proizvod", new { id = proizvodId });
            }
            var narudzba = await _dataContext.StavkaNarudzbe.
                Include(s => s.Narudzba).Where(s => s.ProizvodId == proizvodId &&
                                               s.Narudzba.KorisnikId == korisnik.Id &&
                                               s.Narudzba.StatusNarudzbe == StatusNarudzbe.Gotova)
                .Select(s => s.Narudzba)
                .FirstOrDefaultAsync();
            // Kreiramo novu instancu Recenzije i popuni samo ProizvodId i NarudzbaId (korisnik unosi ostalo)
            var recenzija = new Recenzija
            {
                ProizvodId = proizvodId,
                NarudzbaId = narudzba.Id,
                KorisnikId = korisnik.Id // Popuni korisnikId odmah
            };

            var proizvod = await _dataContext.Proizvod.FindAsync(proizvodId);
            if (proizvod != null)
            {
                 ViewData["ProizvodNaziv"] = proizvod.Naziv;
            }

            return View(recenzija);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Korisnik")]

        public async Task<IActionResult> OstaviRecenziju([Bind("ProizvodId,NarudzbaId,Ocjena,Tekst,DatumDodavanja")] Recenzija recenzija)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
                return Unauthorized();
            var narudzba = await _dataContext.StavkaNarudzbe
                .Include(s => s.Narudzba)
                .Where(s => s.ProizvodId == recenzija.ProizvodId &&
                       s.Narudzba.KorisnikId == korisnik.Id &&
                       s.Narudzba.StatusNarudzbe == StatusNarudzbe.Gotova)
                .Select(s => s.Narudzba)
                .FirstOrDefaultAsync();


            var imaLiNarudzbu = await _dataContext.StavkaNarudzbe.
                Include(s => s.Narudzba)
                .AnyAsync(s => s.ProizvodId == recenzija.ProizvodId &&
                            s.Narudzba.KorisnikId == korisnik.Id &&
                            s.Narudzba.StatusNarudzbe == StatusNarudzbe.Gotova);
            if (!imaLiNarudzbu)
            {
                TempData["RecenzijaGreska"] = "Možete ostaviti recenziju samo za proizvode koje ste naručili.";
                return RedirectToAction("Details", "Proizvod", new { id = recenzija.ProizvodId });
            }
            var imaLiRecenziju = await _dataContext.Recenzija.
                        AnyAsync(r => r.ProizvodId == recenzija.ProizvodId &&
                                 r.KorisnikId == korisnik.Id);
            if (imaLiRecenziju)
            {
                TempData["RecenzijaGreska"] = "Već ste ostavili recenziju za ovaj proizvod";
                return RedirectToAction("Details", "Proizvod", new { id = recenzija.ProizvodId });
            }
            
            recenzija.KorisnikId = korisnik.Id;
            recenzija.DatumDodavanja = DateTime.Now;
            recenzija.NarudzbaId = narudzba.Id;

            //uklanjanje validacije za polja koja korisnik ne unosi
            ModelState.Remove("KorisnikId");
            ModelState.Remove("Korisnik");
            ModelState.Remove("Narudzba");
            ModelState.Remove("NarudzbaId");
            ModelState.Remove("Proizvod");
            ModelState.Remove("DatumDodavanja");

            if (ModelState.IsValid)
            {
                try
                {
                    _dataContext.Add(recenzija);
                    await _dataContext.SaveChangesAsync();

                    TempData["PorukaUspjeha"] = "Vaša recenzija je uspješno dodana!";
                    TempData["ProizvodIdZaPovratak"] = recenzija.ProizvodId; // ProizvodId za povratak na Details view

                    return RedirectToAction("UspjehRecenzije"); 
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

            var proizvod = await _dataContext.Proizvod.FindAsync(recenzija.ProizvodId);
            ViewBag.ProizvodNaziv = proizvod?.Naziv;
            return View(recenzija);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Korisnik, Radnik")]
        public async Task<IActionResult> DetaljiRecenzije(int id)
        {
            var recenzija = await _dataContext.Recenzija
                                            .Include(r => r.Korisnik) 
                                            .Include(r => r.Proizvod) 
                                            .FirstOrDefaultAsync(m => m.Id == id);

            if (recenzija == null)
            {
                return NotFound();
            }

            string currentUserId = null;
            bool isAdmin = false;

            if (_signInManager.IsSignedIn(User))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    currentUserId = currentUser.Id;
                    isAdmin = await _userManager.IsInRoleAsync(currentUser, "Administrator");
                }
            }

            ViewData["CurrentUserId"] = currentUserId;
            ViewData["IsAdmin"] = isAdmin;
            ViewData["ProizvodNaziv"] = recenzija.Proizvod?.Naziv; // Naziv proizvoda za prikaz u viewu

            return View("DetaljiRecenzije", recenzija); // Vraća view "DetaljiRecenzije.cshtml"
        }

        [HttpGet]
        [Authorize(Roles = "Korisnik")]
        public async Task<IActionResult> UrediRecenziju(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                TempData["RecenzijaPrijava"] = "Morate biti prijavljeni da biste uredili recenziju.";
                return Redirect("/Identity/Account/Login");
            }

            var recenzija = await _dataContext.Recenzija.FindAsync(id);

            if (recenzija == null)
            {
                TempData["RecenzijaNotFound"] = "Recenzija nije pronađena.";
                return RedirectToAction("Index", "Proizvod");
            }

            if (recenzija.KorisnikId != korisnik.Id && !await _userManager.IsInRoleAsync(korisnik, "Administrator"))
            {
                TempData["RecenzijaEditError"] = "Nemate dozvolu za uređivanje ove recenzije.";
                return RedirectToAction("DetaljiRecenzije", new { id = recenzija.Id }); // Vratite korisnika na detalje recenzije
            }

            var proizvod = await _dataContext.Proizvod.FindAsync(recenzija.ProizvodId);
            if (proizvod != null)
            {
                ViewData["ProizvodNaziv"] = proizvod.Naziv;
            }

            return View("UrediRecenziju", recenzija);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Korisnik")]
        public async Task<IActionResult> UrediRecenziju(int id, [Bind("Id,ProizvodId,Ocjena,Tekst,NarudzbaId,KorisnikId")] Recenzija recenzija) // DODANO KorisnikId
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

            if (originalRecenzija.KorisnikId != korisnik.Id) // uklonila sam && !await _userManager.IsInRoleAsync(korisnik, "Administrator"
            {
                TempData["RecenzijaEditError"] = "Nemate dozvolu za ažuriranje ove recenzije.";
                return RedirectToAction("DetaljiRecenzije", new { id = originalRecenzija.Id });
            }


            if (ModelState.IsValid)
            {
                try
                {
                    originalRecenzija.DatumDodavanja = System.DateTime.Now;

                    // Koristim SetValues za preostala polja koja dolaze iz forme
                    // To su Ocjena i Tekst, kao i Id, ProizvodId, NarudzbaId, KorisnikId iz hidden polja
                    _dataContext.Entry(originalRecenzija).CurrentValues.SetValues(recenzija);
                    await _dataContext.SaveChangesAsync();

                    TempData["RecenzijaEDIT"] = "Recenzija je uspješno ažurirana!";
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
            return View("UrediRecenziju", recenzija);
        }

        private bool RecenzijaExists(int id)
        {
            return _dataContext.Recenzija.Any(e => e.Id == id);
        }

        // POST: Recenzija/IzbrisiRecenziju/5

        [HttpPost, ActionName("IzbrisiRecenziju")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Korisnik")]
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

            if (recenzija.KorisnikId != korisnik.Id && !await _userManager.IsInRoleAsync(korisnik, "Administrator"))
            {
                TempData["RecenzijaDeleteError"] = "Nemate dozvolu za brisanje ove recenzije.";
                return RedirectToAction("Details", "Proizvod", new { id = recenzija.ProizvodId });
            }

            var proizvodId = recenzija.ProizvodId;

            _dataContext.Recenzija.Remove(recenzija);
            await _dataContext.SaveChangesAsync();

            TempData["RecenzijaDeleteOk"] = "Recenzija je uspješno obrisana!";
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
                ViewBag.ProizvodId = null;
            }

            return View();
        }

    }
}