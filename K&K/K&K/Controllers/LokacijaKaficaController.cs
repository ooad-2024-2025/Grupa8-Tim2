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
    public class LokacijaKaficaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LokacijaKaficaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LokacijaKafica
        public async Task<IActionResult> Index()
        {
            return View(await _context.LokacijaKafica.ToListAsync());
        }

        // GET: LokacijaKafica/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lokacijaKafica = await _context.LokacijaKafica
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lokacijaKafica == null)
            {
                return NotFound();
            }

            return View(lokacijaKafica);
        }

        // GET: LokacijaKafica/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LokacijaKafica/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Adresa,Grad,GeografskaSirina,GeografskaDuzina")] LokacijaKafica lokacijaKafica)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lokacijaKafica);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(lokacijaKafica);
        }

        // GET: LokacijaKafica/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lokacijaKafica = await _context.LokacijaKafica.FindAsync(id);
            if (lokacijaKafica == null)
            {
                return NotFound();
            }
            return View(lokacijaKafica);
        }

        // POST: LokacijaKafica/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Adresa,Grad,GeografskaSirina,GeografskaDuzina")] LokacijaKafica lokacijaKafica)
        {
            if (id != lokacijaKafica.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lokacijaKafica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LokacijaKaficaExists(lokacijaKafica.Id))
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
            return View(lokacijaKafica);
        }

        // GET: LokacijaKafica/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lokacijaKafica = await _context.LokacijaKafica
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lokacijaKafica == null)
            {
                return NotFound();
            }

            return View(lokacijaKafica);
        }

        // POST: LokacijaKafica/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lokacijaKafica = await _context.LokacijaKafica.FindAsync(id);
            if (lokacijaKafica != null)
            {
                _context.LokacijaKafica.Remove(lokacijaKafica);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LokacijaKaficaExists(int id)
        {
            return _context.LokacijaKafica.Any(e => e.Id == id);
        }
    }
}
