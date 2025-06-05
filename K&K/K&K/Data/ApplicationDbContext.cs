using K_K.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace K_K.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Hrana> Hrana { get; set; }
        public DbSet<KarticnoPlacanje> KarticnoPlacanjes { get; set; }
        public DbSet<LokacijaKafica> LokacijaKafica { get; set; }
        public DbSet<Narudzba> Narudzba  { get; set; }
        public DbSet<Obavijest> Obavijest  { get; set; }
        public DbSet<Osoba> Osoba  { get; set; }
        public DbSet<Pice> Pice { get; set; }
        public DbSet<Proizvod> Proizvod { get; set; }
        public DbSet<Recenzija> Recenzija { get; set; }
        public DbSet<StavkaNarudzbe> StavkaNarudzbe { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Hrana>().ToTable("Hrana");
            modelBuilder.Entity<KarticnoPlacanje>().ToTable("KarticnoPlacanje");
            modelBuilder.Entity<LokacijaKafica>().ToTable("LokacijaKafica");
            modelBuilder.Entity<Narudzba>().ToTable("Narudzba");
            modelBuilder.Entity<Obavijest>().ToTable("Obavijest");
            modelBuilder.Entity<Osoba>().ToTable("Osoba");
            modelBuilder.Entity<Pice>().ToTable("Pice");
            modelBuilder.Entity<Proizvod>().ToTable("Proizvod");
            modelBuilder.Entity<Recenzija>().ToTable("Recenzija");
            modelBuilder.Entity<StavkaNarudzbe>().ToTable("StavkaNarudzbe");

            modelBuilder.Entity<Narudzba>()
              .HasOne(n => n.Korisnik)
              .WithMany()
              .HasForeignKey(n => n.KorisnikId)
              .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Narudzba>()
                .HasOne(n => n.Radnik)
                .WithMany()
                .HasForeignKey(n => n.RadnikId)
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<K_K.Models.Korpa> Korpa { get; set; } = default!;
    }
}
