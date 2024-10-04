namespace GithubMarkdownToConfluenceAPI.Services.Interfaces {
    public interface IMarkdownService {
        Task<bool> ProcessMarkdownFromGitHub(string owner, string repository, string delimiter ,string branch);
    }
}
