using BlazorApp2.Blazor.Services.BuisnessLogic.Interfaces;
using BlazorApp2.SharedCode.Models.Partials;

namespace BlazorApp2.Blazor.Services.BuisnessLogic.Implementations
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

        public async Task<FileSearchResult> CrawlDirectoryAsync(
            string path,
            string? searchPattern = null,
            int maxDepth = 3)
        {
            var result = new FileSearchResult
            {
                SearchPath = path,
                Items = new List<FileSystemItem>()
            };

            if (!Directory.Exists(path))
            {
                return result;
            }

            await Task.Run(() => CrawlRecursive(path, searchPattern, maxDepth, 0, result.Items));
            result.TotalCount = result.Items.Count;

            return result;
        }

        private void CrawlRecursive(
            string currentPath,
            string? searchPattern,
            int maxDepth,
            int currentDepth,
            List<FileSystemItem> items)
        {
            if (currentDepth > maxDepth)
                return;

            try
            {
                // Get files
                var files = string.IsNullOrEmpty(searchPattern)
                    ? Directory.GetFiles(currentPath)
                    : Directory.GetFiles(currentPath, searchPattern);

                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        items.Add(new FileSystemItem
                        {
                            Name = fileInfo.Name,
                            FullPath = fileInfo.FullName,
                            IsDirectory = false,
                            Size = fileInfo.Length,
                            LastModified = fileInfo.LastWriteTime,
                            Extension = fileInfo.Extension
                        });
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (Exception) { }
                }

                // Get subdirectories
                var directories = Directory.GetDirectories(currentPath);
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

                        // Recurse into subdirectory
                        CrawlRecursive(dir, searchPattern, maxDepth, currentDepth + 1, items);
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (Exception) { }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception) { }
        }

        public Task<List<FileSystemItem>> GetDirectoryContentsAsync(string path)
        {
            var items = new List<FileSystemItem>();

            if (!Directory.Exists(path))
            {
                return Task.FromResult(items);
            }

            try
            {
                // Add directories
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
                    catch { }
                }

                // Add files
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        items.Add(new FileSystemItem
                        {
                            Name = fileInfo.Name,
                            FullPath = fileInfo.FullName,
                            IsDirectory = false,
                            Size = fileInfo.Length,
                            LastModified = fileInfo.LastWriteTime,
                            Extension = fileInfo.Extension
                        });
                    }
                    catch { }
                }
            }
            catch { }

            return Task.FromResult(items);
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
