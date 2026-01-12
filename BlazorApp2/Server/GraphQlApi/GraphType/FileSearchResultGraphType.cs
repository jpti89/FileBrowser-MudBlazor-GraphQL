using GraphQL.Types;
using BlazorApp2.SharedCode.Models.Partials;

namespace BlazorApp2.Server.GraphQlApi.GraphType
{
    public class FileSearchResultGraphType : ObjectGraphType<FileSearchResult>
    {
        public FileSearchResultGraphType()
        {
            Name = "FileSearchResult";
            Description = "Result of a directory crawl operation";

            Field<ListGraphType<FileSystemItemGraphType>, List<FileSystemItem>>("items")
                .Resolve(context => context.Source.Items)
                .Description("List of files and directories found");

            Field(x => x.TotalCount, nullable: false).Description("Total number of items found");
            Field(x => x.SearchPath, nullable: false).Description("The path that was searched");
        }
    }
}