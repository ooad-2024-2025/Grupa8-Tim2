using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace K_K.Controllers
{

    //[Authorize(Roles = "Administrator,User")]
    public class NarudzbaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Osoba> _userManager;

        public NarudzbaController(ApplicationDbContext context, UserManager<Osoba> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                        .Where(n=>n.KorisnikId==korisnik.Id) 
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
        //prepravila sam da bude Task<IActionResult>
        public async Task<IActionResult> Create()
        {  
            var korisnik =  await _userManager.GetUserAsync(User);
            
            if (korisnik == null)
            {
                return Redirect("/Identity/Account/Register");
            }


            // Dohvati sve stavke iz korpe za korisnika
            var korpa = await _context.Korpa.FirstOrDefaultAsync(k => k.KorisnikId == korisnik.Id);
            if (korpa == null)
            {
                TempData["ErrorMessage"] = "Korpa ne postoji.";
                return RedirectToAction("Index", "Proizvod");
            }
            var stavke= await _context.StavkaKorpe
               .Where(s => s.KorpaId == korpa.Id)
               .ToListAsync();

            double ukupnaCijena = korpa?.ukupnaCijena ?? 0;

            ViewData["Cijena"] = ukupnaCijena;
            ViewData["KorpaStavke"] = stavke;
            ViewData["KorpaId"] = korpa.Id;

            ViewData["KorisnikId"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["RadnikId"] = new SelectList(_context.Users, "Id", "Email");

            ViewData["NacinPreuzimanja"] = new SelectList(Enum.GetValues(typeof(VrstaPreuzimanja)));
            ViewData["NacinPlacanja"] = new SelectList(Enum.GetValues(typeof(VrstaPlacanja)));
            return View();
        }

        // POST: Narudzba/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //uklonila sam Id,KorisnikId,RadnikId,StatusNarudzbe, u parametrima
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StatusNarudzbe,NacinPlacanja,NacinPreuzimanja,DatumNarudzbe,AdresaDostave")] Narudzba narudzba)
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
            narudzba.RadnikId = "f7974104-9ab3-4d73-a79b-85f21b1c9f68";
            narudzba.StatusNarudzbe = StatusNarudzbe.Potvrdjena;



            // Ukloni greske validacije za ova polja jer ih postavljam rucno
            ModelState.Remove("Korisnik");
            ModelState.Remove("Radnik");
            ModelState.Remove("KorisnikId");
            ModelState.Remove("RadnikId");
            if (ModelState.IsValid)
            {
                _context.Add(narudzba);
                await _context.SaveChangesAsync();
               // var korpa = await _context.Korpa.FirstOrDefaultAsync(k => k.KorisnikId == narudzba.KorisnikId);
               /* var stavkeUKorpi = await _context.StavkaKorpe
               .Where(s => s.KorpaId == korpa.Id)
               .ToListAsync();*/

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
                _context.StavkaKorpe.RemoveRange(stavkeUKorpi);
                await _context.SaveChangesAsync();

                if (narudzba.NacinPlacanja == VrstaPlacanja.Kartica)
                {
                    return RedirectToAction("Unos", "KarticnoPlacanje", new { id = narudzba.Id });
                }
                else
                {
                    return RedirectToAction("Index","Proizvod");
                }
               
            }
            
            ViewData["KorisnikId"] = new SelectList(_context.Users, "Id", "Email", narudzba.KorisnikId);
            ViewData["RadnikId"] = new SelectList(_context.Users, "Id", "Email", narudzba.RadnikId);
            ViewData["NacinPreuzimanja"] = new SelectList(Enum.GetValues(typeof(VrstaPreuzimanja)), narudzba.NacinPreuzimanja);
            ViewData["NacinPlacanja"] = new SelectList(Enum.GetValues(typeof(VrstaPlacanja)), narudzba.NacinPlacanja);
            return View(narudzba);
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
            /*
                        if (id != narudzba.Id)
                        {
                            //return NotFound();
                            return Content($"Test Ne postoji narudzba: {id}");
                        }

                        var korisnik = await _userManager.GetUserAsync(User);
                        if (korisnik == null)
                        {
                            return Redirect("/Identity/Account/Register");
                        }


                        var postojeca = await _context.Narudzba.FindAsync(id);
                        if (postojeca == null)
                        {
                            return NotFound();
                        }
            */
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

            // Ažurirajte samo polja koja su promijenjena
            /*if (postojeca.StatusNarudzbe != narudzba.StatusNarudzbe)
             {
                 postojeca.StatusNarudzbe = narudzba.StatusNarudzbe;
             }

             if (postojeca.NacinPlacanja != narudzba.NacinPlacanja)
             {
                 postojeca.NacinPlacanja = narudzba.NacinPlacanja;
             }

             if (postojeca.NacinPreuzimanja != narudzba.NacinPreuzimanja)
             {
                 postojeca.NacinPreuzimanja = narudzba.NacinPreuzimanja;
             }

             if (postojeca.DatumNarudzbe != narudzba.DatumNarudzbe)
             {
                 postojeca.DatumNarudzbe = narudzba.DatumNarudzbe;
             }

             // Za string polja, provjerite null i whitespace
             if (!string.IsNullOrWhiteSpace(narudzba.AdresaDostave) &&
                 postojeca.AdresaDostave != narudzba.AdresaDostave)
             {
                 postojeca.AdresaDostave = narudzba.AdresaDostave;
             }
            */
            // Ova polja uvijek postavite (ako su potrebna)
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
                    _context.Update(narudzba);
                    await _context.SaveChangesAsync();
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
