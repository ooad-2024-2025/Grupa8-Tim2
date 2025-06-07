// K_K/Controllers/ProizvodController.cs (Ovaj kod je isti kao u prethodnom odgovoru)

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace K_K.Controllers
{
    public class ProizvodController : Controller
    {
        private readonly ApplicationDbContext _dataContext;
        private readonly UserManager<IdentityUser> _userManager;

        public ProizvodController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _dataContext = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Proizvod.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proizvod = await _dataContext.Proizvod // Koristimo _dataContext.Proizvod
                                            .FirstOrDefaultAsync(m => m.Id == id);

            if (proizvod == null)
            {
                return NotFound();
            }

            // --- Logika za Recenzije ---
            var recenzije = await _dataContext.Recenzija
                .Include(r => r.Korisnik)
                .Where(r => r.ProizvodId == id)
                .OrderByDescending(r => r.DatumDodavanja)
                .ToListAsync();

            ViewBag.BrojRecenzija = recenzije.Count;
            ViewBag.Recenzije = recenzije;

            if (recenzije.Any())
            {
                ViewBag.ProsjecnaOcjena = recenzije.Average(r => r.Ocjena);
            }
            else
            {
                ViewBag.ProsjecnaOcjena = 0.0;
            }

            // --- Logika za dugme 'Napiši recenziju' ---
            bool mozeOstavitiRecenziju = false;
            int? narudzbaIdZaRecenziju = null;
            var korisnik = await _userManager.GetUserAsync(User);

            if (korisnik != null)
            {
                var korisnikJeVecRecenziraoOvajProizvod = await _dataContext.Recenzija
                    .AnyAsync(r => r.ProizvodId == id && r.KorisnikId == korisnik.Id);

                if (!korisnikJeVecRecenziraoOvajProizvod)
                {
                    var poslednjaNarudzbaKorisnika = await _dataContext.Narudzba
                                                        .Where(n => n.KorisnikId == korisnik.Id)
                                                        .OrderByDescending(n => n.DatumNarudzbe)
                                                        .FirstOrDefaultAsync();

                    if (poslednjaNarudzbaKorisnika != null)
                    {
                        mozeOstavitiRecenziju = true;
                        narudzbaIdZaRecenziju = poslednjaNarudzbaKorisnika.Id;
                    }
                }
            }

            ViewBag.MozeOstavitiRecenziju = mozeOstavitiRecenziju;
            ViewBag.NarudzbaId = narudzbaIdZaRecenziju;

            return View(proizvod);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Velicina,Naziv,Opis,Slika,Cijena")] Proizvod proizvod)
        {
            if (ModelState.IsValid)
            {
                _dataContext.Add(proizvod);
                await _dataContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Proizvod je uspješno dodan!";
                return RedirectToAction(nameof(Index));
            }
            return View(proizvod);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proizvod = await _dataContext.Proizvod.FindAsync(id);
            if (proizvod == null)
            {
                return NotFound();
            }
            return View(proizvod);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Velicina,Naziv,Opis,Slika,Cijena")] Proizvod proizvod)
        {
            if (id != proizvod.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dataContext.Update(proizvod);
                    await _dataContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProizvodExists(proizvod.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Proizvod je uspješno ažuriran!";
                return RedirectToAction(nameof(Index));
            }
            return View(proizvod);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proizvod = await _dataContext.Proizvod
                                            .FirstOrDefaultAsync(m => m.Id == id);
            if (proizvod == null)
            {
                return NotFound();
            }

            return View(proizvod);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proizvod = await _dataContext.Proizvod.FindAsync(id);
            if (proizvod != null)
            {
                _dataContext.Proizvod.Remove(proizvod);
            }

            await _dataContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Proizvod je uspješno obrisan!";
            return RedirectToAction(nameof(Index));
        }

        private bool ProizvodExists(int id)
        {
            return _dataContext.Proizvod.Any(e => e.Id == id);
        }
    }
}