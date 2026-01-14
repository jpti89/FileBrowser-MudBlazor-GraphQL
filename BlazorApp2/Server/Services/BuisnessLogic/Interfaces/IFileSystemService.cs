using BlazorApp2.SharedCode.Models.Partials;

namespace BlazorApp2.Server.Services.BuisnessLogic.Interfaces
{
    public interface IFileSystemService
    {
        Task<List<string>> GetDrivesAsync();
        Task<List<FileSystemItem>> GetSubdirectoriesAsync(string path);
        Task<List<FileSystemItem>> GetDirectoryContentsAsync(string path);
    }
}
