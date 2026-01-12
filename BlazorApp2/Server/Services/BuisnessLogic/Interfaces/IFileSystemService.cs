using BlazorApp2.SharedCode.Models.Partials;

namespace BlazorApp2.Server.Services.BuisnessLogic.Interfaces
{
    public interface IFileSystemService
    {
        Task<List<string>> GetDrivesAsync();
        Task<FileSearchResult> CrawlDirectoryAsync(string path, string? searchPattern = null, int maxDepth = 3);
        Task<List<FileSystemItem>> GetDirectoryContentsAsync(string path);
        Task<List<FileSystemItem>> GetSubdirectoriesAsync(string path);
    }
}
