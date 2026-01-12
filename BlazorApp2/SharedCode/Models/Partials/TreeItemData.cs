namespace BlazorApp2.SharedCode.Models.Partials;

public class TreeItemData
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool IsExpanded { get; set; } = false;
    public bool HasChildren { get; set; } = true;
    public List<TreeItemData> Children { get; set; } = new();
    public bool IsLoading { get; set; } = false;
}