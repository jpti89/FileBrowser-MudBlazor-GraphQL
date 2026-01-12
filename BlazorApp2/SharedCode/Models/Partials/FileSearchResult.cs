namespace BlazorApp2.SharedCode.Models.Partials
{
    public class FileSearchResult
    {
        public List<FileSystemItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public string SearchPath { get; set; } = string.Empty;
    }
}
