using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Ativa Controllers
builder.Services.AddControllers();

// Swagger + suporte a upload
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();

    // Faz o Swagger exibir campo file corretamente
    options.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

// HttpClient para chamar Python
builder.Services.AddHttpClient();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// Não vamos usar antiforgery
// app.UseAntiforgery();

app.UseAuthorization();

app.MapControllers();

app.Run();
