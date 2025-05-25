using APITicketPro.Models;
using APITicketPro.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
            apellido = "Ramírez",
            direccion = "San Salvador",
            dui = "12345678-9",
            id_rol = 1 // Asegúrate de que el rol 1 sea Administrador
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
            apellido = "Mejía",
            direccion = "Santa Ana",
            dui = "98765432-1",
            id_rol = 2 // Técnico
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
            nombre = "María",
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
