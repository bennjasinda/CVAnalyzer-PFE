using Microsoft.EntityFrameworkCore;
using CvParsing.Models;

namespace CvParsing.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Cv> Cvs { get; set; }
    public DbSet<OffreEmploi> OffresEmploi { get; set; }
    public DbSet<Competence> Competences { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cv -> Utilisateur
        modelBuilder.Entity<Cv>()
            .HasOne(c => c.Utilisateur)
            .WithMany()
            .HasForeignKey(c => c.UtilisateurId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}