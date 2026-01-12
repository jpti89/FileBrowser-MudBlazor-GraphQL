using BlazorApp2.Server.Services.BuisnessLogic.Interfaces;
using BlazorApp2.SharedCode.Models.Partials;

namespace BlazorApp2.Server.Services.BuisnessLogic.Implementations
{
    public class FileSystemService : IFileSystemService
    {
        public Task<List<string>> GetDrivesAsync()
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => d.Name)
                .ToList();

            return Task.FromResult(drives);
        }

        public Task<List<FileSystemItem>> GetSubdirectoriesAsync(string path)
        {
            var items = new List<FileSystemItem>();

            if (!Directory.Exists(path))
            {
                return Task.FromResult(items);
            }

            try
            {
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        items.Add(new FileSystemItem
                        {
                            Name = dirInfo.Name,
                            FullPath = dirInfo.FullName,
                            IsDirectory = true,
                            Size = 0,
                            LastModified = dirInfo.LastWriteTime,
                            Extension = string.Empty
                        });
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (Exception) { }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception) { }

            return Task.FromResult(items);
        }
    }
}
