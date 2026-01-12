using Microsoft.AspNetCore.Components;
using MudBlazor;
using BlazorApp2.SharedCode.Models.Partials;

namespace BlazorApp2.Server.Blazor.Areas.Demo.Views
{
    public partial class FileBrowser
    {
        private List<string>? drives;
        private string? selectedDrive;
        private bool isLoadingDrives = true;
        private string? errorMessage;

        private List<TreeItemData> treeItems = new();
        private string? selectedTreeItem;

        private HashSet<string> includedPaths = new();
        private HashSet<string> excludedPaths = new();

        private bool multiselectionTextChoice;
        private string value { get; set; } = "Nothing selected";
        private IEnumerable<string> options { get; set; } = new HashSet<string>() { };

        private bool _visible;
        private readonly DialogOptions _dialogOptions = new() { FullWidth = true };

        private void OpenDialog() => _visible = true;
        private void Submit() => _visible = false;
        private void OnButtonClick()
        {
            LoadDriveTree();
            OpenDialog();
        }

        private string[] extensions =
        {
        "PNG",
        "JPEG",
        "TIFF",
        "PDF",
        "DOCX",
        "XLSX",
        "PPTX",
        "RTF"
    };

        private string GetMultiSelectionText(List<string> selectedValues)
        {
            if (multiselectionTextChoice)
            {
                return $"Selected Extension{(selectedValues.Count > 1 ? "s" : "")}: {string.Join(", ", selectedValues.Select(x => x))}";
            }
            else
            {
                return $"{selectedValues.Count} Extension{(selectedValues.Count > 1 ? "s have" : " has")} been selected";
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadDrives();
        }

        private async Task LoadDrives()
        {
            try
            {
                isLoadingDrives = true;
                drives = await FileSystemService.GetDrivesAsync();
                isLoadingDrives = false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading drives: {ex.Message}";
                isLoadingDrives = false;
            }
        }

        private async Task LoadDriveTree()
        {
            if (string.IsNullOrEmpty(selectedDrive))
                return;

            try
            {
                errorMessage = null;
                treeItems.Clear();

                var subdirs = await FileSystemService.GetSubdirectoriesAsync(selectedDrive);

                var rootItem = new TreeItemData
                {
                    Name = selectedDrive,
                    FullPath = selectedDrive,
                    HasChildren = subdirs.Any(),
                    ChildrenLoaded = true,
                    IsExpanded = false,
                    Children = subdirs.Select(d => new TreeItemData
                    {
                        Name = d.Name,
                        FullPath = d.FullPath,
                        HasChildren = true,
                        ChildrenLoaded = false,
                        Children = new List<TreeItemData>()
                    }).ToList()
                };

                treeItems.Add(rootItem);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading directory tree: {ex.Message}";
            }
        }

        private async Task LoadChildren(TreeItemData item)
        {
            if (item.IsLoading || item.ChildrenLoaded)
                return;

            try
            {
                item.IsLoading = true;
                StateHasChanged();

                var subdirs = await FileSystemService.GetSubdirectoriesAsync(item.FullPath);

                item.Children = subdirs.Select(d => new TreeItemData
                {
                    Name = d.Name,
                    FullPath = d.FullPath,
                    HasChildren = true,
                    ChildrenLoaded = false,
                    Children = new List<TreeItemData>()
                }).ToList();

                item.HasChildren = item.Children.Any();
                item.ChildrenLoaded = true;
                item.IsLoading = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading subdirectories: {ex.Message}";
                item.IsLoading = false;
                item.HasChildren = false;
                item.ChildrenLoaded = true;
                StateHasChanged();
            }
        }

        private void OnSelectedValueChanged(string value)
        {
            selectedTreeItem = value;
        }

        private void AddToIncluded()
        {
            if (!string.IsNullOrEmpty(selectedTreeItem) && !includedPaths.Contains(selectedTreeItem))
            {
                includedPaths.Add(selectedTreeItem);
                excludedPaths.Remove(selectedTreeItem);
            }
        }

        private void AddToExcluded()
        {
            if (!string.IsNullOrEmpty(selectedTreeItem) && !excludedPaths.Contains(selectedTreeItem))
            {
                excludedPaths.Add(selectedTreeItem);
                includedPaths.Remove(selectedTreeItem);
            }
        }

        private void RemoveFromIncluded(string path)
        {
            includedPaths.Remove(path);
        }

        private void RemoveFromExcluded(string path)
        {
            excludedPaths.Remove(path);
        }

        private RenderFragment RenderTreeNode(TreeItemData item) => builder =>
        {
            builder.OpenComponent<MudTreeViewItem<string>>(0);
            builder.AddAttribute(1, "Value", item.FullPath);
            builder.AddAttribute(2, "Text", item.Name);
            builder.AddAttribute(3, "Icon", Icons.Custom.Uncategorized.Folder);
            builder.AddAttribute(4, "IconExpanded", Icons.Custom.Uncategorized.FolderOpen);
            builder.AddAttribute(5, "CanExpand", item.HasChildren);
            builder.AddAttribute(6, "Expanded", item.IsExpanded);
            builder.AddAttribute(7, "ExpandedChanged", EventCallback.Factory.Create<bool>(this, async expanded =>
            {
                item.IsExpanded = expanded;

                if (expanded && !item.ChildrenLoaded)
                {
                    await LoadChildren(item);
                }

                StateHasChanged();
            }));

            if (item.HasChildren)
            {
                builder.AddAttribute(8, "ChildContent", (RenderFragment)(childBuilder =>
                {
                    if (item.IsLoading)
                    {
                        childBuilder.OpenComponent<MudTreeViewItem<string>>(0);
                        childBuilder.AddAttribute(1, "Text", "Loading...");
                        childBuilder.AddAttribute(2, "Icon", Icons.Material.Filled.HourglassEmpty);
                        childBuilder.AddAttribute(3, "CanExpand", false);
                        childBuilder.CloseComponent();
                    }
                    else if (item.ChildrenLoaded)
                    {
                        if (item.Children.Any())
                        {
                            foreach (var child in item.Children)
                            {
                                childBuilder.AddContent(0, RenderTreeNode(child));
                            }
                        }
                        else
                        {
                            childBuilder.OpenComponent<MudTreeViewItem<string>>(0);
                            childBuilder.AddAttribute(1, "Text", "Empty folder");
                            childBuilder.AddAttribute(2, "Icon", Icons.Material.Filled.FolderOff);
                            childBuilder.AddAttribute(3, "CanExpand", false);
                            childBuilder.CloseComponent();
                        }
                    }
                }));
            }

            builder.CloseComponent();
        };
    }
}
