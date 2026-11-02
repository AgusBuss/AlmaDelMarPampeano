using System.Net;
using System.Text.Json;

namespace AlmaDelMarPampeano.Services
{
    // Un mensaje de la conversación. Rol: "user" (la persona) o "model" (el asistente).
    public record MensajeChat(string Rol, string Texto);

    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly string? _apiKey;
        private readonly List<string> _modelos;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(HttpClient http, IConfiguration configuracion, ILogger<GeminiService> logger)
        {
            _http = http;
            _logger = logger;

            // La clave viene de User Secrets (desarrollo) o de la configuración del servidor (producción)
            _apiKey = configuracion["Gemini:ApiKey"];

            // Primero el modelo principal; si está saturado, se prueban los alternativos en orden
            _modelos = new List<string> { configuracion["Gemini:Model"] ?? "gemini-3.8-flash" };
            var alternativos = configuracion.GetSection("Gemini:ModelosAlternativos").Get<string[]>();
            if (alternativos != null) _modelos.AddRange(alternativos);
            _modelos = _modelos.Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToList();
        }

        public async Task<string> ResponderAsync(
            string instruccionSistema,
            IReadOnlyList<MensajeChat> historial,
            CancellationToken cancelacion = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("Falta la configuración Gemini:ApiKey.");
                return "El asistente todavía no está configurado. Probá de nuevo más tarde.";
            }

            var cuerpo = new
            {
                system_instruction = new { parts = new[] { new { text = instruccionSistema } } },
                contents = historial.Select(m => new
                {
                    role = m.Rol,
                    parts = new[] { new { text = m.Texto } }
                }).ToList(),
                generationConfig = new { maxOutputTokens = 1024, temperature = 0.4 }
            };

            var ultimoCodigo = 0;

            foreach (var modelo in _modelos)
            {
                for (var intento = 1; intento <= 2; intento++)
                {
                    var (codigo, texto) = await EnviarAsync(modelo, cuerpo, cancelacion);

                    if (codigo == 200)
                    {
                        _logger.LogInformation("Gemini respondió con el modelo {Modelo}.", modelo);
                        return texto ?? "No encontré una respuesta para eso. ¿Podés reformular la pregunta?";
                    }

                    ultimoCodigo = codigo;

                    // 503 = modelo saturado (es temporal): se reintenta una vez.
                    // Cualquier otro error pasa directo al siguiente modelo.
                    if (codigo != 503) break;
                    if (intento == 1) await Task.Delay(1500, cancelacion);
                }
            }

            return ultimoCodigo == (int)HttpStatusCode.TooManyRequests
                ? "Hay muchas consultas en este momento. Esperá un minuto y volvé a preguntar."
                : "No pude responder en este momento. Probá de nuevo en unos minutos.";
        }

        // Devuelve el código HTTP (0 si no hubo conexión) y el texto de la respuesta, si lo hay.
        private async Task<(int Codigo, string? Texto)> EnviarAsync(
            string modelo, object cuerpo, CancellationToken cancelacion)
        {
            using var pedido = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{modelo}:generateContent");
            pedido.Headers.Add("x-goog-api-key", _apiKey);
            pedido.Content = JsonContent.Create(cuerpo);

            try
            {
                using var respuesta = await _http.SendAsync(pedido, cancelacion);
                var json = await respuesta.Content.ReadAsStringAsync(cancelacion);

                if (!respuesta.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini ({Modelo}) respondió {Codigo}: {Cuerpo}", modelo, (int)respuesta.StatusCode, json);
                    return ((int)respuesta.StatusCode, null);
                }

                return (200, ExtraerTexto(json));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancelacion.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "No se pudo conectar con Gemini ({Modelo}).", modelo);
                return (0, null);
            }
        }

        // La respuesta trae candidates[0].content.parts[]. Se unen las partes de texto
        // (se saltean las partes de "pensamiento" interno del modelo, si las hay).
        private static string? ExtraerTexto(string json)
        {
            using var documento = JsonDocument.Parse(json);

            if (!documento.RootElement.TryGetProperty("candidates", out var candidatos) ||
                candidatos.GetArrayLength() == 0 ||
                !candidatos[0].TryGetProperty("content", out var contenido) ||
                !contenido.TryGetProperty("parts", out var partes))
            {
                return null;
            }

            var textos = new List<string>();
            foreach (var parte in partes.EnumerateArray())
            {
                if (parte.TryGetProperty("thought", out var pensamiento) && pensamiento.ValueKind == JsonValueKind.True)
                    continue;

                if (parte.TryGetProperty("text", out var texto) && texto.GetString() is { Length: > 0 } valor)
                    textos.Add(valor);
            }

            return textos.Count > 0 ? string.Concat(textos).Trim() : null;
        }
    }
}