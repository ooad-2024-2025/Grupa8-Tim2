using K_K.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace K_K.Data
{
    public class ApplicationDbContext : IdentityDbContext<Osoba>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Proizvod> Proizvod { get; set; }
        public DbSet<Hrana> Hrana { get; set; }
        public DbSet<Pice> Pice { get; set; }
        public DbSet<KarticnoPlacanje> KarticnoPlacanjes { get; set; }
        public DbSet<LokacijaKafica> LokacijaKafica { get; set; }
        public DbSet<Narudzba> Narudzba  { get; set; }
        public DbSet<Obavijest> Obavijest  { get; set; }
        public DbSet<Recenzija> Recenzija { get; set; }
        public DbSet<StavkaNarudzbe> StavkaNarudzbe { get; set; }
        public DbSet<StavkaKorpe> StavkaKorpe { get; set; }
        public DbSet<Korpa> Korpa { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proizvod>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<Hrana>("Hrana")
                .HasValue<Pice>("Pice");

            modelBuilder.Entity<Proizvod>().ToTable("Proizvod");
            modelBuilder.Entity<KarticnoPlacanje>().ToTable("KarticnoPlacanje");
            modelBuilder.Entity<LokacijaKafica>().ToTable("LokacijaKafica");
            modelBuilder.Entity<Narudzba>().ToTable("Narudzba");
            modelBuilder.Entity<Obavijest>().ToTable("Obavijest");
            modelBuilder.Entity<Recenzija>().ToTable("Recenzija");
            modelBuilder.Entity<StavkaNarudzbe>().ToTable("StavkaNarudzbe");
            modelBuilder.Entity<StavkaKorpe>().ToTable("StavkaKorpe");
            modelBuilder.Entity<Korpa>().ToTable("Korpa");

            modelBuilder.Entity<Osoba>(b =>
            {
                b.Property(u => u.Ime);
                b.Property(u => u.Prezime);
                b.Property(u => u.Adresa);
                b.Property(u => u.Uloga);
            });
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
    }
}
