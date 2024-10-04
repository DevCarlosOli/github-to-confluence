using GithubMarkdownToConfluenceAPI.Models;
using GithubMarkdownToConfluenceAPI.Repositories.Interfaces;
using GithubMarkdownToConfluenceAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using Octokit;
using System.Text.RegularExpressions;

namespace GithubMarkdownToConfluenceAPI.Services {
    public class MarkdownService : IMarkdownService {
        private readonly IConfluenceRepository _confluenceRepository;
        private readonly string _tokenGitHub;

        public MarkdownService(IConfluenceRepository confluenceRepository, IOptions<GitHubConfig> gitHubConfig) {
            _confluenceRepository = confluenceRepository;
            _tokenGitHub = gitHubConfig.Value.Token;
        }

        public async Task<bool> ProcessMarkdownFromGitHub(string owner, string repository, string delimiter, string branch) {
            var client = new GitHubClient(new ProductHeaderValue("MarkdownToConfluence"));
            var tokenAuth = new Credentials(_tokenGitHub);
            client.Credentials = tokenAuth;

            try {
                // Chama o método recursivo para processar o conteúdo do repositório
                return await ProcessDirectoryContents(client, owner, repository, delimiter, branch);
            }
            catch (ApiException apiEx) {
                // Captura e exibe erros específicos da API do GitHub
                Console.WriteLine($"GitHub API error: {apiEx.Message}");
                throw new Exception($"GitHub API error: {apiEx.Message}", apiEx);
            }
            catch (Exception ex) {
                // Captura e exibe outros erros
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw new Exception($"An error occurred: {ex.Message}", ex);
            }            
        }

        private async Task<bool> ProcessDirectoryContents(GitHubClient client, string owner, string repository, string path, string branch) {
            var contents = await client.Repository.Content.GetAllContentsByRef(owner, repository, path, branch);

            foreach (var file in contents) {
                if (file.Type == ContentType.Dir) {
                    // Se for um diretório, entra no diretório e processa seu conteúdo
                    var result = await ProcessDirectoryContents(client, owner, repository, file.Path, branch);
                    if (!result)
                        return false;
                }
                else if (file.Type == ContentType.File && file.Name.EndsWith(".md")) {
                    // Se for um arquivo Markdown, processa o arquivo
                    var httpClient = new HttpClient();
                    var markdownContent = await httpClient.GetStringAsync(file.DownloadUrl);

                    if (!string.IsNullOrEmpty(markdownContent)) {
                        // Localizar e converter URLs de imagens para URLs absolutas do GitHub
                        markdownContent = ConvertRelativeImageUrlsToAbsolute(markdownContent, owner, repository, branch);
                        // Passa o conteúdo Markdown e o nome do arquivo para o Confluence
                        var foiPostado = await _confluenceRepository.PostToConfluence(markdownContent, file.Name);
                        if (!foiPostado) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private string ConvertRelativeImageUrlsToAbsolute(string markdownContent, string owner, string repository, string branch) {
            // Implementação para substituir URLs relativas por URLs absolutas do GitHub
            string baseUrl = $"https://raw.githubusercontent.com/{owner}/{repository}/{branch}";
            // Expressão regular para localizar imagens no formato Markdown
            string pattern = @"!\[([^\]]*)\]\(([^)]+)\)"; // Localiza ![Alt text](imagePath)

            return Regex.Replace(markdownContent, pattern, m => {
                string altText = m.Groups[1].Value;
                string imagePath = m.Groups[2].Value;

                if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://")) {
                    // Se for uma URL absoluta, retorna como está
                    return $"![{altText}]({imagePath})";
                }

                // Se for uma URL relativa, transforma em uma URL absoluta do GitHub
                string absoluteUrl = $"{baseUrl}/{imagePath.TrimStart('/')}";
                return $"![{altText}]({absoluteUrl})";
            });
        }
    }
}