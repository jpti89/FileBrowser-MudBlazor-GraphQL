using GraphQL;
using GraphQL.Types;
using BlazorApp2.SharedCode.Models.Partials;
using BlazorApp2.Server.GraphQlApi.GraphType;
using BlazorApp2.Server.Services.BuisnessLogic.Interfaces;

namespace BlazorApp2.Server.GraphQlApi.GraphQuery
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

            This.Field<ListGraphType<FileSystemItemGraphType>, List<FileSystemItem>>("getSubdirectories")
                .Argument<NonNullGraphType<StringGraphType>>("path")
                .ResolveAsync(async context =>
                {
                    var fileSystemService = context.RequestServices!.GetRequiredService<IFileSystemService>();
                    string path = context.GetArgument<string>("path");
                    return await fileSystemService.GetSubdirectoriesAsync(path);
                })
                .Description("Get subdirectories of a given path");

            // Get directory contents
            This.Field<ListGraphType<FileSystemItemGraphType>, List<FileSystemItem>>("getDirectoryContents")
                .Argument<NonNullGraphType<StringGraphType>>("path")
                .ResolveAsync(async context =>
                {
                    var fileSystemService = context.RequestServices!.GetRequiredService<IFileSystemService>();
                    string path = context.GetArgument<string>("path");
                    return await fileSystemService.GetDirectoryContentsAsync(path);
                })
                .Description("Get all contents (folders and files) of a given path");
        }
    }
}
