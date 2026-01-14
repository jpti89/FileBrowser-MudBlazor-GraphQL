namespace BlazorApp2.SharedCode.Models.Partials
{
    public class FolderContextMenuItem
    {
        public string Text { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class FolderContextSelectEvent
    {
        public string MenuItemText { get; set; } = string.Empty;
        public TreeItemData? FileNode { get; set; }
    }
}