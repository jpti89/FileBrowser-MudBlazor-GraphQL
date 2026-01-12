using BlazorApp2.Components.Models;

namespace BlazorApp2.Components.Interfaces
{
    public interface IFileSystemService
    {
        Task<List<string>> GetDrivesAsync();
        Task<FileSearchResult> CrawlDirectoryAsync(string path, string? searchPattern = null, int maxDepth = 3);
        Task<List<FileSystemItem>> GetDirectoryContentsAsync(string path);
        Task<List<FileSystemItem>> GetSubdirectoriesAsync(string path);
    }
}
