using GraphQL.Types;
using BlazorApp2.SharedCode.Models.Partials;

namespace BlazorApp2.Blazor.GraphQlApi.GraphType
{
    public class FileSystemItemGraphType : ObjectGraphType<FileSystemItem>
    {
        public FileSystemItemGraphType()
        {
            Name = "FileSystemItem";
            Description = "Represents a file or directory in the file system";

            Field(x => x.Name, nullable: false).Description("Name of the file or directory");
            Field(x => x.FullPath, nullable: false).Description("Full path of the file or directory");
            Field(x => x.IsDirectory, nullable: false).Description("Indicates if this is a directory");
            Field(x => x.Size, nullable: false).Description("Size in bytes (0 for directories)");
            Field(x => x.LastModified, nullable: false).Description("Last modification date");
            Field(x => x.Extension, nullable: false).Description("File extension (empty for directories)");
        }
    }
}