using Microsoft.AspNetCore.Http.Features;
using Petzy.FrameReceiver.Services;
using Petzy.FrameReceiver.Storage;

var builder = WebApplication.CreateBuilder(args);

// Configurar limite de upload
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024;
});

// Adicionar serviços de controle e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicionar serviços de integração com Python
builder.Services.AddHttpClient<IFrameAnalysisService, FrameAnalysisService>();
builder.Services.AddScoped<IFrameAnalysisService, FrameAnalysisService>();
// builder.Services.AddSingleton<FrameStore>(); // se FrameStore não for estático

// 🔹 Adicionar CORS antes de builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configurar pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Usar CORS depois do builder.Build()
app.UseCors("AllowAll");

// app.UseHttpsRedirection();
app.MapControllers();

app.Run();