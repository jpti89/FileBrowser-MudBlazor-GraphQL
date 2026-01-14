using BlazorApp2.Server.Blazor.Areas.Demo.Components;
using BlazorApp2.SharedCode.Models.Partials;

namespace BlazorApp2.Server.Blazor.Areas.Demo.Views
{
    public partial class FileExplorerComponentUsage
    {
        private bool browserOpen;
        private FileExplorerDialog? fileExplorer;

        private HashSet<string> includedPaths { get; set; } = new();
        private HashSet<string> excludedPaths { get; set; } = new();

        private List<FolderContextMenuItem> folderContextMenu = new()
        {
            new FolderContextMenuItem { Text = "Add Folder", Icon = "fa-solid fa-folder-plus" },
            new FolderContextMenuItem { Text = "Exclude Folder", Icon = "fa-solid fa-folder-minus" },
            new FolderContextMenuItem { Text = "Focus Folder", Icon = "fa-solid fa-magnifying-glass" }
        };

        private async Task OpenExplorerDialog()
        {
            browserOpen = true;
        }

        private void OnExplorerClose()
        {
            browserOpen = false;
        }

        private void OnFolderContextSelect(FolderContextSelectEvent eventArgs)
        {
            if (eventArgs.FileNode == null || string.IsNullOrEmpty(eventArgs.FileNode.FullPath))
                return;

            switch (eventArgs.MenuItemText)
            {
                case "Add Folder":
                    includedPaths.Add(eventArgs.FileNode.FullPath);
                    excludedPaths.Remove(eventArgs.FileNode.FullPath);
                    break;

                case "Exclude Folder":
                    excludedPaths.Add(eventArgs.FileNode.FullPath);
                    includedPaths.Remove(eventArgs.FileNode.FullPath);
                    break;

                case "Focus Folder":
                    fileExplorer?.SetViewPath(eventArgs.FileNode.FullPath);
                    break;

            }

            StateHasChanged();
        }
    }
}