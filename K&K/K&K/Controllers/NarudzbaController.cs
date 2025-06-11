using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace K_K.Controllers
{

    //[Authorize(Roles = "Administrator,User")]
    public class NarudzbaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Osoba> _userManager;
        private readonly Models.IEmailSender _emailSender;

        public NarudzbaController(ApplicationDbContext context, UserManager<Osoba> userManager, Models.IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Narudzba
        public async Task<IActionResult> Index()
        {/* //ovo dodati kad dodamo uloge
            IQueryable<Narudzba> narudzbeQuery = _context.Narudzba
                                                .Include(n => n.Korisnik)
                                                .Include(n => n.Radnik);
            if (!User.IsInRole("Admin"))
            {
                // Korisnik nije admin, prikaži samo njegove narudžbe
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return Unauthorized();
                }

                narudzbeQuery = narudzbeQuery.Where(n => n.KorisnikId == userId);
            }

            var narudzbe = await narudzbeQuery.ToListAsync();*/
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return Unauthorized(); // ili redirect na login
            }
            var narudzbe = await _context.Narudzba
                        .Where(n=>n.KorisnikId==korisnik.Id &&
                n.StatusNarudzbe != StatusNarudzbe.NaCekanju)
                        .ToListAsync();

            return View(narudzbe);
            //var applicationDbContext = _context.Narudzba.Include(n => n.Korisnik).Include(n => n.Radnik);
            //return View(await applicationDbContext.ToListAsync());
        }

        // GET: Narudzba/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            /* var narudzba = await _context.Narudzba
             .Include(n => n.Korisnik)
             //.Include(n => n.Radnik)
             .FirstOrDefaultAsync(n => n.Id == id);*/
            var narudzba = await _context.Narudzba.FindAsync(id);

            if (narudzba == null)
            {
                return NotFound();
            }

            var stavke = await _context.StavkaNarudzbe
             .Where(s => s.NarudzbaId == id)
             .Include(s => s.Proizvod) 
             .ToListAsync();

            ViewData["stavke"] = stavke;
            return View(narudzba);
        }


        // GET: Narudzba/Create
        public async Task<IActionResult> Create()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return Redirect("/Identity/Account/Register");
            }

            var korpa = await _context.Korpa.FirstOrDefaultAsync(k => k.KorisnikId == korisnik.Id);
            if (korpa == null)
            {
                TempData["ErrorMessage"] = "Korpa ne postoji.";
                return RedirectToAction("Index", "Proizvod");
            }
            var stavke = await _context.StavkaKorpe
               .Where(s => s.KorpaId == korpa.Id)
               .ToListAsync();

            if (!stavke.Any()) // Provjera da li korpa ima stavki prije prikaza forme
            {
                TempData["ErrorMessage"] = "Vaša korpa je prazna. Dodajte proizvode prije naručivanja.";
                return RedirectToAction("Index", "Proizvod");
            }

            double ukupnaCijena = korpa?.ukupnaCijena ?? 0;

            ViewData["Cijena"] = ukupnaCijena;
            ViewData["KorpaStavke"] = stavke;
            ViewData["KorpaId"] = korpa.Id;
            // Ne treba SelectList za KorisnikId i RadnikId jer ih postavljamo automatski
            // ViewData["KorisnikId"] = new SelectList(_context.Users, "Id", "Email");
            // ViewData["RadnikId"] = new SelectList(_context.Users, "Id", "Email");

            // Ne treba SelectList za NacinPlacanja jer će korisnik birati dugmetom
            ViewData["NacinPreuzimanja"] = new SelectList(Enum.GetValues(typeof(VrstaPreuzimanja)));
            ViewData["DanasnjiDatum"] = DateTime.Now.ToString("dd-MM-yyyy");
            return View();
        }

        // POST: Narudzba/Create (Jedinstvena akcija za oba načina plaćanja)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("NacinPreuzimanja,DatumNarudzbe,AdresaDostave")] Narudzba narudzba,
            string submitButton // Dodajemo parametar za prepoznavanje kliknutog dugmeta
        )
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return Redirect("/Identity/Account/Register");
            }

            var korpa = await _context.Korpa.FirstOrDefaultAsync(k => k.KorisnikId == korisnik.Id);
            var stavkeUKorpi = await _context.StavkaKorpe
                .Where(s => s.KorpaId == korpa.Id)
                .ToListAsync();
            if (stavkeUKorpi == null || stavkeUKorpi.Count == 0)
            {
                TempData["ErrorMessage"] = "Vaša korpa je prazna. Dodajte proizvode prije naručivanja.";
                return RedirectToAction("Index", "Proizvod");
            }

            narudzba.KorisnikId = korisnik.Id;
            narudzba.RadnikId = "f7974104-9ab3-4d73-a79b-85f21b1c9f68"; // Hardkodiran radnik ID

            // Ukloni greske validacije za polja koja postavljamo rucno
            ModelState.Remove("Korisnik");
            ModelState.Remove("Radnik");
            ModelState.Remove("KorisnikId");
            ModelState.Remove("RadnikId");
            ModelState.Remove("NacinPlacanja"); // Ukloni NacinPlacanja jer se postavlja na osnovu dugmeta

            // Postavi NacinPlacanja i StatusNarudzbe na osnovu kliknutog dugmeta
            if (submitButton == "gotovina")
            {
                narudzba.NacinPlacanja = VrstaPlacanja.Gotovina;
                narudzba.StatusNarudzbe = StatusNarudzbe.Potvrdjena; // Odmah potvrđena za gotovinu
            }
            else if (submitButton == "kartica")
            {
                narudzba.NacinPlacanja = VrstaPlacanja.Kartica;
                narudzba.StatusNarudzbe = StatusNarudzbe.NaCekanju;
              //  narudzba.StatusNarudzbe = StatusNarudzbe.NaCekanjuPlacanja; // Na čekanju za karticu
            }
            else
            {
                // Ovo bi se trebalo dogoditi samo ako korisnik na neki način pošalje formu bez klika na dugme
                ModelState.AddModelError("", "Morate odabrati način plaćanja.");
            }


            if (ModelState.IsValid)
            {
                _context.Add(narudzba);
                await _context.SaveChangesAsync();

                // Premjesti stavke iz korpe u stavke narudžbe
                foreach (var stavka in stavkeUKorpi)
                {
                    var novaStavka = new StavkaNarudzbe
                    {
                        NarudzbaId = narudzba.Id,
                        ProizvodId = stavka.ProizvodId,
                        Kolicina = stavka.Kolicina,
                        Cijena = stavka.Cijena
                    };
                    _context.StavkaNarudzbe.Add(novaStavka);
                }
                await _context.SaveChangesAsync();

                if (narudzba.NacinPlacanja == VrstaPlacanja.Kartica)
                {
                    // Za kartično plaćanje, ne brišemo korpu ovdje!
                    // Korpa se briše tek nakon uspješnog plaćanja u KarticnoPlacanjeControlleru.
                    return RedirectToAction("Unos", "KarticnoPlacanje", new { id = narudzba.Id });
                }
                else // Gotovina
                {
                    // Za gotovinsko plaćanje, brišemo korpu i preusmjeravamo na uspjeh
                    _context.StavkaKorpe.RemoveRange(stavkeUKorpi);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Narudžba je uspješno kreirana i potvrđena!";
                    return RedirectToAction("Uspjeh", "Narudzba"); // Preusmjeri na Uspjeh akciju u NarudzbaControlleru
                }
            }

            // Ako model nije validan, vratite korisnika na formu
            double ukupnaCijena = korpa?.ukupnaCijena ?? 0;
            ViewData["Cijena"] = ukupnaCijena;
            ViewData["KorpaStavke"] = stavkeUKorpi;
            ViewData["KorpaId"] = korpa.Id;
            ViewData["NacinPreuzimanja"] = new SelectList(Enum.GetValues(typeof(VrstaPreuzimanja)), narudzba.NacinPreuzimanja);
            return View(narudzba);
        }

        // Akcija za uspjeh nakon gotovinskog plaćanja
        public IActionResult Uspjeh()
        {
            // Ova akcija će prikazati poruku o uspješnoj narudžbi
            return View();
        }

        // GET: Narudzba/Edit/5
        public async Task<IActionResult> Edit(int? id)
        { 
            
        
            
            if (id == null)
            {
                return NotFound();
            }

            var narudzba = await _context.Narudzba.FindAsync(id);

            if (narudzba == null)
            {
                return NotFound();
            }
           /* if (!User.IsInAnyRole("Admin", "Radnik"))
            {

            }*/

                ViewData["KorisnikId"] = new SelectList(_context.Users, "Id", "Email", narudzba.KorisnikId);
           ViewData["RadnikId"] = new SelectList(_context.Users, "Id", "Email", narudzba.RadnikId);
            ViewData["NacinPlacanja"] = new SelectList(Enum.GetValues(typeof(VrstaPlacanja)), narudzba.NacinPlacanja);

            return View(narudzba);
            
        }

        // POST: Narudzba/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //Id,KorisnikId,RadnikId, ovo sam izbacila iz Bind dodati ako bude trebalo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StatusNarudzbe,NacinPlacanja,NacinPreuzimanja,DatumNarudzbe,AdresaDostave")] Narudzba narudzba)
        {
            
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return Redirect("/Identity/Account/Register");
            }

            //var postojeca = await _context.Narudzba.FindAsync(id);
            if (narudzba == null)
            {
                return NotFound();
            }

            
            narudzba.Id = id;
            narudzba.KorisnikId = korisnik.Id;
           narudzba.RadnikId = "f7974104-9ab3-4d73-a79b-85f21b1c9f68";

            ModelState.Remove("Korisnik");
            ModelState.Remove("Radnik");
            ModelState.Remove("KorisnikId");
            ModelState.Remove("RadnikId");

            if (ModelState.IsValid)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("<html><body>");
                    sb.AppendLine($"<p>Poštovani <strong>{korisnik.Ime} {korisnik.Prezime}</strong>,</p>");
                    sb.AppendLine("<p>Zahvaljujemo na vašoj narudžbi.</p>");
                    sb.AppendLine("<p>Vaša narudžba je gotova i možete je preuzeti.</p>");
                    sb.AppendLine("<p>S poštovanjem,<br/>Tim K&amp;K</p>");
                    sb.AppendLine("</body></html>");

                    string htmlBody = sb.ToString();
                    _context.Update(narudzba);
                    await _context.SaveChangesAsync();
                    if (narudzba.StatusNarudzbe == StatusNarudzbe.Gotova)
                    {
                        await _emailSender.SendEmailAsync(korisnik.Email, "Potvrda narudžbe", sb.ToString());
                    }
                    if (narudzba.NacinPlacanja == VrstaPlacanja.Kartica)
                    {
                        return RedirectToAction("Unos","KarticnoPlacanje");
                    }
                  return RedirectToAction(nameof(Index));
                    //return RedirectToAction("Index","Proizvod");
                    

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NarudzbaExists(narudzba.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we get here, model validation failed - return to Edit view
            ViewData["KorisnikId"] = new SelectList(_context.Users, "Id", "Email", narudzba.KorisnikId);
           ViewData["RadnikId"] = new SelectList(_context.Users, "Id", "Email", narudzba.RadnikId);
            ViewData["NacinPlacanja"] = new SelectList(Enum.GetValues(typeof(VrstaPlacanja)), narudzba.NacinPlacanja);
            ViewData["NacinPreuzimanja"] = new SelectList(Enum.GetValues(typeof(VrstaPreuzimanja)), narudzba.NacinPreuzimanja);

            return View(narudzba);
           

        }

        // GET: Narudzba/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var narudzba = await _context.Narudzba
                .Include(n => n.Korisnik)
                .Include(n => n.Radnik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (narudzba == null)
            {
                return NotFound();
            }

            return View(narudzba);
        }

        // POST: Narudzba/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var narudzba = await _context.Narudzba.FindAsync(id);
            if (narudzba != null)
            {
                _context.Narudzba.Remove(narudzba);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NarudzbaExists(int id)
        {
            return _context.Narudzba.Any(e => e.Id == id);
        }
        
       

    }
}
