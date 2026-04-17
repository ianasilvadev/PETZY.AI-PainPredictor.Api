
# 🐾 Petzy.FrameReceiver – Backend de Análise de Dor em Animais

Este é o backend em .NET que recebe frames de vídeo, envia para um serviço de IA em Python e retorna a análise (espécie, score de dor, bounding box).

## 📋 Pré‑requisitos

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download) (recomendado 8.0)
- Serviço Python de análise rodando em `http://localhost:8000/analyze-frame`  
  (exemplo: modelo de detecção de dor em animais)
- Opcional: `curl` para testes manuais

## 🚀 Como executar o backend

1. **Clone ou acesse a pasta do projeto**  
   ```bash
   cd Petzy.FrameReceiver
   ```

2. **Restaure as dependências**  
   ```bash
   dotnet restore
   ```

3. **Compile o projeto**  
   ```bash
   dotnet build
   ```

4. **Execute a aplicação**  
   ```bash
   dotnet run
   ```

   O console mostrará algo como:
   ```
   Now listening on: http://0.0.0.0:5217
   Application started. Press Ctrl+C to shut down.
   ```

5. **Acesse o Swagger** (apenas em desenvolvimento)  
   Abra o navegador em: [http://localhost:5217/swagger](http://localhost:5217/swagger)  
   Lá você pode testar todos os endpoints (`POST /frames`, `GET /frames/{id}`, etc.)

## 🔌 Endpoints principais

| Método | Rota               | Descrição                                                                 |
|--------|--------------------|---------------------------------------------------------------------------|
| POST   | `/frames`          | Envia um frame (imagem) para análise. Parâmetros: `Frame` (arquivo), `Timestamp` (opcional) |
| GET    | `/frames/{id}`     | Recupera um frame salvo pelo seu ID, incluindo a análise e a imagem em Base64 |
| GET    | `/frames/latest`   | Retorna o último frame enviado                                            |
| GET    | `/frames/by-date?date=YYYY-MM-DD` | Lista todos os frames de uma data específica                 |
| GET    | `/frames/{id}/image` | Retorna apenas a imagem do frame (formato JPEG)                          |

## 🧪 Testando com `curl`

Após o backend estar rodando e o serviço Python ativo na porta 8000, teste o envio de um frame:

```bash
curl -X POST http://localhost:5217/frames \
  -F "Frame=@/caminho/para/imagem.jpg" \
  -F "Timestamp=2025-01-01T12:00:00Z"
```

Exemplo de resposta bem‑sucedida:
```json
{
  "frameId": "abc-123",
  "timestamp": "2025-01-01T12:00:00Z",
  "analysis": {
    "animals": [
      {
        "trackId": 1,
        "species": "gato",
        "bbox": [146, 12, 414, 425],
        "riskScore": 0.73,
        "riskLevel": "critico"
      }
    ]
  }
}
```

## 🐍 Integração com o serviço Python

O backend chama o serviço Python em `http://localhost:8000/analyze-frame`.  
Certifique‑se de que o serviço esteja ativo **antes** de enviar frames para o .NET.  
O contrato esperado é:
- **Método:** POST  
- **Campo do arquivo:** `file` (multipart/form-data)  
- **Resposta JSON** contendo `animals` (array com `species`, `riskScore`, `riskLevel`, `bbox`)

## ⚙️ Configuração adicional

### Porta do backend
Por padrão o Kestrel usa a porta `5217` (conforme log). Se precisar alterar, defina a variável de ambiente `ASPNETCORE_URLS` ou edite o `launchSettings.json`.

### CORS
O CORS está liberado para todas as origens (`AllowAnyOrigin`). Em produção, restrinja para a URL do seu frontend.

### Limite de upload
O tamanho máximo de cada frame é **5 MB** (configurado no `Program.cs`).

## 🛠️ Possíveis problemas e soluções

| Problema                                      | Verifique / Solução                                          |
|-----------------------------------------------|--------------------------------------------------------------|
| `System.Net.Http.HttpRequestException`        | Serviço Python não está rodando em `http://localhost:8000`.  |
| `ERR_CONNECTION_REFUSED` no frontend          | O backend não está na porta `5217`. Rode `dotnet run` e veja a porta real. |
| `415 Unsupported Media Type`                  | O campo do arquivo deve ser `Frame` (no .NET) e `file` (no Python). Ajuste o `FrameAnalysisService`. |
| `400 Bad Request`                             | Frame maior que 1 MB (ajuste o limite no controller ou reduza a imagem). |
| Swagger não abre                              | Certifique‑se de que o ambiente é `Development`. No `Program.cs` o Swagger só é habilitado nesse modo. |

## 📦 Estrutura de pastas (relevante)

```
Petzy.FrameReceiver/
├── Controllers/
│   └── FramesController.cs
├── Services/
│   ├── IFrameAnalysisService.cs
│   └── FrameAnalysisService.cs   # implementa chamada HTTP para o Python
├── DTOs/
│   ├── FrameUploadRequest.cs
│   └── FrameAnalysisResponse.cs
├── Storage/
│   └── FrameStore.cs             # armazenamento em memória
├── Program.cs
└── appsettings.json
```

---

Com esse README, outro desenvolvedor (ou você mesmo no futuro) conseguirá levantar o backend sem dúvidas. Basta seguir os passos e garantir que o serviço Python esteja ativo. 🚀