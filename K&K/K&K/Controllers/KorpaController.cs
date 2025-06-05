using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_K.Data;
using K_K.Models;

namespace K_K.Controllers
{
    public class KorpaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly List<StavkaKorpe> listaStavki;

        public KorpaController(ApplicationDbContext context)
        {
            _context = context;
        }
        /*
        [HttpPost]
        public async Task<IActionResult> dodajUkorpu(int proizvodId)
        {
            // Dobavi korisnika iz baze na osnovu trenutnog User.Identity.Name (email ili username)
             var korisnik = await _context.Osoba.FirstOrDefaultAsync(u => u.Email == User.Identity.Name); 
            //var korisnik = await _context.Osoba.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            if (korisnik == null)
            {
             return Unauthorized(); // ili neka greška ako korisnik nije ulogovan
            }
            

            var korpa = await _context.Korpa
                .Include(k => k.Stavke)
                .FirstOrDefaultAsync(k => k.KorisnikId == korisnik.Id && !k.Kupljeno);

            if (korpa == null)
            {
                korpa = new Korpa
                {
                    KorisnikId = korisnik.Id,
                    Stavke = new List<StavkaKorpe>(),
                    Kupljeno = false
                   
                };
                _context.Korpa.Add(korpa);
                await _context.SaveChangesAsync(); // Save da dobiješ korpa.Id
            }

            var stavka = korpa.Stavke.FirstOrDefault(s => s.ProizvodId == proizvodId);
            if (stavka != null)
            {
                stavka.Kolicina++;
            }
            else
            {
                var proizvod = await _context.Proizvod.FindAsync(proizvodId);
                if (proizvod == null)
                {
                    return NotFound(); // Proizvod ne postoji
                }
                korpa.Stavke.Add(new StavkaKorpe
                {
                    ProizvodId = proizvodId,
                    Kolicina = 1,
                    Cijena = proizvod.Cijena
                });
            }

            await _context.SaveChangesAsync();
             return RedirectToAction("Index", "Proizvod");
            
        }
        *
    //pregled korpe za ulogovanog korisnika
        public async Task<IActionResult> Pregled(int korisnikId)
        {
            var korpa = await _context.Korpa
                .Include(k => k.Stavke)
                .ThenInclude(s => s.Proizvod)
                .FirstOrDefaultAsync(k => k.KorisnikId == korisnikId);

            if (korpa == null)
                return NotFound();

            return View(korpa);
        }
        //ovo mozda ne treba
        */
        // GET: Korpa
        public async Task<IActionResult> Index()
        {

            
            var user = await _context.Osoba.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var korpa = await _context.Korpa.FirstOrDefaultAsync(k => k.KorisnikId == user.Id && !k.Kupljeno);

            if (korpa == null)
            {
                TempData["Poruka"] = "Korpa je trenutno prazna!";
                return RedirectToAction("PraznaKorpa");
            }

            var stavke = await _context.StavkaKorpe
                .Include(sk => sk.Proizvod)
                .Where(sk => sk.KorpaId == korpa.Id)
                .ToListAsync();

            if (!stavke.Any())
            {
                TempData["Poruka"] = "Korpa je trenutno prazna!";
                return RedirectToAction("PraznaKorpa");
            }
             
            return View(stavke);
        }
        

        public IActionResult PraznaKorpa()
        {
            ViewBag.Poruka = TempData["Poruka"];
            return View();
        }

        // mozda treba izbrisati jer pregled korpe treba omoguciti samo po id
        // GET: Korpa/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korpa = await _context.Korpa
                .Include(k => k.Korisnik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (korpa == null)
            {
                return NotFound();
            }

            return View(korpa);
        }


        // GET: Korpa/Create
        public IActionResult Create()
        {
            ViewData["KorisnikId"] = new SelectList(_context.Osoba, "Id", "Email");

            // Nabavi proizvode iz baze za prikaz u formi
            var proizvodi = _context.Proizvod.ToList();
            ViewData["Proizvodi"] = new SelectList(proizvodi, "Id", "Naziv");
            return View();
        }

        // POST: Korpa/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,KorisnikId")] Korpa korpa)
        {
            if (ModelState.IsValid)
            {
                _context.Add(korpa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var proizvodi = _context.Proizvod.ToList();
            ViewData["Proizvodi"] = new SelectList(proizvodi, "Id", "Naziv");
            ViewData["KorisnikId"] = new SelectList(_context.Osoba, "Id", "Email", korpa.KorisnikId);
            return View(korpa);
        }

        // GET: Korpa/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korpa = await _context.Korpa.FindAsync(id);
            if (korpa == null)
            {
                return NotFound();
            }
            ViewData["KorisnikId"] = new SelectList(_context.Osoba, "Id", "Email", korpa.KorisnikId);
            return View(korpa);
        }

        // POST: Korpa/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KorisnikId")] Korpa korpa)
        {
            if (id != korpa.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(korpa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KorpaExists(korpa.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["KorisnikId"] = new SelectList(_context.Osoba, "Id", "Email", korpa.KorisnikId);
            return View(korpa);
        }

        // GET: Korpa/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korpa = await _context.Korpa
                .Include(k => k.Korisnik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (korpa == null)
            {
                return NotFound();
            }

            return View(korpa);
        }

        // POST: Korpa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var korpa = await _context.Korpa.FindAsync(id);
            if (korpa != null)
            {
                _context.Korpa.Remove(korpa);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KorpaExists(int id)
        {
            return _context.Korpa.Any(e => e.Id == id);
        }
        /*
        public async Task<IActionResult> EditStavka(int? id)
        {
            if (id == null)
                return NotFound();

            var stavka = await _context.StavkaKorpe
                .Include(s => s.Proizvod)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stavka == null)
                return NotFound();

            return View(stavka);
        }

        // POST: Korpa/EditStavka/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStavka(int id, [Bind("Id,Kolicina,ProizvodId, KorpaId")] StavkaKorpe stavka)
        {
            if (id != stavka.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stavka);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StavkaKorpeExists(stavka.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(stavka);
        }

        private bool StavkaKorpeExists(int id)
        {
            return _context.StavkaKorpe.Any(e => e.Id == id);
        }*/

    }
}

