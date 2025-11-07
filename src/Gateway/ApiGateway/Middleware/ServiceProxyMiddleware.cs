// Middleware responsável por rotear as requisições para os microserviços apropriados
// Atua como um proxy reverso direcionando chamadas baseado no path da URL

using SharedLibrary.DTOs;
using System.Text;
using System.Text.Json;

namespace ApiGateway.Middleware
{
    public class ServiceProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServiceProxyMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public ServiceProxyMiddleware(
            RequestDelegate next, 
            HttpClient httpClient, 
            ILogger<ServiceProxyMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            
            // Define qual serviço deve processar a requisição baseado no path
            string? targetServiceUrl = GetTargetServiceUrl(path);
            
            if (string.IsNullOrEmpty(targetServiceUrl))
            {
                // Se não é um path de microserviço, passa para o próximo middleware
                await _next(context);
                return;
            }

            try
            {
                await ProxyRequestAsync(context, targetServiceUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar requisição para o serviço: {ServiceUrl}", targetServiceUrl);
                await HandleErrorAsync(context, ex);
            }
        }

        // Determina qual serviço deve processar a requisição
        private string? GetTargetServiceUrl(string? path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var vendasBaseUrl = _configuration["ServiceEndpoints:VendasServiceUrl"];
            var estoqueBaseUrl = _configuration["ServiceEndpoints:EstoqueServiceUrl"];

            return path switch
            {
                var p when p.StartsWith("/api/vendas") => vendasBaseUrl,
                var p when p.StartsWith("/api/estoque") => estoqueBaseUrl,
                var p when p.StartsWith("/api/produtos") => estoqueBaseUrl, // Produtos são gerenciados pelo serviço de estoque
                _ => null
            };
        }

        // Encaminha a requisição para o microserviço apropriado
        private async Task ProxyRequestAsync(HttpContext context, string targetServiceUrl)
        {
            var request = context.Request;
            var targetUri = new Uri(new Uri(targetServiceUrl), request.Path + request.QueryString);

            using var httpRequestMessage = new HttpRequestMessage(
                new HttpMethod(request.Method), 
                targetUri);

            // Copia headers da requisição original
            foreach (var header in request.Headers)
            {
                if (!httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    httpRequestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            // Copia o corpo da requisição se existir
            if (request.ContentLength > 0)
            {
                var content = new StreamContent(request.Body);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    request.ContentType ?? "application/json");
                httpRequestMessage.Content = content;
            }

            // Envia a requisição para o microserviço
            using var response = await _httpClient.SendAsync(httpRequestMessage);
            
            // Copia a resposta de volta para o cliente
            context.Response.StatusCode = (int)response.StatusCode;
            
            foreach (var header in response.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in response.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            await response.Content.CopyToAsync(context.Response.Body);
        }

        // Trata erros de comunicação com os microserviços
        private async Task HandleErrorAsync(HttpContext context, Exception ex)
        {
            var response = new ApiResponse<object>
            {
                Sucesso = false,
                Mensagem = "Erro interno do servidor",
                Erros = new List<string> { "Falha na comunicação com o serviço" }
            };

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}