using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Controllers
{
    public class RecenzijaController : Controller
    {
        private readonly ApplicationDbContext dataContext;
        private readonly UserManager<IdentityUser> _userManager;

        public RecenzijaController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            dataContext = context;
            _userManager = userManager;
        }

        // GET: Prikaz svih recenzija za proizvod
        public async Task<IActionResult> PrikaziRecenziju(int proizvodId)
        {
            var recenzije = await dataContext.Recenzija
                .Include(r => r.Korisnik)
                .Where(r => r.ProizvodId == proizvodId)
                .ToListAsync();

            ViewData["ProizvodId"] = proizvodId;
            return View("RecenzijeView", recenzije);
        }

        // GET: Forma za ostavljanje recenzije
        public async Task<IActionResult> OstaviRecenziju(int proizvodId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
                return Unauthorized();

            return View(new Recenzija { ProizvodId = proizvodId });
        }

        // POST: Snimi recenziju
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OstaviRecenziju(Recenzija recenzija)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
                return Unauthorized();

            recenzija.KorisnikId = korisnik.Id;
            recenzija.DatumDodavanja = System.DateTime.Now;

            // Provera da li korisnik već ostavio recenziju za ovaj proizvod
            var vecPostoji = await dataContext.Recenzija
                .AnyAsync(r => r.ProizvodId == recenzija.ProizvodId && r.KorisnikId == korisnik.Id);

            if (vecPostoji)
            {
                TempData["Poruka"] = "Već ste ostavili recenziju za ovaj proizvod.";
                return RedirectToAction("PrikaziRecenziju", new { proizvodId = recenzija.ProizvodId });
            }

            if (ModelState.IsValid)
            {
                dataContext.Recenzija.Add(recenzija);
                await dataContext.SaveChangesAsync();
                return RedirectToAction("PrikaziRecenziju", new { proizvodId = recenzija.ProizvodId });
            }

            return View(recenzija);
        }

        // GET: Uredi recenziju
        public async Task<IActionResult> UrediRecenziju(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var recenzija = await dataContext.Recenzija.FindAsync(id);

            if (recenzija == null || korisnik == null)
                return NotFound();

            // Dozvoli uređivanje samo sopstvene recenzije
            if (recenzija.KorisnikId != korisnik.Id)
                return Unauthorized();

            return View(recenzija);
        }

        // POST: Uredi recenziju
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UrediRecenziju(Recenzija r)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var original = await dataContext.Recenzija.FindAsync(r.Id);

            if (original == null || korisnik == null)
                return NotFound();

            if (original.KorisnikId != korisnik.Id)
                return Unauthorized();

            original.Ocjena = r.Ocjena;
            original.Tekst = r.Tekst;
            await dataContext.SaveChangesAsync();

            return RedirectToAction("PrikaziRecenziju", new { proizvodId = original.ProizvodId });
        }

        // POST: Brisanje recenzije
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IzbrisiRecenziju(int id)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            var recenzija = await dataContext.Recenzija.FindAsync(id);

            if (recenzija == null)
                return NotFound();

            // Dozvoli brisanje samo sopstvene recenzije (ili dodaš admin proveru)
            if (recenzija.KorisnikId != korisnik.Id)
                return Unauthorized();

            dataContext.Recenzija.Remove(recenzija);
            await dataContext.SaveChangesAsync();

            return RedirectToAction("PrikaziRecenziju", new { proizvodId = recenzija.ProizvodId });
        }
    }
}
