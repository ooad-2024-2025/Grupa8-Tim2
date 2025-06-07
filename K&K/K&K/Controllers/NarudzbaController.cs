using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Controllers
{

    //[Authorize(Roles = "Administrator,User")]
    public class NarudzbaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NarudzbaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Narudzba
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Narudzba.Include(n => n.Korisnik).Include(n => n.Radnik);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Narudzba/Details/5
        public async Task<IActionResult> Details(int? id)
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
            double ukupnaCijena = IzracunajUkupnuCijenuNarudzbe(narudzba.Id);
            ViewBag.UkupnaCijena = ukupnaCijena;

            return View(narudzba);
        }

        // GET: Narudzba/Create
        public IActionResult Create()
        {
            ViewData["KorisnikId"] = new SelectList(_context.Osoba, "Id", "Email");
            ViewData["RadnikId"] = new SelectList(_context.Osoba, "Id", "Email");

            ViewData["NacinPreuzimanja"] = new SelectList(Enum.GetValues(typeof(VrstaPreuzimanja)));
            ViewData["NacinPlacanja"] = new SelectList(Enum.GetValues(typeof(VrstaPlacanja)));
            return View();
        }

        // POST: Narudzba/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,KorisnikId,RadnikId,StatusNarudzbe,NacinPlacanja,NacinPreuzimanja,DatumNarudzbe,AdresaDostave")] Narudzba narudzba)
        {
            if (ModelState.IsValid)
            {
                _context.Add(narudzba);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KorisnikId"] = new SelectList(_context.Osoba, "Id", "Email", narudzba.KorisnikId);
            ViewData["RadnikId"] = new SelectList(_context.Osoba, "Id", "Email", narudzba.RadnikId);
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
           
            ViewData["KorisnikId"] = new SelectList(_context.Osoba, "Id", "Email", narudzba.KorisnikId);
            ViewData["RadnikId"] = new SelectList(_context.Osoba, "Id", "Email", narudzba.RadnikId);
            ViewData["NacinPlacanja"] = new SelectList(Enum.GetValues(typeof(VrstaPlacanja)), narudzba.NacinPlacanja);

            return View(narudzba);
        }

        // POST: Narudzba/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KorisnikId,RadnikId,StatusNarudzbe,NacinPlacanja,NacinPreuzimanja,DatumNarudzbe,AdresaDostave")] Narudzba narudzba)
        {
            if (id != narudzba.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(narudzba);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["KorisnikId"] = new SelectList(_context.Osoba, "Id", "Email", narudzba.KorisnikId);
            ViewData["RadnikId"] = new SelectList(_context.Osoba, "Id", "Email", narudzba.RadnikId);
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

        public double IzracunajUkupnuCijenuNarudzbe(int narudzbaId)
        {
            var stavke = _context.StavkaNarudzbe
                .Where(s => s.NarudzbaId == narudzbaId)
                .ToList();

            var narudzba = _context.Narudzba.Find(narudzbaId);

            if (narudzba == null)
                throw new Exception("Narudžba nije pronađena.");

            double ukupno = stavke.Sum(s => s.Cijena * s.Kolicina);

            // Dodaj cijenu dostave ako je potrebno
            if (narudzba.NacinPreuzimanja == VrstaPreuzimanja.Dostava)
            {
                ukupno += 5.0; // fiksna cijena dostave
            }

            return ukupno;
        }
>>>>>>> cf77c554c283aa17c7c8eb98be8892ac50e8b120
    }
}
