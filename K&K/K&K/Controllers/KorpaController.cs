using K_K.Data;
using K_K.Models;
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
    public class KorpaController : Controller
    {
        private readonly ApplicationDbContext _context;
        

        public KorpaController(ApplicationDbContext context)
        {
            _context = context;
            
        }

        // GET: Korpa
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Korpa.Include(k => k.Korisnik);

            // return View(await applicationDbContext.ToListAsync());
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int korisnikId = int.Parse(userId);
            /*var korpa = await _context.Korpa
                .Include(k => k.Korisnik)
                .FirstOrDefaultAsync(k => k.KorisnikId == korisnikId);*/
            return View(await applicationDbContext.ToListAsync());
            
        }
        /*
        public async Task<IActionResult> KorpaView()
        {
            var user = await _context.Osoba.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var korpa = await _context.Korpa.FirstOrDefaultAsync(c => c.KorisnikId == user.Id );
            if (korpa == null || !_context.Korpa.Any())
            {
                // Cart is empty or null, display a popup or redirect to a specific page
                TempData["CartMessage"] = "Korpa je trenutno prazna!";
                return RedirectToAction("PraznaKorpa");
            }
            var productCarts = _context.Korpa
    .Include(k => k.Stavke)
        .ThenInclude(s => s.Proizvod)
    .Where(k => k.Id == korpa.Id)
    .ToList();
            return View(productCarts);
        }*/
        public async Task<IActionResult> KorpaView()
        {
            var username = User.Identity?.Name;

            System.Diagnostics.Debug.WriteLine($"User.Identity.Name = {username}");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Osoba.FirstOrDefaultAsync(u => u.Email == username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            /*var korpa = await _context.Korpa
                .Include(k => k.Stavke)
                    .ThenInclude(s => s.Proizvod)
                .FirstOrDefaultAsync(c => c.KorisnikId == user.Id); 

            if (korpa == null || korpa.Stavke == null || !korpa.Stavke.Any())
            {
                TempData["CartMessage"] = "Korpa je trenutno prazna!";
                return RedirectToAction("PraznaKorpa");
            }*/

            return View(); //bilo u view korpa 
        }

        public async Task<IActionResult> DodajUKorpu(int proizvodId)
        { /*
            try
            {
                // Dobij korisnički ID
                var korisnikId = await GetKorisnikId();
                if (!korisnikId.HasValue)
                {
                    return RedirectToAction("Login");
               }

                // Pronađi proizvod
                var proizvod = await _context.Proizvod.FirstOrDefaultAsync(m => m.Id ==proizvodId);
                if (proizvod == null)
                {
                    return NotFound("Proizvod nije pronađen");
                }

                // Pronađi ili kreiraj korpu
                var korpa = await _context.Korpa
                    .Include(k => k.Stavke)
                    .FirstOrDefaultAsync(k => k.KorisnikId == korisnikId.Value);

                if (korpa == null)
                {
                    korpa = new Korpa
                    {
                        KorisnikId = korisnikId.Value,
                        brojProizvoda = 0,
                        ukupnaCijena = 0,
                        Stavke = new List<StavkaKorpe>()
                    };
                    _context.Korpa.Add(korpa);
                    await _context.SaveChangesAsync(); // Sačuvaj da dobiješ ID
                }

                // Osiguraj da Stavke nisu null
                if (korpa.Stavke == null)
                    korpa.Stavke = new List<StavkaKorpe>();

                // Provjeri da li stavka već postoji
                var postojecaStavka = korpa.Stavke.FirstOrDefault(s => s.ProizvodId == proizvodId);

                if (postojecaStavka != null)
                {
                    // Uvećaj količinu postojeće stavke
                    postojecaStavka.Kolicina++;
                    postojecaStavka.Cijena = postojecaStavka.Kolicina * proizvod.Cijena;
                }
                else
                {
                    // Kreiraj novu stavku
                    var novaStavka = new StavkaKorpe
                    {
                        KorpaId = korpa.Id,
                        ProizvodId = proizvodId,
                        Kolicina = 1,
                        Cijena = proizvod.Cijena
                    };
                    _context.StavkaKorpe.Add(novaStavka);
                    korpa.Stavke.Add(novaStavka); // Dodaj u lokalnu kolekciju
                }

                // Izračunaj totale
                korpa.brojProizvoda = korpa.Stavke.Sum(s => s.Kolicina);
                korpa.ukupnaCijena = korpa.Stavke.Sum(s => s.Cijena);

                await _context.SaveChangesAsync();

                return RedirectToAction("KorpaView", new { korisnikId = korisnikId.Value });
            }
            catch (Exception ex)
            {
                // Detaljnije logovanje
                Console.WriteLine($"Greška pri dodavanju u korpu: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Greška pri dodavanju u korpu");
            }
            */
            var applicationDbContext = _context.Korpa.Include(k => k.Korisnik);
            return View(await applicationDbContext.ToListAsync());
        }

      
        [HttpPost]
        public async Task<IActionResult> PromijeniKolicinu(int korpaId, int proizvodId, int kolicina)
        {
            var stavka = await _context.StavkaKorpe
                .FirstOrDefaultAsync(s => s.KorpaId == korpaId && s.ProizvodId == proizvodId);

            if (stavka != null)
            {
                var proizvod = await _context.Proizvod.FindAsync(proizvodId);
                stavka.Kolicina = kolicina;
                stavka.Cijena = kolicina * proizvod.Cijena;

                await _context.SaveChangesAsync();
                await AzurirajKorpu(korpaId);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UkloniStavku(int korpaId, int proizvodId)
        {
            var stavka = await _context.StavkaKorpe
                .FirstOrDefaultAsync(s => s.KorpaId == korpaId && s.ProizvodId == proizvodId);

            if (stavka != null)
            {
                _context.StavkaKorpe.Remove(stavka);
                await _context.SaveChangesAsync();
                await AzurirajKorpu(korpaId);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> OcistiKorpu(int korisnikId)
        {
            /*var korpa = await _context.Korpa
                .Include(k => k.Stavke)
                .FirstOrDefaultAsync(k => k.KorisnikId == korisnikId);

            if (korpa != null)
            {
                _context.StavkaKorpe.RemoveRange(korpa.Stavke);
                korpa.brojProizvoda = 0;
                korpa.ukupnaCijena = 0;
                await _context.SaveChangesAsync();
            }*/

            return Ok();
        }

        private async Task AzurirajKorpu(int korpaId)
        {
            var korpa = await _context.Korpa.FindAsync(korpaId);
            if (korpa != null)
            {
                var ukupnaKolicina = await _context.StavkaKorpe
                    .Where(s => s.KorpaId == korpaId)
                    .SumAsync(s => s.Kolicina);
                var ukupnaCijena = await _context.StavkaKorpe
                    .Where(s => s.KorpaId == korpaId)
                    .SumAsync(s => s.Cijena);

                korpa.brojProizvoda = ukupnaKolicina;
                korpa.ukupnaCijena = ukupnaCijena;
                await _context.SaveChangesAsync();
            }
        }

        // Pomoćna metoda za uklanjanje stavke
        public async Task<IActionResult> UkloniIzKorpe(int stavkaId, int korisnikId)
        {
            var stavka = await _context.StavkaKorpe.FindAsync(stavkaId);
            if (stavka != null)
            {
                _context.StavkaKorpe.Remove(stavka);

                // Ažuriraj korpu
                var korpa = await _context.Korpa.FindAsync(stavka.KorpaId);
                if (korpa != null)
                {
                    var ukupnaKolicina = await _context.StavkaKorpe
                        .Where(s => s.KorpaId == korpa.Id && s.Id != stavkaId)
                        .SumAsync(s => s.Kolicina);

                    var ukupnaCijena = await _context.StavkaKorpe
                        .Where(s => s.KorpaId == korpa.Id && s.Id != stavkaId)
                        .SumAsync(s => s.Cijena);

                    korpa.brojProizvoda = ukupnaKolicina;
                    korpa.ukupnaCijena = ukupnaCijena;
                    _context.Update(korpa);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("KorpaView", new { korisnikId = korisnikId });
        }

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
    }
}
