using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace K_K.Controllers
{
    public class ProizvodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProizvodController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Proizvod
        public async Task<IActionResult> Index(string searchTerm, string sortOrder, string tipProizvoda,
                                                string podkategorija, decimal? minPrice, decimal? maxPrice, string velicina)
        {
            var proizvodi = _context.Proizvod.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                proizvodi = proizvodi.Where(p => p.Naziv.Contains(searchTerm) || p.Opis.Contains(searchTerm));
            }
            if (tipProizvoda == "Hrana")
            {
                proizvodi = proizvodi.Where(p => p is Hrana);
            }
            else if (tipProizvoda == "Pice")
            {
                proizvodi = proizvodi.Where(p => p is Pice);
            }

            if (!string.IsNullOrEmpty(podkategorija))
            {
                switch (podkategorija.ToLower())
                {
                    case "pecivo":
                        proizvodi = proizvodi.Where(p => p is Hrana && ((Hrana)p).VrstaHrane == VrstaHrane.Pecivo);
                        break;
                    case "kolaci":
                        proizvodi = proizvodi.Where(p => p is Hrana && ((Hrana)p).VrstaHrane == VrstaHrane.Kolac);
                        break;
                    case "kafa":
                        proizvodi = proizvodi.Where(p => p is Pice && ((Pice)p).VrstaPica == VrstaPica.Kafa);
                        break;
                    case "caj":
                        proizvodi = proizvodi.Where(p => p is Pice && ((Pice)p).VrstaPica == VrstaPica.Caj);
                        break;
                    case "ostalo":
                        proizvodi = proizvodi.Where(p => p is Pice && ((Pice)p).VrstaPica == VrstaPica.Ostalo);
                        break;
                }
            }

            if (minPrice.HasValue)
            {
                proizvodi = proizvodi.Where(p => p.Cijena >= (double)minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                proizvodi = proizvodi.Where(p => p.Cijena <= (double)maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(velicina))
            {
                if (velicina.ToLower() == "mala")
                {
                    proizvodi = proizvodi.Where(p => p.Velicina == Velicina.Mala);
                }
                else if (velicina.ToLower() == "velika")
                {
                    proizvodi = proizvodi.Where(p => p.Velicina == Velicina.Velika);
                }
            }

            ViewBag.NazivSortParam = String.IsNullOrEmpty(sortOrder) ? "naziv_desc" : "";
            ViewBag.CijenaSortParam = sortOrder == "cijena" ? "cijena_desc" : "cijena";

            switch (sortOrder)
            {
                case "naziv_desc":
                    proizvodi = proizvodi.OrderByDescending(p => p.Naziv);
                    break;
                case "cijena":
                    proizvodi = proizvodi.OrderBy(p => p.Cijena);
                    break;
                case "cijena_desc":
                    proizvodi = proizvodi.OrderByDescending(p => p.Cijena);
                    break;
                default:
                    proizvodi = proizvodi.OrderBy(p => p.Naziv);
                    break;
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TipProizvoda = tipProizvoda;
            ViewBag.Podkategorija = podkategorija;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Velicina = velicina;


            return View(await proizvodi.ToListAsync());
        }


        // GET: Proizvod/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proizvod = await _context.Proizvod
                .FirstOrDefaultAsync(m => m.Id == id);
            if (proizvod == null)
            {
                return NotFound();
            }
            var recenzije = await _context.Recenzija.Where(r => r.ProizvodId == id)
                .Include(r => r.Korisnik)
                .Include(r => r.Narudzba)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            ViewBag.Recenzije = recenzije;
            ViewBag.ProsjecnaOcjena = recenzije.Any() ? recenzije.Average(r => r.Ocjena) : 0;
            ViewBag.BrojRecenzija = recenzije.Count;
            return View(proizvod);
        }

        // GET: Proizvod/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Proizvod/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,Velicina,Naziv,Opis,Slika,Cijena")] Proizvod proizvod,
                                                string tipProizvoda, VrstaHrane? vrstaHrane,
                                                VrstaPica? vrstaPica, IFormFile slikaFile)
        {
            ModelState.Remove("Slika");
            if (slikaFile != null && slikaFile.Length > 0)
            {
                try
                {
                    /*
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "proizvodi");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileExtension = Path.GetExtension(slikaFile.FileName);
                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await slikaFile.CopyToAsync(stream);
                    }

                    proizvod.Slika = fileName;
                    */
                    if (slikaFile.Length > 2 * 1024 * 1024) // 2MB
                    {
                        ModelState.AddModelError("", "Slika je prevelika. Maksimalno 2MB.");
                        return View(proizvod);
                    }

                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(slikaFile.ContentType.ToLower()))
                    {
                        ModelState.AddModelError("", "Nepodrzan format. Koristite JPG, PNG ili GIF.");
                        return View(proizvod);
                    }

                    // Konverzija u Base64
                    using (var memoryStream = new MemoryStream())
                    {
                        await slikaFile.CopyToAsync(memoryStream);
                        var imageBytes = memoryStream.ToArray();
                        var base64String = Convert.ToBase64String(imageBytes);
                        proizvod.Slika = $"data:{slikaFile.ContentType};base64,{base64String}";
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Greška pri upload-u slike: {ex.Message}");
                    return View(proizvod);
                }
            }
            if (string.IsNullOrEmpty(tipProizvoda))
            {
                ModelState.AddModelError("", "Molimo odaberite tip proizvoda.");
                return View(proizvod);
            }

            if (tipProizvoda == "Hrana" && !vrstaHrane.HasValue)
            {
                ModelState.AddModelError("", "Molimo odaberite vrstu hrane.");
                return View(proizvod);
            }

            if (tipProizvoda == "Pice" && !vrstaPica.HasValue)
            {
                ModelState.AddModelError("", "Molimo odaberite vrstu pića.");
                return View(proizvod);
            }

            if (ModelState.IsValid)
            {
                Proizvod finalProizvod = null;

                if (tipProizvoda == "Hrana" && vrstaHrane.HasValue)
                {
                    finalProizvod = new Hrana
                    {
                        Naziv = proizvod.Naziv,
                        Opis = proizvod.Opis,
                        Slika = proizvod.Slika,
                        Cijena = proizvod.Cijena,
                        VrstaHrane = vrstaHrane.Value,
                        Velicina = proizvod.Velicina
                    };
                }
                else if (tipProizvoda == "Pice" && vrstaPica.HasValue)
                {
                    finalProizvod = new Pice
                    {
                        Naziv = proizvod.Naziv,
                        Opis = proizvod.Opis,
                        Slika = proizvod.Slika,
                        Cijena = proizvod.Cijena,
                        VrstaPica = vrstaPica.Value,
                        Velicina = proizvod.Velicina
                    };
                }
                else
                {
                    ModelState.AddModelError("", "Molimo odaberite tip proizvoda i kategoriju.");
                    return View(proizvod);
                }
                _context.Add(finalProizvod);
                await _context.SaveChangesAsync();
                TempData["ProizvodKreiran"] = "Proizvod je uspješno kreiran.";
                return RedirectToAction(nameof(Index));
            }

            return View(proizvod);
        }

        // GET: Proizvod/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proizvod = await _context.Proizvod.FindAsync(id);
            if (proizvod == null)
            {
                return NotFound();
            }
            return View(proizvod);
        }

        // POST: Proizvod/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Velicina,Naziv,Opis,Slika,Cijena")] Proizvod proizvod, IFormFile slikaFile)
        {
            if (id != proizvod.Id)
            {
                return NotFound();
            }
            ModelState.Remove("slikaFile");

            if (slikaFile != null && slikaFile.Length > 0)
            {
                try
                {
                    /*//jednom
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "proizvodi");
                    Directory.CreateDirectory(uploadsFolder);

                    //brisanje stare
                    var originalProizvod = await _context.Proizvod.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (originalProizvod != null && !string.IsNullOrEmpty(originalProizvod.Slika) && originalProizvod.Slika != "default.jpg")
                    {
                        var oldFilePath = Path.Combine(uploadsFolder, originalProizvod.Slika);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // wwwroot/images/proizvodi/...
                    var fileExtension = Path.GetExtension(slikaFile.FileName);
                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    //nova sačuvana
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await slikaFile.CopyToAsync(stream);
                    }

                    proizvod.Slika = fileName
                    */
                    if (slikaFile.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "Slika je prevelika. Maksimalno 2MB.");
                        return View(proizvod);
                    }

                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(slikaFile.ContentType.ToLower()))
                    {
                        ModelState.AddModelError("", "Nepodrzan format. Koristite JPG, PNG ili GIF.");
                        return View(proizvod);
                    }

                    // Konverzija u Base64
                    using (var memoryStream = new MemoryStream())
                    {
                        await slikaFile.CopyToAsync(memoryStream);
                        var imageBytes = memoryStream.ToArray();
                        var base64String = Convert.ToBase64String(imageBytes);
                        proizvod.Slika = $"data:{slikaFile.ContentType};base64,{base64String}";
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Greška pri upload-u slike: {ex.Message}");
                }
            }
            else
            {
                //ako nema nove slike, zadržavamo staru
                var originalProizvod = await _context.Proizvod.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (originalProizvod != null)
                {
                    proizvod.Slika = originalProizvod.Slika;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proizvod);
                    await _context.SaveChangesAsync();
                    TempData["ProizvodAzuriran"] = "Proizvod je uspješno ažuriran!";
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
                return RedirectToAction(nameof(Index));
            }
            return View(proizvod);
        }

        // GET: Proizvod/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proizvod = await _context.Proizvod
                .FirstOrDefaultAsync(m => m.Id == id);
            if (proizvod == null)
            {
                return NotFound();
            }

            return View(proizvod);
        }

        // POST: Proizvod/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proizvod = await _context.Proizvod.FindAsync(id);
            if (proizvod != null)
            {
                _context.Proizvod.Remove(proizvod);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProizvodExists(int id)
        {
            return _context.Proizvod.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<JsonResult> Search(string term)
        {
            var proizvodi = await _context.Proizvod
                .Where(p => p.Naziv.Contains(term) || p.Opis.Contains(term))
                .Select(p => new { p.Id, p.Naziv, p.Cijena, p.Slika, p.Opis })
                .Take(10)
                .ToListAsync();

            return Json(proizvodi);
        }
    }
}