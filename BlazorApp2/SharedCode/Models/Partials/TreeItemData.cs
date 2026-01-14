namespace BlazorApp2.SharedCode.Models.Partials;

public class TreeItemData
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool IsExpanded { get; set; } = false;
    public bool HasChildren { get; set; } = false;
    public List<TreeItemData> Children { get; set; } = new();
    public bool IsLoading { get; set; } = false;
    public bool ChildrenLoaded { get; set; } = false;
    public bool IsDirectory { get; set; }
    public long Size { get; set; } = 0;
    public string Extension { get; set; } = string.Empty;
}