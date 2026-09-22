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

        public DbSet<Fisioterapeuta> Fisioterapeutas { get; set; }
            = null!;

        public DbSet<Paciente> Pacientes { get; set; }
            = null!;

        public DbSet<Cita> Citas { get; set; }
            = null!;

        public DbSet<Tratamiento> Tratamientos { get; set; }
            = null!;

        public DbSet<SesionTratamiento> SesionesTratamiento
        {
            get;
            set;
        } = null!;

        public DbSet<ConsultaClinica> ConsultasClinicas
        {
            get;
            set;
        } = null!;

        public DbSet<ProgramaRehabilitacion> ProgramasRehabilitacion
        {
            get;
            set;
        } = null!;

        public DbSet<EvolucionDiaria> EvolucionesDiarias
        {
            get;
            set;
        } = null!;

        public DbSet<Ejercicio> Ejercicios
        {
            get;
            set;
        } = null!;

        public DbSet<EjercicioAsignado> EjerciciosAsignados
        {
            get;
            set;
        } = null!;

        public DbSet<RegistroEjercicioPaciente>
            RegistrosEjerciciosPaciente
        {
            get;
            set;
        } = null!;

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // CORREOS ÚNICOS
            // =========================================================

            modelBuilder.Entity<Fisioterapeuta>()
                .HasIndex(f => f.Correo)
                .IsUnique();

            modelBuilder.Entity<Paciente>()
                .HasIndex(p => p.Correo)
                .IsUnique();

            // =========================================================
            // CITA - FISIOTERAPEUTA
            // =========================================================

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Fisioterapeuta)
                .WithMany(f => f.Citas)
                .HasForeignKey(c => c.FisioterapeutaId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // CITA - PACIENTE
            // =========================================================

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // TRATAMIENTO - PACIENTE
            // =========================================================

            modelBuilder.Entity<Tratamiento>()
                .HasOne(t => t.Paciente)
                .WithMany(p => p.Tratamientos)
                .HasForeignKey(t => t.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // TRATAMIENTO - FISIOTERAPEUTA
            // =========================================================

            modelBuilder.Entity<Tratamiento>()
                .HasOne(t => t.Fisioterapeuta)
                .WithMany(f => f.Tratamientos)
                .HasForeignKey(t => t.FisioterapeutaId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // TRATAMIENTO - SESIONES
            // =========================================================

            modelBuilder.Entity<SesionTratamiento>()
                .HasOne(s => s.Tratamiento)
                .WithMany(t => t.Sesiones)
                .HasForeignKey(s => s.TratamientoId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================================================
            // SESIÓN - CITA
            // =========================================================

            modelBuilder.Entity<SesionTratamiento>()
                .HasOne(s => s.Cita)
                .WithOne(c => c.SesionTratamiento)
                .HasForeignKey<Cita>(c => c.SesionTratamientoId)
                .OnDelete(DeleteBehavior.SetNull);

            // =========================================================
            // FECHAS
            // =========================================================

            modelBuilder.Entity<Cita>()
                .Property(c => c.Fecha)
                .HasColumnType("date");

            modelBuilder.Entity<Tratamiento>()
                .Property(t => t.FechaInicio)
                .HasColumnType("date");

            modelBuilder.Entity<SesionTratamiento>()
                .Property(s => s.FechaCompletada)
                .HasColumnType(
                    "timestamp without time zone");

            // =========================================================
            // SESIONES ÚNICAS
            // =========================================================

            modelBuilder.Entity<SesionTratamiento>()
                .HasIndex(s => new
                {
                    s.TratamientoId,
                    s.NumeroSesion
                })
                .IsUnique();

            // =========================================================
            // CONSULTAS CLÍNICAS
            // =========================================================

            modelBuilder.Entity<ConsultaClinica>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.ConsultasClinicas)
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConsultaClinica>()
                .HasOne(c => c.Fisioterapeuta)
                .WithMany(f => f.ConsultasClinicas)
                .HasForeignKey(c => c.FisioterapeutaId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // PROGRAMAS
            // =========================================================

            modelBuilder.Entity<ProgramaRehabilitacion>()
                .HasOne(p => p.Paciente)
                .WithMany(p => p.ProgramasRehabilitacion)
                .HasForeignKey(p => p.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProgramaRehabilitacion>()
                .HasOne(p => p.Fisioterapeuta)
                .WithMany(f => f.ProgramasRehabilitacion)
                .HasForeignKey(p => p.FisioterapeutaId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // EVOLUCIONES
            // =========================================================

            modelBuilder.Entity<EvolucionDiaria>()
                .HasOne(e => e.ProgramaRehabilitacion)
                .WithMany(p => p.Evoluciones)
                .HasForeignKey(e => e.ProgramaRehabilitacionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EvolucionDiaria>()
                .HasOne(e => e.Fisioterapeuta)
                .WithMany(f => f.Evoluciones)
                .HasForeignKey(e => e.FisioterapeutaId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // EJERCICIO - PROGRAMA
            // Se mantiene compatibilidad con la estructura anterior.
            // =========================================================

            modelBuilder.Entity<EjercicioAsignado>()
                .HasOne(e => e.ProgramaRehabilitacion)
                .WithMany(p => p.EjerciciosAsignados)
                .HasForeignKey(e => e.ProgramaRehabilitacionId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // =========================================================
            // EJERCICIO - CATÁLOGO
            // =========================================================

            modelBuilder.Entity<EjercicioAsignado>()
                .HasOne(e => e.Ejercicio)
                .WithMany(e => e.Asignaciones)
                .HasForeignKey(e => e.EjercicioId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // EJERCICIO - CITA
            // =========================================================

            modelBuilder.Entity<EjercicioAsignado>()
                .HasOne(e => e.Cita)
                .WithMany(c => c.EjerciciosAsignados)
                .HasForeignKey(e => e.CitaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // =========================================================
            // FECHA DE ASIGNACIÓN
            // =========================================================

            modelBuilder.Entity<EjercicioAsignado>()
                .Property(e => e.FechaAsignacion)
                .HasColumnType("timestamp with time zone");

            // =========================================================
            // PACIENTE - REGISTRO DE EJERCICIO
            // =========================================================

            modelBuilder.Entity<RegistroEjercicioPaciente>()
                .HasOne(r => r.Paciente)
                .WithMany(p => p.RegistrosEjercicios)
                .HasForeignKey(r => r.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // ASIGNACIÓN - REGISTRO
            // =========================================================

            modelBuilder.Entity<RegistroEjercicioPaciente>()
                .HasOne(r => r.EjercicioAsignado)
                .WithMany()
                .HasForeignKey(r => r.EjercicioAsignadoId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================================================
            // FECHAS DE REGISTRO
            // =========================================================

            modelBuilder.Entity<RegistroEjercicioPaciente>()
                .Property(r => r.Fecha)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<RegistroEjercicioPaciente>()
                .Property(r => r.FechaCompletado)
                .HasColumnType("timestamp with time zone");

            // =========================================================
            // EVITAR REGISTROS DUPLICADOS
            // =========================================================

            modelBuilder.Entity<RegistroEjercicioPaciente>()
                .HasIndex(r => new
                {
                    r.EjercicioAsignadoId,
                    r.PacienteId,
                    r.Fecha
                })
                .IsUnique();

            // =========================================================
            // ÍNDICES PARA EJERCICIOS
            // =========================================================

            modelBuilder.Entity<Ejercicio>()
                .HasIndex(e => e.Nombre);

            modelBuilder.Entity<Ejercicio>()
                .HasIndex(e => e.TipoContraccion);

            modelBuilder.Entity<Ejercicio>()
                .HasIndex(e => e.RegionAnatomica);

            modelBuilder.Entity<Ejercicio>()
                .HasIndex(e => e.ObjetivoTerapeutico);

            modelBuilder.Entity<Ejercicio>()
                .HasIndex(e => e.FasePaciente);

            modelBuilder.Entity<EjercicioAsignado>()
                .HasIndex(e => e.CitaId);
        }
    }
}