using GraphQL;
using GraphQL.Types;
using BlazorApp2.SharedCode.Models.Partials;
using BlazorApp2.GraphQlApi.GraphType;
using BlazorApp2.Services.BuisnessLogic.Interfaces;

namespace BlazorApp2.GraphQlApi.GraphQuery
{
    public static class FileSystemQuery
    {
        public static void AddFileSystemQuery(this ObjectGraphType This)
        {
            // Get all available drives
            This.Field<ListGraphType<StringGraphType>, List<string>>("getDrives")
                .ResolveAsync(async context =>
                {
                    var fileSystemService = context.RequestServices!.GetRequiredService<IFileSystemService>();
                    return await fileSystemService.GetDrivesAsync();
                })
                .Description("Get all available drives on the system");

            // Crawl directory with search pattern
            This.Field<FileSearchResultGraphType, FileSearchResult>("crawlDirectory")
                .Argument<NonNullGraphType<StringGraphType>>("path")
                .Argument<StringGraphType>("searchPattern")
                .Argument<NonNullGraphType<IntGraphType>>("maxDepth")
                .ResolveAsync(async context =>
                {
                    var fileSystemService = context.RequestServices!.GetRequiredService<IFileSystemService>();
                    string path = context.GetArgument<string>("path");
                    string? searchPattern = context.GetArgument<string?>("searchPattern");
                    int maxDepth = context.GetArgument<int>("maxDepth");
                    return await fileSystemService.CrawlDirectoryAsync(path, searchPattern, maxDepth);
                })
                .Description("Crawl a directory recursively with optional search pattern");

            // Get directory contents (non-recursive)
            This.Field<ListGraphType<FileSystemItemGraphType>, List<FileSystemItem>>("getDirectoryContents")
                .Argument<NonNullGraphType<StringGraphType>>("path")
                .ResolveAsync(async context =>
                {
                    var fileSystemService = context.RequestServices!.GetRequiredService<IFileSystemService>();
                    string path = context.GetArgument<string>("path");
                    return await fileSystemService.GetDirectoryContentsAsync(path);
                })
                .Description("Get immediate contents of a directory");

            // Search for specific files
            This.Field<ListGraphType<FileSystemItemGraphType>, List<FileSystemItem>>("searchFiles")
                .Argument<NonNullGraphType<StringGraphType>>("rootPath")
                .Argument<NonNullGraphType<StringGraphType>>("searchPattern")
                .Argument<NonNullGraphType<IntGraphType>>("maxDepth")
                .ResolveAsync(async context =>
                {
                    var fileSystemService = context.RequestServices!.GetRequiredService<IFileSystemService>();
                    string rootPath = context.GetArgument<string>("rootPath");
                    string searchPattern = context.GetArgument<string>("searchPattern");
                    int maxDepth = context.GetArgument<int>("maxDepth");

                    var result = await fileSystemService.CrawlDirectoryAsync(rootPath, searchPattern, maxDepth);
                    return result.Items.Where(i => !i.IsDirectory).ToList();
                })
                .Description("Search for files matching a pattern");

            // Get subdirectories only
            This.Field<ListGraphType<FileSystemItemGraphType>, List<FileSystemItem>>("getSubdirectories")
                .Argument<NonNullGraphType<StringGraphType>>("path")
                .ResolveAsync(async context =>
                {
                    var fileSystemService = context.RequestServices!.GetRequiredService<IFileSystemService>();
                    string path = context.GetArgument<string>("path");
                    return await fileSystemService.GetSubdirectoriesAsync(path);
                })
                .Description("Get subdirectories of a given path");
        }
    }
}
