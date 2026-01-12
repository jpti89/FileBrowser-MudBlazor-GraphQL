using BlazorApp2.Components.Models;
using BlazorApp2.Components.Services;
using BlazorApp2.Components.Interfaces;

namespace BlazorApp2.Components.GraphQL
{
    public class Query
    {
        public async Task<List<string>> GetDrives([Service] IFileSystemService fileSystemService)
        {
            return await fileSystemService.GetDrivesAsync();
        }

        public async Task<FileSearchResult> CrawlDirectory(
            string path,
            string? searchPattern,
            int maxDepth,
            [Service] IFileSystemService fileSystemService)
        {
            return await fileSystemService.CrawlDirectoryAsync(path, searchPattern, maxDepth);
        }

        public async Task<List<FileSystemItem>> GetDirectoryContents(
            string path,
            [Service] IFileSystemService fileSystemService)
        {
            return await fileSystemService.GetDirectoryContentsAsync(path);
        }

        public async Task<List<FileSystemItem>> SearchFiles(
            string rootPath,
            string searchPattern,
            int maxDepth,
            [Service] IFileSystemService fileSystemService)
        {
            var result = await fileSystemService.CrawlDirectoryAsync(rootPath, searchPattern, maxDepth);
            return result.Items.Where(i => !i.IsDirectory).ToList();
        }

        public async Task<List<FileSystemItem>> GetSubdirectories(
            string path,
            [Service] IFileSystemService fileSystemService)
        {
            return await fileSystemService.GetSubdirectoriesAsync(path);
        }
    }
}
