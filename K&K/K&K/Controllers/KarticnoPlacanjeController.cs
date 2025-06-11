using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class KarticnoPlacanjeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Osoba> _userManager;

    public KarticnoPlacanjeController(ApplicationDbContext context, UserManager<Osoba> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View(new KarticnoPlacanje());
    }

    
    [HttpGet]
    public async Task<IActionResult> Unos(int id) // Primamo ID narudžbe
    {
        
        var narudzba = await _context.Narudzba.FindAsync(id);
        if (narudzba == null || narudzba.StatusNarudzbe != StatusNarudzbe.NaCekanju)
        {
            TempData["ErrorMessage"] = "Narudžba nije spremna za plaćanje ili ne postoji.";
            return RedirectToAction("Index", "Narudzba");
        }

        var model = new KarticnoPlacanje
        {
            NarudzbaId = id // Proslijedi ID narudžbe u model
        };
        await ObrisiStareNeplaceneNarudzbe(id); 
        return View(model);
    }
    private async Task ObrisiStareNeplaceneNarudzbe(int id)
    {
        try
        {
            //narudzbe starije od 30 min
            var cutoffTime = DateTime.Now.AddMinutes(-30);

            var stareNarudzbe = await _context.Narudzba
                .Where(n => n.StatusNarudzbe == StatusNarudzbe.NaCekanju &&
                            n.DatumNarudzbe < cutoffTime && n.Id != id)
                .ToListAsync();

            if (stareNarudzbe.Any())
            {
                foreach (var narudzba in stareNarudzbe)
                {
                    
                    var stavke = await _context.StavkaNarudzbe
                        .Where(s => s.NarudzbaId == narudzba.Id)
                        .ToListAsync();
                    _context.StavkaNarudzbe.RemoveRange(stavke);

                    _context.Narudzba.Remove(narudzba);
                }

                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška pri brisanju starih narudžbi: {ex.Message}");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unos([Bind("Id,BrojKartice,CVV,DatumIsteka,NarudzbaId")] KarticnoPlacanje karticnoPlacanje)
    {
        if (ModelState.IsValid)
        {
            // Simulacija uspješnog plaćanja (ovdje bi inače bila integracija s payment gateway-om)
            // Ako je plaćanje uspješno:
            var narudzba = await _context.Narudzba.FindAsync(karticnoPlacanje.NarudzbaId);
            if (narudzba != null)
            {
                narudzba.StatusNarudzbe = StatusNarudzbe.Potvrdjena; // Ažuriraj status narudžbe
                _context.Update(narudzba); // Ažuriraj narudžbu u bazi
                await _context.SaveChangesAsync();

                // Sada kada je plaćanje uspješno, obriši stavke iz korpe
                var korisnik = await _userManager.GetUserAsync(User);
                var korpa = await _context.Korpa.FirstOrDefaultAsync(k => k.KorisnikId == korisnik.Id);
                if (korpa != null)
                {
                    var stavkeUKorpi = await _context.StavkaKorpe
                        .Where(s => s.KorpaId == korpa.Id)
                        .ToListAsync();
                    _context.StavkaKorpe.RemoveRange(stavkeUKorpi);
                    await _context.SaveChangesAsync();
                }

                // Dodaj podatke o kartičnom plaćanju u bazu ako želite čuvati te transakcije
                _context.Add(karticnoPlacanje);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Uspješno ste platili narudžbu!";
                return RedirectToAction("Uspjeh"); // Preusmjeri na Uspjeh akciju unutar KarticnoPlacanjeController-a
            }
            else
            {
                ModelState.AddModelError("", "Greška: Narudžba nije pronađena.");
            }
        }

        // Ako dođe do ovdje, znači da je validacija neuspješna ili je narudžba null
        return View(karticnoPlacanje);
    }

    public IActionResult Uspjeh()
    {
        // Prikazuje poruku o uspješnom kartičnom plaćanju
        return View();
    }


    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IzvrsiUplatu(KarticnoPlacanje placanje)
    {
        // Debug ispis svih validacijskih grešaka
        foreach (var entry in ModelState)
        {
            var field = entry.Key;
            var errors = entry.Value.Errors;
            foreach (var error in errors)
            {
                Console.WriteLine($"[VALIDACIJA] Polje: {field}, Greška: {error.ErrorMessage}");
            }
        }

        
            // ... (prvi dio koda, uključujući provjeru ModelState.IsValid za Data Annotations) ...

            if (!ModelState.IsValid)
            {
                ViewBag.NarudzbaId = placanje.NarudzbaId;
                return View("Unos", placanje); // Vraćanje na formu ako Data Annotations nisu prošle
            }

            // --- OVDJE JE KLJUČNI DIO ZA PRIKAZ GREŠKE NA EKRANU ---

            string specificnaGreska = null; // Promjenljiva za poruku o grešci na ekranu

            var narudzba = _context.Narudzba.Find(placanje.NarudzbaId);
            if (narudzba == null)
            {
                specificnaGreska = "Narudžba sa datim ID-jem ne postoji. Molimo provjerite podatke.";
            }
            else if (string.IsNullOrEmpty(placanje.BrojKartice) || placanje.BrojKartice.Length < 12 || placanje.BrojKartice.Length > 16)
            {
                specificnaGreska = "Broj kartice mora imati između 12 i 16 cifara. Molimo ispravite.";
            }
            else if (!LuhnValidacija(placanje.BrojKartice))
            {
                specificnaGreska = "Broj kartice nije validan (Luhn provjera neuspješna). Unesite ispravan broj.";
            }
            else if (string.IsNullOrEmpty(placanje.CVV) || placanje.CVV.Length != 3 || !placanje.CVV.All(char.IsDigit))
            {
                specificnaGreska = "CVV mora imati tačno 3 cifre i biti numerički.";
            }
            else if (placanje.DatumIsteka < DateTime.Now)
            {
                specificnaGreska = "Datum isteka kartice je u prošlosti. Molimo unesite budući datum.";
            }

            // Ako je specificnaGreska postavljena, to znači da postoji greška u poslovnoj logici
            if (specificnaGreska != null)
            {
                // Dodajemo ovu poruku u ModelState, što je način da je proslijedimo View-u
                // string.Empty znači da se greška odnosi na cijelu formu, ne na specifično polje
                ModelState.AddModelError(string.Empty, specificnaGreska);

                ViewBag.NarudzbaId = placanje.NarudzbaId;
                return View("Unos", placanje); // Vrati se na formu sa dodatom greškom
            }

            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik != null)
            {
                var korpa = await _context.Korpa.FirstOrDefaultAsync(k => k.KorisnikId == korisnik.Id);//nadji mu korpu pa brisi iz nje sve
                if (korpa != null)
                {
                    var stavkeUKorpi = await _context.StavkaKorpe
                        .Where(s => s.KorpaId == korpa.Id)
                        .ToListAsync();

                    if (stavkeUKorpi.Any())
                    {
                        _context.StavkaKorpe.RemoveRange(stavkeUKorpi);

                        korpa.brojProizvoda = 0;
                        korpa.ukupnaCijena = 0;
                        _context.Update(korpa);
                    }
                }
            }

            if (narudzba != null)
            {
                narudzba.StatusNarudzbe = StatusNarudzbe.Potvrdjena;
                _context.Update(narudzba);
            }

        // ... (ostatak koda ako je plaćanje uspješno: snimanje u bazu, preusmjeravanje na Uspjeh) ...
            placanje.VrijemePlacanja = DateTime.Now;
            placanje.Uspjesno = true;

            _context.KarticnoPlacanjes.Add(placanje);
            await _context.SaveChangesAsync();

            TempData["Poruka"] = "Plaćanje je uspješno izvršeno.";
            return RedirectToAction("Uspjeh");
        
    }


    public bool UspjesnostPlacanja(KarticnoPlacanje placanje)
    {
        var narudzba = _context.Narudzba.Find(placanje.NarudzbaId);
        if (narudzba == null)
        {
            Console.WriteLine("Narudžba nije pronađena.");
            return false;
        }

        if (string.IsNullOrEmpty(placanje.BrojKartice) || placanje.BrojKartice.Length < 12 || placanje.BrojKartice.Length > 16)
        {
            Console.WriteLine("Neispravan broj kartice.");
            return false;
        }

        if (!LuhnValidacija(placanje.BrojKartice))
        {
            Console.WriteLine("Luhn validacija nije prošla.");
            return false;
        }

        if (string.IsNullOrEmpty(placanje.CVV) || placanje.CVV.Length != 3 || !placanje.CVV.All(char.IsDigit))
        {
            Console.WriteLine("Neispravan CVV.");
            return false;
        }

        if (placanje.DatumIsteka < DateTime.Now)
        {
            Console.WriteLine("Kartica je istekla.");
            return false;
        }


        return true;
    }

    private bool LuhnValidacija(string brojKartice)
    {
        // Provjera da li je broj kartice prazan ili null na početku
        if (string.IsNullOrEmpty(brojKartice))
            return false;

        int suma = 0;
        // Ključna promjena: Inicijaliziramo 'dupliraj' na false.
        // To znači da prva cifra s desna (index: brojKartice.Length - 1) NEĆE biti duplirana.
        // Sljedeća cifra (pretposljednja) HOĆE biti duplirana.
        bool dupliraj = false;

        // Iteriramo od zadnje cifre prema prvoj (s desna na lijevo)
        for (int i = brojKartice.Length - 1; i >= 0; i--)
        {
            // Provjeravamo da li je karakter cifra
            if (!char.IsDigit(brojKartice[i]))
                return false; // Ako nije cifra, broj kartice je neispravan

            // Pretvaramo karakter u int cifru
            int cifra = brojKartice[i] - '0'; // Brz način pretvaranja 'char' cifre u 'int'

            // Ako je 'dupliraj' true, onda dupliramo cifru
            if (dupliraj)
            {
                cifra *= 2; // Dupliramo cifru

                // Ako je duplirana cifra veća od 9, oduzimamo 9 (isto kao zbrajanje cifara)
                // Npr. 14 -> 1+4 = 5. Ili 14 - 9 = 5. Oba načina rade.
                if (cifra > 9)
                    cifra -= 9;
            }

            // Dodajemo (originalnu ili dupliranu i smanjenu) cifru u sumu
            suma += cifra;

            // Mijenjamo stanje 'dupliraj' za sljedeću iteraciju
            dupliraj = !dupliraj;
        }

        // Na kraju, provjeravamo da li je ukupna suma djeljiva sa 10
        return suma % 10 == 0;
    }
    

    public IActionResult Greska()
    {
        return View();
    }


}
