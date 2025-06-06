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
        private readonly List<StavkaKorpe> stavke;

        public KorpaController(ApplicationDbContext context)
        {
            _context = context;
            stavke = new List<StavkaKorpe>();
        }

        // GET: Korpa
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Korpa.Include(k => k.Korisnik);
            return View(await applicationDbContext.ToListAsync());
            
        }
        public IActionResult KorpaView()
        {
            var korpa = new Korpa
            {
                Stavke = stavke,
                ukupnaCijena = stavke.Sum(x => x.Proizvod.Cijena * x.Kolicina)

            };
            return View(korpa);
        }
        
        public IActionResult dodajUKorpu(int id)
        {
            var dodaj = _context.Proizvod.Find(id);
            var postoji = stavke.FirstOrDefault(item => item.Proizvod.Id == id);
             if(postoji != null)
            {
                postoji.Kolicina++;
                
            }
            else
            {
                stavke.Add(new StavkaKorpe
                {
                    Proizvod = dodaj,
                    Kolicina = 1,
                    Cijena=dodaj.Cijena
                });
            }

               return RedirectToAction("KorpaView");
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
