using APITicketPro.Helpers;
using APITicketPro.Models;
using APITicketPro.Services;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox", "libwkhtmltox.dll"));
// Add services to the container.

// PARA PERMITIR CORS EN LA API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("https://localhost:7130", "https://localhost:7298") // Puerto del frontend
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddControllers();

builder.Services.AddDbContext<DBTicketProContext>(options =>
    options.UseSqlServer(
            builder.Configuration.GetConnectionString("TicketProDBConnection")
        )
);
builder.Services.AddControllersWithViews(); // Esto habilita las vistas para el ViewRenderer
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddScoped<ViewRenderer>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Para correos Gmail
builder.Services.AddScoped<EmailService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DBTicketProContext>();

    if (!db.usuario.Any(u => u.nombre_usuario == "admin1"))
    {
        // ADMIN
        var admin = new usuario
        {
            nombre_usuario = "admin1",
            email = "admin1@ticketpro.com",
            contrasenia = BCrypt.Net.BCrypt.HashPassword("Admin123"),
            tipo_usuario = 'I',
            estado = true,
            fecha_registro = DateTime.Now
        };
        db.usuario.Add(admin);
        db.SaveChanges();

        db.usuario_interno.Add(new usuario_interno
        {
            id_usuario = admin.id_usuario,
            nombre = "Luis",
            apellido = "Ram�rez",
            direccion = "San Salvador",
            dui = "12345678-9",
            id_rol = 1 // Aseg�rate de que el rol 1 sea Administrador
        });

        db.contacto_usuario.Add(new contacto_usuario
        {
            id_usuario = admin.id_usuario,
            email = "admin.contacto@ticketpro.com",
            telefono = "7777-8888"
        });

        // TECNICO
        var tecnico = new usuario
        {
            nombre_usuario = "tecnico1",
            email = "tecnico1@ticketpro.com",
            contrasenia = BCrypt.Net.BCrypt.HashPassword("Tec12345"),
            tipo_usuario = 'I',
            estado = true,
            fecha_registro = DateTime.Now
        };
        db.usuario.Add(tecnico);
        db.SaveChanges();

        db.usuario_interno.Add(new usuario_interno
        {
            id_usuario = tecnico.id_usuario,
            nombre = "Carlos",
            apellido = "Mej�a",
            direccion = "Santa Ana",
            dui = "98765432-1",
            id_rol = 2 // T�cnico
        });

        db.contacto_usuario.Add(new contacto_usuario
        {
            id_usuario = tecnico.id_usuario,
            email = "tecnico.contacto@ticketpro.com",
            telefono = "6666-5555"
        });

        // CLIENTE
        var cliente = new usuario
        {
            nombre_usuario = "cliente1",
            email = "cliente1@empresa.com",
            contrasenia = BCrypt.Net.BCrypt.HashPassword("Cliente123"),
            tipo_usuario = 'E',
            estado = true,
            fecha_registro = DateTime.Now
        };
        db.usuario.Add(cliente);
        db.SaveChanges();

        db.usuario_externo.Add(new usuario_externo
        {
            id_usuario = cliente.id_usuario,
            nombre = "Mar�a",
            apellido = "Lopez",
            empresa = "Empresa XYZ"
        });

        db.contacto_usuario.Add(new contacto_usuario
        {
            id_usuario = cliente.id_usuario,
            email = "cliente.contacto@empresa.com",
            telefono = "2222-3333"
        });

        db.SaveChanges();
    }
    if (!db.usuario.Any(u => u.nombre_usuario == "admin2"))
    {
        // Nuevo ADMIN
        var admin = new usuario
        {
            nombre_usuario = "admin2",
            email = "admin2@ticketpro.com",
            contrasenia = BCrypt.Net.BCrypt.HashPassword("Admin456"),
            tipo_usuario = 'I',
            estado = true,
            fecha_registro = DateTime.Now
        };
        db.usuario.Add(admin);
        db.SaveChanges();

        db.usuario_interno.Add(new usuario_interno
        {
            id_usuario = admin.id_usuario,
            nombre = "Ana",
            apellido = "Gonz�lez",
            direccion = "San Miguel",
            dui = "12312312-3",
            id_rol = 1 // Aseg�rate que el rol 1 sea Administrador
        });

        // Nuevo TECNICO
        var tecnico = new usuario
        {
            nombre_usuario = "tecnico2",
            email = "tecnico2@ticketpro.com",
            contrasenia = BCrypt.Net.BCrypt.HashPassword("Tec45678"),
            tipo_usuario = 'I',
            estado = true,
            fecha_registro = DateTime.Now
        };
        db.usuario.Add(tecnico);
        db.SaveChanges();

        db.usuario_interno.Add(new usuario_interno
        {
            id_usuario = tecnico.id_usuario,
            nombre = "Pedro",
            apellido = "Cruz",
            direccion = "La Libertad",
            dui = "45645645-6",
            id_rol = 2 // T�cnico
        });

        // Nuevo CLIENTE
        var cliente = new usuario
        {
            nombre_usuario = "cliente2",
            email = "cliente2@empresa.com",
            contrasenia = BCrypt.Net.BCrypt.HashPassword("Cliente456"),
            tipo_usuario = 'E',
            estado = true,
            fecha_registro = DateTime.Now
        };
        db.usuario.Add(cliente);
        db.SaveChanges();

        db.usuario_externo.Add(new usuario_externo
        {
            id_usuario = cliente.id_usuario,
            nombre = "Laura",
            apellido = "Mart�nez",
            empresa = "Soluciones XYZ"
        });

        db.SaveChanges();
    }


}



app.UseCors("AllowFrontend");
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseStaticFiles(); // Esto habilita wwwroot

app.MapControllers();

app.Run();
