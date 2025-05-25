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
