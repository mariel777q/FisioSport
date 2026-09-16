using Microsoft.EntityFrameworkCore;
using FISIOSPORT.Models;

namespace FISIOSPORT.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Fisioterapeuta> Fisioterapeutas { get; set; } = null!;
        public DbSet<Paciente> Pacientes { get; set; } = null!;
        public DbSet<Cita> Citas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Correos únicos
            modelBuilder.Entity<Fisioterapeuta>()
                .HasIndex(f => f.Correo)
                .IsUnique();

            modelBuilder.Entity<Paciente>()
                .HasIndex(p => p.Correo)
                .IsUnique();

            // Fisioterapeuta 1 ---- N Citas
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Fisioterapeuta)
                .WithMany(f => f.Citas)
                .HasForeignKey(c => c.FisioterapeutaId);

            // Paciente 1 ---- N Citas
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.PacienteId);

            // En PostgreSQL solo necesitamos fecha,
            // no timestamp con zona horaria.
            modelBuilder.Entity<Cita>()
                .Property(c => c.Fecha)
                .HasColumnType("date");
        }
    }
}