using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class RecenzijaController : Controller
{
    private readonly ApplicationDbContext dataContext;

    public RecenzijaController(ApplicationDbContext context)
    {
        dataContext = context;
    }

    private async Task<Osoba> GetTrenutniKorisnikAsync()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (idClaim == null || !int.TryParse(idClaim, out int korisnikId))
            return null;

        return await dataContext.Osoba.FindAsync(korisnikId);
    }

    // Prikaz svih recenzija za proizvod
    public async Task<IActionResult> PrikaziRecenziju(int proizvodId)
    {
        var recenzije = await dataContext.Recenzija
            .Include(r => r.Korisnik)
            .Where(r => r.ProizvodId == proizvodId)
            .ToListAsync();

        return View("RecenzijeView", recenzije);
    }

    // GET: Forma za ostavljanje recenzije
    public async Task<IActionResult> OstaviRecenziju(int proizvodId)
    {
        var korisnik = await GetTrenutniKorisnikAsync();
        if (korisnik == null) return Unauthorized();

        var vecPostoji = await dataContext.Recenzija
            .AnyAsync(r => r.ProizvodId == proizvodId && r.KorisnikId == korisnik.Id);

        if (vecPostoji)
        {
            TempData["Poruka"] = "Već ste ostavili recenziju za ovaj proizvod.";
            return RedirectToAction("PrikaziRecenziju", new { proizvodId });
        }

        return View(new Recenzija { ProizvodId = proizvodId });
    }

    // POST: Snimi recenziju
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OstaviRecenziju(Recenzija recenzija)
    {
        var korisnik = await GetTrenutniKorisnikAsync();
        if (korisnik == null) return Unauthorized();

        recenzija.KorisnikId = korisnik.Id;

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
        var korisnik = await GetTrenutniKorisnikAsync();
        var recenzija = await dataContext.Recenzija.FindAsync(id);

        if (recenzija == null || korisnik == null)
            return NotFound();

        if (recenzija.KorisnikId != korisnik.Id)
            return Unauthorized();

        return View(recenzija);
    }

    // POST: Uredi recenziju
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UrediRecenziju(Recenzija r)
    {
        var korisnik = await GetTrenutniKorisnikAsync();
        var original = await dataContext.Recenzija.FindAsync(r.Id);

        if (original == null || korisnik == null)
            return NotFound();

        if (original.KorisnikId != korisnik.Id)
            return Unauthorized();

        original.Ocjena = r.Ocjena;

        await dataContext.SaveChangesAsync();
        return RedirectToAction("PrikaziRecenziju", new { proizvodId = original.ProizvodId });
    }

    // Brisanje recenzije
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IzbrisiRecenziju(int id)
    {
        var korisnik = await GetTrenutniKorisnikAsync();
        var recenzija = await dataContext.Recenzija.FindAsync(id);

        if (recenzija == null || korisnik == null)
            return NotFound();

        bool jeAdmin = korisnik.Uloga == Uloga.Admin;
        bool jeVlasnik = recenzija.KorisnikId == korisnik.Id;

        if (!jeAdmin && !jeVlasnik)
            return Unauthorized();

        dataContext.Recenzija.Remove(recenzija);
        await dataContext.SaveChangesAsync();

        return RedirectToAction("PrikaziRecenziju", new { proizvodId = recenzija.ProizvodId });
    }
}
