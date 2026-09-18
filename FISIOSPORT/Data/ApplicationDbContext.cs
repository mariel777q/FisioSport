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

        public DbSet<Tratamiento> Tratamientos { get; set; } = null!;

        public DbSet<SesionTratamiento> SesionesTratamiento { get; set; } = null!;
        public DbSet<ConsultaClinica> ConsultasClinicas { get; set; } = null!;

        public DbSet<ProgramaRehabilitacion> ProgramasRehabilitacion { get; set; } = null!;

        public DbSet<EvolucionDiaria> EvolucionesDiarias { get; set; } = null!;

        public DbSet<Ejercicio> Ejercicios { get; set; } = null!;

        public DbSet<EjercicioAsignado> EjerciciosAsignados { get; set; } = null!;

        public DbSet<RegistroEjercicioPaciente> RegistrosEjerciciosPaciente { get; set; } = null!;

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
                .HasForeignKey(c => c.FisioterapeutaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Paciente 1 ---- N Citas
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Paciente 1 ---- N Tratamientos
            modelBuilder.Entity<Tratamiento>()
                .HasOne(t => t.Paciente)
                .WithMany(p => p.Tratamientos)
                .HasForeignKey(t => t.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fisioterapeuta 1 ---- N Tratamientos
            modelBuilder.Entity<Tratamiento>()
                .HasOne(t => t.Fisioterapeuta)
                .WithMany(f => f.Tratamientos)
                .HasForeignKey(t => t.FisioterapeutaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tratamiento 1 ---- N Sesiones
            modelBuilder.Entity<SesionTratamiento>()
                .HasOne(s => s.Tratamiento)
                .WithMany(t => t.Sesiones)
                .HasForeignKey(s => s.TratamientoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Sesión 1 ---- 0..1 Cita
            modelBuilder.Entity<SesionTratamiento>()
                .HasOne(s => s.Cita)
                .WithOne(c => c.SesionTratamiento)
                .HasForeignKey<Cita>(c => c.SesionTratamientoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Fecha PostgreSQL
            modelBuilder.Entity<Cita>()
                .Property(c => c.Fecha)
                .HasColumnType("date");

            modelBuilder.Entity<Tratamiento>()
                .Property(t => t.FechaInicio)
                .HasColumnType("date");

            modelBuilder.Entity<SesionTratamiento>()
                .Property(s => s.FechaCompletada)
                .HasColumnType("timestamp without time zone");

            // No puede existir dos veces el mismo número de sesión
            // dentro del mismo tratamiento.
            modelBuilder.Entity<SesionTratamiento>()
                .HasIndex(s => new
                {
                    s.TratamientoId,
                    s.NumeroSesion
                })
                .IsUnique();
            // Paciente 1 ---- N Consultas clínicas
            modelBuilder.Entity<ConsultaClinica>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.ConsultasClinicas)
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fisioterapeuta 1 ---- N Consultas clínicas
            modelBuilder.Entity<ConsultaClinica>()
                .HasOne(c => c.Fisioterapeuta)
                .WithMany(f => f.ConsultasClinicas)
                .HasForeignKey(c => c.FisioterapeutaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Paciente 1 ---- N Programas de rehabilitación
            modelBuilder.Entity<ProgramaRehabilitacion>()
                .HasOne(p => p.Paciente)
                .WithMany(p => p.ProgramasRehabilitacion)
                .HasForeignKey(p => p.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fisioterapeuta 1 ---- N Programas de rehabilitación
            modelBuilder.Entity<ProgramaRehabilitacion>()
                .HasOne(p => p.Fisioterapeuta)
                .WithMany(f => f.ProgramasRehabilitacion)
                .HasForeignKey(p => p.FisioterapeutaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Programa 1 ---- N Evoluciones
            modelBuilder.Entity<EvolucionDiaria>()
                .HasOne(e => e.ProgramaRehabilitacion)
                .WithMany(p => p.Evoluciones)
                .HasForeignKey(e => e.ProgramaRehabilitacionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Fisioterapeuta 1 ---- N Evoluciones
            modelBuilder.Entity<EvolucionDiaria>()
                .HasOne(e => e.Fisioterapeuta)
                .WithMany(f => f.Evoluciones)
                .HasForeignKey(e => e.FisioterapeutaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Programa 1 ---- N Ejercicios asignados
            modelBuilder.Entity<EjercicioAsignado>()
                .HasOne(e => e.ProgramaRehabilitacion)
                .WithMany(p => p.EjerciciosAsignados)
                .HasForeignKey(e => e.ProgramaRehabilitacionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ejercicio 1 ---- N Asignaciones
            modelBuilder.Entity<EjercicioAsignado>()
                .HasOne(e => e.Ejercicio)
                .WithMany(e => e.Asignaciones)
                .HasForeignKey(e => e.EjercicioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Paciente 1 ---- N Registros de ejercicios
            modelBuilder.Entity<RegistroEjercicioPaciente>()
                .HasOne(r => r.Paciente)
                .WithMany(p => p.RegistrosEjercicios)
                .HasForeignKey(r => r.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ejercicio asignado 1 ---- N Registros
            modelBuilder.Entity<RegistroEjercicioPaciente>()
                .HasOne(r => r.EjercicioAsignado)
                .WithMany()
                .HasForeignKey(r => r.EjercicioAsignadoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Un ejercicio no puede registrarse dos veces para el mismo paciente
            // durante el mismo día.
            modelBuilder.Entity<RegistroEjercicioPaciente>()
                .HasIndex(r => new
                {
                    r.EjercicioAsignadoId,
                    r.PacienteId,
                    r.Fecha
                })
                .IsUnique();
        }
    }
}