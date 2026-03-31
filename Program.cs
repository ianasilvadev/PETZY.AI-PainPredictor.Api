using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Configuração de upload (até 5MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024;
});

// ✅ Controllers
builder.Services.AddControllers();

// 🔥 Swagger (documentação + interface)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🌐 Swagger UI (interface web)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔥 Mapeia controllers (seu /frames)
app.MapControllers();

app.UseHttpsRedirection();

app.Run();