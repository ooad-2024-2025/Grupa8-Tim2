using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace K_K.Controllers
{
    public class KarticnoPlacanjeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KarticnoPlacanjeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /KarticnoPlacanje/Unos
        public IActionResult Unos()
        {
            return View();
        }

        // POST: /KarticnoPlacanje/IzvrsiUplatu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IzvrsiUplatu(KarticnoPlacanje placanje)
        {
            if (!ModelState.IsValid)
                return View("Unos", placanje);

            var uspjeh = UspjesnostPlacanja(placanje);

            placanje.VrijemePlacanja = DateTime.Now;
            placanje.Uspjesno = uspjeh;

            _context.KarticnoPlacanjes.Add(placanje);
            await _context.SaveChangesAsync();

            TempData["Poruka"] = uspjeh ? "Plaćanje je uspješno." : "Plaćanje nije uspjelo.";
            return RedirectToAction("Potvrda");
        }

        // GET: /KarticnoPlacanje/Potvrda
        public IActionResult Potvrda()
        {
            ViewBag.Poruka = TempData["Poruka"];
            return View();
        }

        // Metoda za validaciju plaćanja
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
}
