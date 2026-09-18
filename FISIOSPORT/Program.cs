using FISIOSPORT.Data;
using FISIOSPORT.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// BASE DE DATOS - SUPABASE / POSTGRESQL
// ======================================================

var connectionString =
    builder.Configuration["ConnectionStrings:DefaultConnection"];

Console.WriteLine(
    $"CONEXIÓN ENCONTRADA: {!string.IsNullOrWhiteSpace(connectionString)}"
);

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró ConnectionStrings:DefaultConnection."
    );
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.CommandTimeout(60);
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null
            );
        }
    )
);
// ======================================================
// MVC
// ======================================================

builder.Services.AddControllersWithViews();

// ======================================================
// AUTENTICACIÓN POR COOKIES
// ======================================================

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";

        options.Cookie.Name = "FisioSport.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// ======================================================
// RENDER
// ======================================================

// Render proporciona la variable PORT.
// En local esta parte no modifica tu configuración.
var renderPort = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(renderPort))
{
    builder.WebHost.UseUrls(
        $"http://0.0.0.0:{renderPort}"
    );
}

var app = builder.Build();

// ======================================================
// BASE DE DATOS + DATOS INICIALES
// ======================================================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Aplica automáticamente migraciones pendientes.
    await context.Database.MigrateAsync();

    // --------------------------------------------------
    // FISIOTERAPEUTAS
    // --------------------------------------------------

    if (!await context.Fisioterapeutas.AnyAsync())
    {
        var ana = new Fisioterapeuta
        {
            Nombre = "Dra. Ana Rojas",
            Correo = "ana@fisiosport.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            Especialidad = "Rehabilitación deportiva",
            NumeroColegiado = "COL-4521"
        };

        var carlos = new Fisioterapeuta
        {
            Nombre = "Lic. Carlos Mendoza",
            Correo = "carlos@fisiosport.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            Especialidad = "Terapia neurológica",
            NumeroColegiado = "COL-7789"
        };

        context.Fisioterapeutas.AddRange(
            ana,
            carlos
        );

        await context.SaveChangesAsync();
    }

    // --------------------------------------------------
    // PACIENTES
    // --------------------------------------------------

    if (!await context.Pacientes.AnyAsync())
    {
        var paciente1 = new Paciente
        {
            Nombre = "Mario Fernandez",
            Correo = "mario@correo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            Telefono = "70011223",
            ContactoEmergencia = "Lucia Fernandez - 70099887"
        };

        var paciente2 = new Paciente
        {
            Nombre = "Elena Castro",
            Correo = "elena@correo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            Telefono = "70022334",
            ContactoEmergencia = "Jorge Castro - 70088776"
        };

        var paciente3 = new Paciente
        {
            Nombre = "Ricardo Peña",
            Correo = "ricardo@correo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            Telefono = "70033445",
            ContactoEmergencia = "Marta Peña - 70077665"
        };

        context.Pacientes.AddRange(
            paciente1,
            paciente2,
            paciente3
        );

        await context.SaveChangesAsync();
    }

    // --------------------------------------------------
    // CITAS
    // --------------------------------------------------

    if (!await context.Citas.AnyAsync())
    {
        var ana = await context.Fisioterapeutas
            .FirstAsync(f => f.Correo == "ana@fisiosport.com");

        var carlos = await context.Fisioterapeutas
            .FirstAsync(f => f.Correo == "carlos@fisiosport.com");

        var mario = await context.Pacientes
            .FirstAsync(p => p.Correo == "mario@correo.com");

        var elena = await context.Pacientes
            .FirstAsync(p => p.Correo == "elena@correo.com");

        var ricardo = await context.Pacientes
            .FirstAsync(p => p.Correo == "ricardo@correo.com");

        var citas = new List<Cita>
        {
            new Cita
            {
                FisioterapeutaId = ana.Id,
                PacienteId = mario.Id,
                Fecha = DateTime.Today,
                Hora = "09:00",
                Estado = "Confirmada",
                Notas = "Sesión de rodilla, segunda semana."
            },

            new Cita
            {
                FisioterapeutaId = ana.Id,
                PacienteId = elena.Id,
                Fecha = DateTime.Today,
                Hora = "10:30",
                Estado = "Pendiente",
                Notas = "Primera evaluación."
            },

            new Cita
            {
                FisioterapeutaId = ana.Id,
                PacienteId = ricardo.Id,
                Fecha = DateTime.Today.AddDays(1),
                Hora = "11:00",
                Estado = "Confirmada",
                Notas = "Continuación de terapia de hombro."
            },

            new Cita
            {
                FisioterapeutaId = carlos.Id,
                PacienteId = mario.Id,
                Fecha = DateTime.Today,
                Hora = "15:00",
                Estado = "Completada",
                Notas = "Terapia neurológica, sesión 4."
            }
        };

        context.Citas.AddRange(citas);

        await context.SaveChangesAsync();
    }
}

// ======================================================
// MIDDLEWARE
// ======================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Para comprobar después que Render está funcionando.
app.MapGet("/health", () => Results.Ok("FisioSport OK"));

// ======================================================
// RUTA PRINCIPAL
// ======================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();