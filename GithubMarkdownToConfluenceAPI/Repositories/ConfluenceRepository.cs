using GithubMarkdownToConfluenceAPI.Models;
using GithubMarkdownToConfluenceAPI.Repositories.Interfaces;
using Markdig;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Text;
using System.Text.RegularExpressions;

namespace GithubMarkdownToConfluenceAPI.Repositories {
    public class ConfluenceRepository : IConfluenceRepository {
        private readonly ConfluenceConfig _config;

        public ConfluenceRepository(IOptions<ConfluenceConfig> config) {
            _config = config.Value;
        }

        public async Task<bool> PostToConfluence(string markdownContent, string fileName) {
            try {
                var client = new RestClient(_config.BaseUrl);
                var request = new RestRequest("rest/api/content", Method.Post);

                string htmlContent = Markdown.ToHtml(markdownContent);// Conversão de markdown para HTML via Markdig

                string authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.AuthToken));
                request.AddHeader("Authorization", $"Basic {authInfo}");
                request.AddHeader("Content-Type", "application/json");

                string pageTitle = $"{_config.PageTitle} - {fileName}";

                var body = new {
                    type = "page",
                    title = pageTitle,
                    space = new { key = _config.SpaceKey },
                    body = new {
                        storage = new {
                            value = htmlContent, // O conteúdo HTML convertido
                            representation = "storage" // Necessário para que o Confluence interprete o conteúdo como HTML
                        }
                    }
                };

                request.AddJsonBody(body);

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful) {
                    throw new Exception($"Falha ao postar no Confluence: {response.StatusCode} - {response.Content}");
                }

                return true;
            }
            catch (Exception ex) {
                Console.WriteLine($"Erro: {ex.Message}");
                return false;
            }
        }
    }
}