using BlazorApp2.SharedCode.Models.Enums;
using BlazorApp2.SharedCode.Models.Partials;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace BlazorApp2.Server.Blazor.Areas.Demo.Views
{
    public partial class FileBrowser
    {
        private List<string>? drives;
        private List<TreeItemData> treeItems = new();

        private string? selectedDrive;
        private string? currentPath;
        private string? errorMessage;
        private string? selectedTreeItem;
        private string? rightClickedPath;

        private bool isLoadingDrives = true;
        private bool _visibleDialog;

        private void OpenDialog() => _visibleDialog = true;
        private void Submit() => _visibleDialog = false;

        private readonly DialogOptions _dialogOptions = new() { FullWidth = true, MaxWidth = MaxWidth.Large };

        List<string> extensions = Enum.GetNames(typeof(BrowserFileTypes)).ToList();

        private HashSet<string> includedPaths { get; set; } = new();
        private HashSet<string> excludedPaths { get; set; } = new();
        private IEnumerable<string> ExtensionOptions { get; set; } = new HashSet<string>() { };

        private async Task OnExplorerSelectorButtonClick()
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

        private string GetMultiSelectionExtensionsChoice(List<string> selectedValues)
        {
            return string.Join(", ", selectedValues);
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
                await Task.Delay(1);
                StateHasChanged();

                var contents = await FileSystemService.GetDirectoryContentsAsync(selectedDrive);
                var subdirs = contents.Where(c => c.IsDirectory).ToList();
                var files = contents.Where(c => !c.IsDirectory).ToList();

                var displayName = selectedDrive.TrimEnd('\\').Split('\\').LastOrDefault() ?? selectedDrive;
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = selectedDrive;
                }

                List<TreeItemData> children = new List<TreeItemData>();

                if (subdirs.Any())
                {
                    var childrenTasks = subdirs.Select(async d =>
                    {
                        try
                        {
                            var childSubdirs = await FileSystemService.GetSubdirectoriesAsync(d.FullPath);
                            return new TreeItemData
                            {
                                Name = d.Name,
                                FullPath = d.FullPath,
                                IsDirectory = true,
                                HasChildren = childSubdirs.Any(),
                                ChildrenLoaded = false,
                                Children = new List<TreeItemData>()
                            };
                        }
                        catch
                        {
                            return new TreeItemData
                            {
                                Name = d.Name,
                                FullPath = d.FullPath,
                                IsDirectory = true,
                                HasChildren = false,
                                ChildrenLoaded = false,
                                Children = new List<TreeItemData>()
                            };
                        }
                    });

                    var folderChildren = await Task.WhenAll(childrenTasks);

                    var fileChildren = files.Select(f => new TreeItemData
                    {
                        Name = f.Name,
                        FullPath = f.FullPath,
                        IsDirectory = false,
                        HasChildren = false,
                        ChildrenLoaded = true,
                        Size = f.Size,
                        Extension = f.Extension,
                        Children = new List<TreeItemData>()
                    });

                    children = folderChildren.Concat(fileChildren).ToList();
                }

                var rootItem = new TreeItemData
                {
                    Name = displayName,
                    FullPath = selectedDrive,
                    IsDirectory = true,
                    HasChildren = subdirs.Any() || files.Any(),
                    ChildrenLoaded = true,
                    IsExpanded = false,
                    Children = children
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
            if (item.IsLoading || item.ChildrenLoaded || !item.IsDirectory)
                return;

            try
            {
                item.IsLoading = true;
                StateHasChanged();

                var contents = await FileSystemService.GetDirectoryContentsAsync(item.FullPath);

                var directories = contents.Where(c => c.IsDirectory).ToList();
                var files = contents.Where(c => !c.IsDirectory).ToList();

                var childrenTasks = directories.Select(async d =>
                {
                    try
                    {
                        var childSubdirs = await FileSystemService.GetSubdirectoriesAsync(d.FullPath);
                        return new TreeItemData
                        {
                            Name = d.Name,
                            FullPath = d.FullPath,
                            IsDirectory = true,
                            HasChildren = childSubdirs.Any(),
                            ChildrenLoaded = false,
                            Children = new List<TreeItemData>()
                        };
                    }
                    catch
                    {
                        return new TreeItemData
                        {
                            Name = d.Name,
                            FullPath = d.FullPath,
                            IsDirectory = true,
                            HasChildren = false,
                            ChildrenLoaded = false,
                            Children = new List<TreeItemData>()
                        };
                    }
                });

                var folderChildren = await Task.WhenAll(childrenTasks);

                var fileChildren = files.Select(f => new TreeItemData
                {
                    Name = f.Name,
                    FullPath = f.FullPath,
                    IsDirectory = false,
                    HasChildren = false,
                    ChildrenLoaded = true,
                    Size = f.Size,
                    Extension = f.Extension,
                    Children = new List<TreeItemData>()
                });

                item.Children = folderChildren.Concat(fileChildren).ToList();
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
                RenderTreeViewItem(activatorBuilder, item);
            }));

            if (item.IsDirectory)
            {
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

                    menuBuilder.OpenComponent<MudMenuItem>(2);
                    menuBuilder.AddAttribute(1, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, (MouseEventArgs args) => FocusFromContext(item.FullPath)));
                    menuBuilder.AddAttribute(2, "ChildContent", (RenderFragment)((textBuilder) =>
                    {
                        textBuilder.AddContent(0, "Focus Folder");
                    }));
                    menuBuilder.CloseComponent();
                }));

                builder.CloseComponent();
            }
            else
            {
                builder.AddAttribute(5, "ChildContent", (RenderFragment)(menuBuilder =>
                {
                    menuBuilder.OpenComponent<MudMenuItem>(0);
                    menuBuilder.AddAttribute(1, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, (MouseEventArgs args) => AddToIncludedFromContext(item.FullPath)));
                    menuBuilder.AddAttribute(2, "ChildContent", (RenderFragment)((textBuilder) =>
                    {
                        textBuilder.AddContent(0, "Select File");
                    }));
                    menuBuilder.CloseComponent();
                }));

                builder.CloseComponent();
            }
        };

        private void RenderTreeViewItem(RenderTreeBuilder builder, TreeItemData item)
        {
            builder.OpenComponent<MudTreeViewItem<string>>(0);
            builder.AddAttribute(1, "Value", item.FullPath);
            builder.AddAttribute(2, "Text", item.Name);

            if (item.IsDirectory)
            {
                builder.AddAttribute(3, "Icon", @"fa-solid fa-folder");
                builder.AddAttribute(4, "IconExpanded", @"fa-solid fa-folder-open");
            }
            else
            {
                builder.AddAttribute(3, "Icon", GetFileIcon(item.Extension));
            }

            builder.AddAttribute(5, "CanExpand", item.HasChildren);
            builder.AddAttribute(6, "Expanded", item.IsExpanded);
            builder.AddAttribute(7, "ExpandedChanged", EventCallback.Factory.Create<bool>(this, async expanded =>
            {
                item.IsExpanded = expanded;
                if (expanded && !item.ChildrenLoaded && item.IsDirectory)
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
                        childBuilder.AddAttribute(2, "Icon", @"fa-hourglass");
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
                    }
                }));
            }

            builder.CloseComponent();
        }

        private string GetFileIcon(string extension)
        {
            return extension.ToLower() switch
            {
                ".txt" => @"fa-solid fa-file-lines",
                ".csv" => @"fa-solid fa-file-csv",
                ".pdf" => @"fa-solid fa-file-pdf",
                ".doc" or ".docx" => @"fa-solid fa-file-word",
                ".xls" or ".xlsx" => @"fa-solid fa-file-excel",
                ".pptx" => @"fa-solid fa-file-powerpoint",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".tiff" => @"fa-solid fa-file-image",
                ".mp3" or ".wav" => @"fa-solid fa-file-audio",
                ".mp4" or ".avi" => @"fa-solid fa-file-video",
                ".zip" or ".rar" => @"fa-solid fa-file-zipper",
                ".exe" => @"fa-solid fa-file",
                _ => @"fa-solid fa-file",
            };
        }

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
        private async Task FocusFromContext(string path)
        {
            if (!string.IsNullOrEmpty(path) && !includedPaths.Contains(path))
            {
                currentPath = path;
                selectedDrive = path;
                await LoadDriveTree();
            }
        }
    }
}