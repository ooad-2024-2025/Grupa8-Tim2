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

        public DbSet<Proizvod> Proizvod { get; set; }
        public DbSet<Hrana> Hrana { get; set; }
        public DbSet<Pice> Pice { get; set; }
        public DbSet<KarticnoPlacanje> KarticnoPlacanjes { get; set; }
        public DbSet<LokacijaKafica> LokacijaKafica { get; set; }
        public DbSet<Narudzba> Narudzba  { get; set; }
        public DbSet<Obavijest> Obavijest  { get; set; }
        public DbSet<Osoba> Osoba  { get; set; }
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
            modelBuilder.Entity<Osoba>().ToTable("Osoba");
            modelBuilder.Entity<Recenzija>().ToTable("Recenzija");
            modelBuilder.Entity<StavkaNarudzbe>().ToTable("StavkaNarudzbe");
            modelBuilder.Entity<StavkaKorpe>().ToTable("StavkaKorpe");
            modelBuilder.Entity<Korpa>().ToTable("Korpa");

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
