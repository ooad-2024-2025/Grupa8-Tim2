using K_K.Data;
using K_K.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace K_K.Controllers
{
    
    public class KorpaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Osoba> _userManager;


        public KorpaController(ApplicationDbContext context, UserManager<Osoba> userManager)
        {
            _context = context;
            _userManager = userManager;


        }
        [Authorize(Roles = "Administrator")]
        // GET: Korpa
        public async Task<IActionResult> Index()
        {
            var korpe = await _context.Korpa
                .Include(k => k.Korisnik) // Učitava korisnika povezano sa korpom
                .Include(k => k.Stavke)   // Učitava stavke korpe
                    .ThenInclude(s => s.Proizvod) // Učitava proizvod za svaku stavku
                .ToListAsync();

            // Opcionalno: automatski izračunaj ukupnu cijenu i broj proizvoda ako nije ažurirano u bazi
            foreach (var korpa in korpe)
            {
                if (korpa.Stavke != null && korpa.Stavke.Any())
                {
                    korpa.brojProizvoda = korpa.Stavke.Sum(s => s.Kolicina);
                    korpa.ukupnaCijena = korpa.Stavke.Sum(s => s.Cijena);
                }
                else
                {
                    korpa.brojProizvoda = 0;
                    korpa.ukupnaCijena = 0;
                }
            }

            return View(korpe);
        }




        public async Task<IActionResult> dodajIzNarudzbe(int narudzbaId)
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
                return Redirect("/Identity/Account/Register");

            var stavkeNarudzbe = await _context.StavkaNarudzbe
                .Where(n => n.NarudzbaId == narudzbaId)
                .Include(s => s.Proizvod)
                .ToListAsync();

            if (!stavkeNarudzbe.Any())
            {
                TempData["PorukaZaKorpu"] = "Stavke su uklonjene ili je narudžba prazna";
                return View();
            }

            var korpa = await _context.Korpa
                .Include(k => k.Stavke)
                .FirstOrDefaultAsync(c => c.KorisnikId == korisnik.Id);

            var stavkeZaKorpu = stavkeNarudzbe.Select(s => new StavkaKorpe
            {
                ProizvodId = s.ProizvodId,
                Kolicina = s.Kolicina,
                Cijena = s.Cijena
            }).ToList();

            //  staro stanje korpe za računanje dodatnih vrijednosti
            int staraKolicina = korpa?.brojProizvoda ?? 0;
            double staraCijena = korpa?.ukupnaCijena ?? 0;
            int dodatnaKolicina = 0;
            double dodatnaCijena = 0;

            if (korpa == null)
            {
                korpa = new Korpa
                {
                    KorisnikId = korisnik.Id,
                    brojProizvoda = stavkeZaKorpu.Sum(s => s.Kolicina),
                    ukupnaCijena = stavkeZaKorpu.Sum(s => s.Cijena * s.Kolicina),
                    Stavke = stavkeZaKorpu
                };
                _context.Korpa.Add(korpa);
            }
            else
            {
                foreach (var novaStavka in stavkeZaKorpu)
                {
                    var postojecaStavka = korpa.Stavke.FirstOrDefault(s => s.ProizvodId == novaStavka.ProizvodId);
                    if (postojecaStavka != null)
                    {
                        postojecaStavka.Kolicina += novaStavka.Kolicina;
                        postojecaStavka.Cijena = novaStavka.Cijena;
                        dodatnaKolicina += novaStavka.Kolicina;
                        dodatnaCijena += novaStavka.Cijena;
                        await PromijeniKolicinu(korpa.Id, postojecaStavka.ProizvodId, postojecaStavka.Kolicina);
                    }
                    else
                    {
                        korpa.Stavke.Add(novaStavka);
                    }
                }
            }

            // ukupne vrijednosti
            //korpa.brojProizvoda = korpa.Stavke.Sum(s => s.Kolicina);
            //korpa.ukupnaCijena = korpa.Stavke.Sum(s => s.Cijena * s.Kolicina);

            //TempData umjesto ViewBag (zbog RedirectToAction)
            TempData["DodatnaKolicina"] = dodatnaKolicina.ToString();
            TempData["DodatnaCijena"] = dodatnaCijena.ToString();
            TempData["PorukaZaKorpu2"] = "Proizvodi su uspješno dodani u korpu";

            await _context.SaveChangesAsync();

            await AzurirajKorpu(korpa.Id);

            return RedirectToAction("KorpaView", "Korpa");
        }

        [Authorize(Roles = "Korisnik")]
        public async Task<IActionResult> KorpaView()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return Redirect("/Identity/Account/Register");

            }

            var korisnikId = korisnik.Id;
            var korpa = _context.Korpa
                .Include(k => k.Stavke)
                .ThenInclude(s => s.Proizvod)  
                .FirstOrDefault(k => k.KorisnikId == korisnikId);

            if (korpa == null || korpa.Stavke == null || !korpa.Stavke.Any())
            {
                TempData["CartMessage"] = "Korpa je trenutno prazna!";
                return View();
            }

            return View(korpa);
        }

        public async Task<IActionResult> DodajUKorpu(int Id)
        {

            // Dobij korisnički ID
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                //return Redirect("/Identity/Account/Login");
                return DodajUKorpuGost(Id);
            }

            // Pronađi proizvod
            var proizvod = await _context.Proizvod.FirstOrDefaultAsync(m => m.Id == Id);
            if (proizvod == null)
            {
                return NotFound();
            }

            // Pronađi ili kreiraj korpu
            var korpa = await _context.Korpa
                .Include(k => k.Stavke)
                .FirstOrDefaultAsync(k => k.KorisnikId == korisnik.Id);

            if (korpa == null)
            {
                korpa = new Korpa
                {
                    KorisnikId = korisnik.Id,
                    brojProizvoda = 0,
                    ukupnaCijena = 0,
                    Stavke = new List<StavkaKorpe>()
                };
                _context.Korpa.Add(korpa);
                await _context.SaveChangesAsync(); // Sačuvaj da dobiješ ID
            }

            // Osiguraj da Stavke nisu null
            if (korpa.Stavke == null)
            {
                korpa.Stavke = new List<StavkaKorpe>();
            }

            // da li stavka postoji vec od prije, povecava se kolicina ako postoji
            var postojecaStavka = korpa.Stavke.FirstOrDefault(s => s.ProizvodId == Id);
            if (postojecaStavka != null)
            {
                postojecaStavka.Kolicina++;
                postojecaStavka.Cijena = Math.Round(postojecaStavka.Kolicina * proizvod.Cijena,2);
            }
            else
            {
                //nova stavka
                var novaStavka = new StavkaKorpe
                {
                    KorpaId = korpa.Id,
                    ProizvodId = Id,
                    Kolicina = 1,
                    Cijena = proizvod.Cijena
                };
                _context.StavkaKorpe.Add(novaStavka);
                korpa.Stavke.Add(novaStavka); // Dodaj u lokalnu kolekciju
            }
            await _context.SaveChangesAsync();
            // Izračunaj totale
            //korpa.brojProizvoda = korpa.Stavke.Sum(s => s.Kolicina);
            //korpa.ukupnaCijena = korpa.Stavke.Sum(s => s.Cijena);
            await AzurirajKorpu(korpa.Id);

            //await DodajProizvodUKorpu(korisnik.Id, Id); da se ne dupla KOD

            return RedirectToAction("KorpaView", new { korisnikId = korisnik.Id });
            //ovdje mozda: return RedirectToAction("KorpaView");

        }


        [HttpPost]
        public async Task<IActionResult> PromijeniKolicinu(int korpaId, int proizvodId, int kolicina)
        {
            if (kolicina <= 0)
            {
                return await UkloniStavku(korpaId, proizvodId);
            }
            var stavka = await _context.StavkaKorpe
                .FirstOrDefaultAsync(s => s.KorpaId == korpaId && s.ProizvodId == proizvodId);

            if (stavka != null)
            {
                var proizvod = await _context.Proizvod.FindAsync(proizvodId);
                if (proizvod != null)
                {

                    stavka.Kolicina = kolicina;
                    stavka.Cijena = Math.Round(kolicina * proizvod.Cijena,2);

                    await _context.SaveChangesAsync();
                    await AzurirajKorpu(korpaId);
                }
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UkloniStavku(int korpaId, int proizvodId)
        {
            var stavka = await _context.StavkaKorpe
                .FirstOrDefaultAsync(s => s.KorpaId == korpaId && s.ProizvodId == proizvodId);

            if (stavka != null)
            {
                _context.StavkaKorpe.Remove(stavka);
                await _context.SaveChangesAsync();
                await AzurirajKorpu(korpaId);
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> OcistiKorpu(int korpaId)
        {
            var korpa = await _context.Korpa
                .Include(k => k.Stavke)
                .FirstOrDefaultAsync(k => k.Id == korpaId);

            if (korpa != null && korpa.Stavke.Any())
            {
                _context.StavkaKorpe.RemoveRange(korpa.Stavke);
                korpa.brojProizvoda = 0;
                korpa.ukupnaCijena = 0;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        private async Task AzurirajKorpu(int korpaId)
        {
            var korpa = await _context.Korpa.FindAsync(korpaId);
            if (korpa != null)
            {
                var ukupnaKolicina = await _context.StavkaKorpe
                    .Where(s => s.KorpaId == korpaId)
                    .SumAsync(s => s.Kolicina);
                var ukupnaCijena = await _context.StavkaKorpe
                    .Where(s => s.KorpaId == korpaId)
                    .SumAsync(s => s.Cijena);

                korpa.brojProizvoda = ukupnaKolicina;
                korpa.ukupnaCijena = ukupnaCijena;
                await _context.SaveChangesAsync();
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetKorpaInfo(int korpaId)
        {
            var korpa = await _context.Korpa
                .Include(k => k.Stavke)
                .ThenInclude(s => s.Proizvod)
                .FirstOrDefaultAsync(k => k.Id == korpaId);

            if (korpa == null)
            {
                return Json(new { success = false });
            }

            var korpaInfo = new
            {
                success = true,
                brojProizvoda = korpa.brojProizvoda,
                ukupnaCijena = korpa.ukupnaCijena,
                stavke = korpa.Stavke.Select(s => new
                {
                    id = s.Id,
                    proizvodId = s.ProizvodId,
                    naziv = s.Proizvod.Naziv,
                    kolicina = s.Kolicina,
                    cijena = s.Cijena,
                    jedinicnaCijena = s.Proizvod.Cijena
                }).ToList()
            };

            return Json(korpaInfo);
        }
        public async Task<IActionResult> UkloniIzKorpe(int stavkaId, int korisnikId)
        {
            var stavka = await _context.StavkaKorpe.FindAsync(stavkaId);
            if (stavka != null)
            {
                _context.StavkaKorpe.Remove(stavka);

                //Azuriranje korpe
                var korpa = await _context.Korpa.FindAsync(stavka.KorpaId);
                if (korpa != null)
                {
                    var ukupnaKolicina = await _context.StavkaKorpe
                        .Where(s => s.KorpaId == korpa.Id && s.Id != stavkaId)
                        .SumAsync(s => s.Kolicina);

                    var ukupnaCijena = await _context.StavkaKorpe
                        .Where(s => s.KorpaId == korpa.Id && s.Id != stavkaId)
                        .SumAsync(s => s.Cijena);

                    korpa.brojProizvoda = ukupnaKolicina;
                    korpa.ukupnaCijena = ukupnaCijena;
                    _context.Update(korpa);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("KorpaView", new { korisnikId = korisnikId });
        }

        
        private bool KorpaExists(int id)
        {
            return _context.Korpa.Any(e => e.Id == id);
        }


        [HttpPost]
        public IActionResult DodajUKorpuGost(int Id)
        {
            //sa TempData sacuvati proizvod
            var guestCartKey = "GuestCart_" + Guid.NewGuid().ToString("N")[..8];
            TempData[guestCartKey] = Id;
            TempData["GuestCartKey"] = guestCartKey;

            //GostOPCIJE za gosta da se prikaze registracija/login
            return RedirectToAction("GostKorpa");
        }

        public IActionResult GostKorpa()
        {
            var guestCartKey = TempData["GuestCartKey"]?.ToString();
            if (string.IsNullOrEmpty(guestCartKey) || TempData[guestCartKey] == null)
            {
                return RedirectToAction("Index", "Proizvod");
            }

            //Očuvanje podataka između ciklusa
            TempData.Keep("GuestCartKey");
            TempData.Keep(guestCartKey);

            var proizvodId = (int)TempData[guestCartKey];
            var proizvod = _context.Proizvod.Find(proizvodId);

            ViewBag.Proizvod = proizvod;
            ViewBag.GuestCartKey = guestCartKey;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DodajUKorpuNakonPrijave()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return RedirectToAction("Index", "Proizvod");
            }

            //Provjeravanje da li postoji proizvod za dodavanje iz guest korpe
            var guestCartKey = TempData["GuestCartKey"]?.ToString();
            if (!string.IsNullOrEmpty(guestCartKey) && TempData[guestCartKey] != null)
            {
                var proizvodId = (int)TempData[guestCartKey];
                await DodajProizvodUKorpu(korisnik.Id, proizvodId);

                //izbrisem iz tempdata ako postoji
                TempData.Remove(guestCartKey);
                TempData.Remove("GuestCartKey");

                TempData["SuccessMessage1"] = "Proizvod je uspješno dodan u korpu!";
            }

            return RedirectToAction("KorpaView");
        }

        private async Task DodajProizvodUKorpu(string korisnikId, int proizvodId)
        {
            var proizvod = await _context.Proizvod.FirstOrDefaultAsync(m => m.Id == proizvodId);
            if (proizvod == null) return;

            var korpa = await _context.Korpa
                .Include(k => k.Stavke)
                .FirstOrDefaultAsync(k => k.KorisnikId == korisnikId);

            if (korpa == null)
            {
                korpa = new Korpa
                {
                    KorisnikId = korisnikId,
                    brojProizvoda = 0,
                    ukupnaCijena = 0,
                    Stavke = new List<StavkaKorpe>()
                };
                _context.Korpa.Add(korpa);
                await _context.SaveChangesAsync();
            }

            if (korpa.Stavke == null)
            {
                korpa.Stavke = new List<StavkaKorpe>();
            }

            var postojecaStavka = korpa.Stavke.FirstOrDefault(s => s.ProizvodId == proizvodId);
            if (postojecaStavka != null)
            {
                postojecaStavka.Kolicina++;
                postojecaStavka.Cijena = Math.Round(postojecaStavka.Kolicina * proizvod.Cijena,2);
            }
            else
            {
                var novaStavka = new StavkaKorpe
                {
                    KorpaId = korpa.Id,
                    ProizvodId = proizvodId,
                    Kolicina = 1,
                    Cijena = proizvod.Cijena
                };
                _context.StavkaKorpe.Add(novaStavka);
                korpa.Stavke.Add(novaStavka);
            }

            await _context.SaveChangesAsync();
            await AzurirajKorpu(korpa.Id);
        }


        //nakon prijave/registracije se poziva
        public async Task<IActionResult> KorpaGostView()
        {
            var korisnik = await _userManager.GetUserAsync(User);
            if (korisnik == null)
            {
                return RedirectToAction("Index", "Proizvod");
            }

            //provjeravanje Query string parametara koji dolaze sa returnUrl
            var guestCartKey = Request.Query["GuestCartKey"].ToString();
            if (string.IsNullOrEmpty(guestCartKey))
            {
                guestCartKey = TempData["GuestCartKey"]?.ToString();
            }

            if (!string.IsNullOrEmpty(guestCartKey))
            {
                var proizvodIdObj = TempData[guestCartKey];
                if (proizvodIdObj != null && int.TryParse(proizvodIdObj.ToString(), out int proizvodId))
                {
                    await DodajProizvodUKorpu(korisnik.Id, proizvodId);

                    //brisanje
                    TempData.Remove(guestCartKey);
                    TempData.Remove("GuestCartKey");


                }
            }

            return RedirectToAction("KorpaView");
        }


        public IActionResult GostLogin()
        {
            var guestCartKey = TempData["GuestCartKey"]?.ToString();
            if (!string.IsNullOrEmpty(guestCartKey))
            {
                //ocuvanje opet, da bi ostalo nakon Redirecta
                TempData.Keep("GuestCartKey");
                TempData.Keep(guestCartKey);

                //url sa guest korpom
                var returnUrl = Url.Action("KorpaGostView", "Korpa", new { GuestCartKey = guestCartKey });
                return Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            return Redirect("/Identity/Account/Login");
        }

        public IActionResult GostRegister()
        {
            var guestCartKey = TempData["GuestCartKey"]?.ToString();
            if (!string.IsNullOrEmpty(guestCartKey))
            {
                TempData.Keep("GuestCartKey");
                TempData.Keep(guestCartKey);

                var returnUrl = Url.Action("KorpaGostView", "Korpa", new { GuestCartKey = guestCartKey });
                return Redirect($"/Identity/Account/Register?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            return Redirect("/Identity/Account/Register");
        }
    }
}