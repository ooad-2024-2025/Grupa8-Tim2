using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_K.Data;
using K_K.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

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

        // GET: LokacijaKafica/Map - Nova funkcionalnost za mapu
        public async Task<IActionResult> Map()
        {
            var lokacije = await _context.LokacijaKafica.ToListAsync();
            return View(lokacije);
        }

        // API endpoint za dobijanje lokacija u JSON formatu

        [HttpGet]
        public async Task<IActionResult> GetLokacije()
        {
            // Debug log
            System.Diagnostics.Debug.WriteLine($"GetLokacije called at: {DateTime.Now:HH:mm:ss.fff}");
            Console.WriteLine($"GetLokacije called at: {DateTime.Now:HH:mm:ss.fff}");

            var lokacije = await _context.LokacijaKafica
                .Select(l => new {
                    id = l.Id,
                    adresa = l.Adresa,
                    grad = l.Grad,
                    lat = l.GeografskaSirina,
                    lng = l.GeografskaDuzina
                })
                .ToListAsync();

            System.Diagnostics.Debug.WriteLine($"Returning {lokacije.Count} locations");
            Console.WriteLine($"Returning {lokacije.Count} locations");

            return Json(lokacije);
        }


        [HttpPost]
        public async Task<IActionResult> NajblizaLokacija([FromBody] KoordinateModel koordinate)
        {
            var lokacije = await _context.LokacijaKafica.ToListAsync();

            if (!lokacije.Any())
            {
                return Json(new { success = false, message = "Nema dostupnih lokacija" });
            }

            var najbliza = lokacije
                .Select(l => new {
                    lokacija = l,
                    udaljenost = IzracunajUdaljenost(
                        koordinate.Latitude, koordinate.Longitude,
                        l.GeografskaSirina, l.GeografskaDuzina)
                })
                .OrderBy(x => x.udaljenost)
                .First();

            return Json(new
            {
                success = true,
                lokacija = new
                {
                    id = najbliza.lokacija.Id,
                    adresa = najbliza.lokacija.Adresa,
                    grad = najbliza.lokacija.Grad,
                    lat = najbliza.lokacija.GeografskaSirina,
                    lng = najbliza.lokacija.GeografskaDuzina,
                    udaljenost = Math.Round(najbliza.udaljenost, 2)
                }
            });
        }


        private double IzracunajUdaljenost(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radijus Zemlje u kilometrima
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }


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

        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
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

        [Authorize(Roles = "Administrator")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
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

        [Authorize(Roles = "Administrator")]
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
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


    public class KoordinateModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}