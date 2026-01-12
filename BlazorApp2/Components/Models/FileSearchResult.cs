namespace BlazorApp2.Components.Models
{
    public class FileSearchResult
    {
        public List<FileSystemItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public string SearchPath { get; set; } = string.Empty;
    }
}
