using GithubMarkdownToConfluenceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GithubMarkdownToConfluenceAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase {
        private readonly IMarkdownService _markdownService;

        public GitHubController(IMarkdownService markdownService) {
            _markdownService = markdownService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessMarkdownFromGitHub([FromQuery] string owner, [FromQuery] string repository, [FromQuery] string delimiter, [FromQuery] string branch) {
            try {
                // Chama o serviço para processar os arquivos Markdown do GitHub
                bool isSuccess = await _markdownService.ProcessMarkdownFromGitHub(owner, repository, delimiter, branch);

                if (isSuccess) {
                    return Ok("Arquivo Markdown processado e enviado para o Confluence.");
                }
                else {
                    return StatusCode(500, "Ocorreu um erro ao processar e enviar o arquivo Markdown para o Confluence.");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Erro: {ex.Message}");
                return StatusCode(500, "Um erro ocorreu enquanto os arquivos Markdown eram processados.");
            }
        }
    }
}