using BlazorApp2.SharedCode.Models.Enums;
using BlazorApp2.SharedCode.Models.Partials;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace BlazorApp2.Server.Blazor.Areas.Demo.Views
{
    public partial class FileBrowser
    {
        private List<string>? drives;
        private List<TreeItemData> treeItems = new();

        private string? selectedDrive;
        private string currentPath = "C:\\";
        private string? errorMessage;
        private string? selectedTreeItem;
        private string? rightClickedPath;

        private bool isLoadingDrives = true;
        private bool multiSelectionExtensionsChoice;
        private bool _visible;
        private string value { get; set; } = "Nothing selected";

        private HashSet<string> includedPaths = new();
        private HashSet<string> excludedPaths = new();

        private IEnumerable<string> options { get; set; } = new HashSet<string>() { };

        private readonly DialogOptions _dialogOptions = new() { FullWidth = true, MaxWidth = MaxWidth.Large };

        private void OpenDialog() => _visible = true;
        private void Submit() => _visible = false;

        private async Task OnButtonClick()
        {
            await LoadDrives();

            if (drives != null && drives.Any())
            {
                selectedDrive = drives.FirstOrDefault(d => d.StartsWith("C:")) ?? drives.First();
                currentPath = selectedDrive;
                await LoadDriveTree();
            }
            OpenDialog();
        }

        List<string> extensions = Enum.GetNames(typeof(BrowserFileTypes)).ToList();

        private string GetMultiSelectionExtensionsChoice(List<string> selectedValues)
        {
            if (multiSelectionExtensionsChoice)
            {
                return $"Selected Extension{(selectedValues.Count > 1 ? "s" : "")}: {string.Join(", ", selectedValues.Select(x => x))}";
            }
            else
            {
                return $"{selectedValues.Count} Extension{(selectedValues.Count > 1 ? "s have" : " has")} been selected";
            }
        }


        private async Task LoadDrives()
        {
            try
            {
                isLoadingDrives = true;
                drives = await FileSystemService.GetDrivesAsync();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading drives: {ex.Message}";
            }
            finally
            {
                isLoadingDrives = false;
            }
        }

        private async Task OnDriveSelected(string drive)
        {
            selectedDrive = drive;
            currentPath = drive;
            await LoadDriveTree();
        }

        private async Task NavigateToPath()
        {
            if (string.IsNullOrEmpty(currentPath))
                return;

            selectedDrive = currentPath;
            await LoadDriveTree();
        }

        private async Task NavigateUp()
        {
            if (string.IsNullOrEmpty(currentPath))
                return;

            try
            {
                var parentPath = Directory.GetParent(currentPath)?.FullName;
                if (!string.IsNullOrEmpty(parentPath))
                {
                    currentPath = parentPath;
                    selectedDrive = currentPath;
                    await LoadDriveTree();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Cannot navigate up: {ex.Message}";
            }
        }

        private async Task RefreshCurrentPath()
        {
            await LoadDriveTree();
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
            currentPath = value;
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
            builder.OpenComponent<MudMenu>(0);
            builder.AddAttribute(1, "ActivationEvent", MouseEvent.RightClick);
            builder.AddAttribute(2, "PositionAtCursor", true);
            builder.AddAttribute(3, "Style", "display: block; width: 100%;");
            builder.AddAttribute(4, "ActivatorContent", (RenderFragment)(activatorBuilder =>
            {
                activatorBuilder.OpenComponent<MudTreeViewItem<string>>(0);
                activatorBuilder.AddAttribute(1, "Value", item.FullPath);
                activatorBuilder.AddAttribute(2, "Text", item.Name);
                activatorBuilder.AddAttribute(3, "Icon", Icons.Custom.Uncategorized.Folder);
                activatorBuilder.AddAttribute(4, "IconExpanded", Icons.Custom.Uncategorized.FolderOpen);
                activatorBuilder.AddAttribute(5, "CanExpand", item.HasChildren);
                activatorBuilder.AddAttribute(6, "Expanded", item.IsExpanded);
                activatorBuilder.AddAttribute(7, "ExpandedChanged", EventCallback.Factory.Create<bool>(this, async expanded =>
                {
                    item.IsExpanded = expanded;
                    if (expanded && !item.ChildrenLoaded)
                    {
                        await LoadChildren(item);
                    }
                    StateHasChanged();
                }));

                activatorBuilder.AddAttribute(8, "oncontextmenu", EventCallback.Factory.Create<MouseEventArgs>(this, (args) =>
                {
                    rightClickedPath = item.FullPath;
                }));

                if (item.HasChildren)
                {
                    activatorBuilder.AddAttribute(9, "ChildContent", (RenderFragment)(childBuilder =>
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

                activatorBuilder.CloseComponent();
            }));

            builder.AddAttribute(5, "ChildContent", (RenderFragment)(menuBuilder =>
            {
                menuBuilder.OpenComponent<MudMenuItem>(0);
                menuBuilder.AddAttribute(1, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, (MouseEventArgs args) => AddToIncludedFromContext(item.FullPath)));
                menuBuilder.AddAttribute(2, "ChildContent", (RenderFragment)((textBuilder) =>
                {
                    textBuilder.AddContent(0, "Add Folder");
                }));
                menuBuilder.CloseComponent();
                
                menuBuilder.OpenComponent<MudMenuItem>(1);
                menuBuilder.AddAttribute(1, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, (MouseEventArgs args) => AddToExcludedFromContext(item.FullPath)));
                menuBuilder.AddAttribute(2, "ChildContent", (RenderFragment)((textBuilder) =>
                {
                    textBuilder.AddContent(0, "Exclude Folder");
                }));
                menuBuilder.CloseComponent();
            }));

            builder.CloseComponent();
        };

        private void AddToIncludedFromContext(string path)
        {
            if (!string.IsNullOrEmpty(path) && !includedPaths.Contains(path))
            {
                includedPaths.Add(path);
                excludedPaths.Remove(path);
            }
        }

        private void AddToExcludedFromContext(string path)
        {
            if (!string.IsNullOrEmpty(path) && !excludedPaths.Contains(path))
            {
                excludedPaths.Add(path);
                includedPaths.Remove(path);
            }
        }
    }
}