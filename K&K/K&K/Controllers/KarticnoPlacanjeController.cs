using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Mvc;

public class KarticnoPlacanjeController : Controller
{
    private readonly ApplicationDbContext _context;

    public KarticnoPlacanjeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View(new KarticnoPlacanje());
    }

   
    [HttpGet]
    public IActionResult Unos(int id)
    {
        ViewBag.NarudzbaId = id;

        var model = new KarticnoPlacanje
        {
            NarudzbaId = id
        };

        return View(new KarticnoPlacanje { NarudzbaId = id });
    }
    /* 
     [HttpPost]
     [ValidateAntiForgeryToken]
     public async Task<IActionResult> IzvrsiUplatu(KarticnoPlacanje placanje)
     {
         if (!ModelState.IsValid)
         {
             TempData["Poruka"] = "Model nije validan.";
             return RedirectToAction("Greska");
         }

         var uspjeh = UspjesnostPlacanja(placanje);

         placanje.VrijemePlacanja = DateTime.Now;
         placanje.Uspjesno = uspjeh;

         _context.KarticnoPlacanjes.Add(placanje);
         await _context.SaveChangesAsync();

         if (uspjeh)
         {
             ViewData["Poruka"] = "Plaćanje je uspješno izvršeno.";
             return RedirectToAction("Uspjeh");
         }
         else
         {
             TempData["Poruka"] = "Greška u validaciji podataka.";
             return RedirectToAction("Greska");
         }
     }
     */
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
    public IActionResult Uspjeh()
    {
        return View();
    }

    public IActionResult Greska()
    {
        return View();
    }


}
