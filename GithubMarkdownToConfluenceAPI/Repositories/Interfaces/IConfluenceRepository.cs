namespace GithubMarkdownToConfluenceAPI.Repositories.Interfaces {
    public interface IConfluenceRepository {
        Task<bool> PostToConfluence(string markdownContent, string fileName);
    }
}