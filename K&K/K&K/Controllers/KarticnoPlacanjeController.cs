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
        // Prosledi ID narudzbe u view ako treba
        ViewBag.NarudzbaId = id;

        // Inicijalizuj model i prosledi u view
        var model = new KarticnoPlacanje { NarudzbaId = id };
        return View(model); // Ovo će tražiti view Unos.cshtml
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IzvrsiUplatu(KarticnoPlacanje placanje)
    {
        if (!ModelState.IsValid)
            return View("Index", placanje);

        var uspjeh = UspjesnostPlacanja(placanje);

        placanje.VrijemePlacanja = DateTime.Now;
        placanje.Uspjesno = uspjeh;

        _context.KarticnoPlacanjes.Add(placanje);
        await _context.SaveChangesAsync();

        ViewData["Poruka"] = uspjeh ? "Plaćanje je uspješno." : "Plaćanje nije uspjelo.";

        // Vrati isti view sa modelom i porukom
        return View("Index", placanje);
    }

    public bool UspjesnostPlacanja(KarticnoPlacanje placanje)
    {
        var narudzba = _context.Narudzba.Find(placanje.NarudzbaId);
        if (narudzba == null)
            return false;

        if (string.IsNullOrEmpty(placanje.BrojKartice) || placanje.BrojKartice.Length < 12 || placanje.BrojKartice.Length > 16)
            return false;

        if (!LuhnValidacija(placanje.BrojKartice))
            return false;

        if (string.IsNullOrEmpty(placanje.CVV) || placanje.CVV.Length != 3 || !placanje.CVV.All(char.IsDigit))
            return false;

        if (placanje.DatumIsteka < DateTime.Now)
            return false;

        return true;
    }

    private bool LuhnValidacija(string brojKartice)
    {
        int suma = 0;
        bool dupliraj = false;

        for (int i = brojKartice.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(brojKartice[i]))
                return false;

            int cifra = brojKartice[i] - '0';
            if (dupliraj)
            {
                cifra *= 2;
                if (cifra > 9)
                    cifra -= 9;
            }

            suma += cifra;
            dupliraj = !dupliraj;
        }

        return suma % 10 == 0;
    }
}
