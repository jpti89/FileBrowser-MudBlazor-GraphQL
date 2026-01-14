using BlazorApp2.SharedCode.Models.Partials;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using BlazorApp2.Server.Services.BuisnessLogic.Interfaces;

namespace BlazorApp2.Server.Blazor.Areas.Demo.Components
{
    public partial class FileExplorerDialog : ComponentBase
    {
        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
        [Parameter] public List<FolderContextMenuItem> FolderContextMenuItems { get; set; } = new();
        [Parameter] public List<FolderContextMenuItem> FileContextMenuItems { get; set; } = new();
        [Parameter] public EventCallback<FolderContextSelectEvent> OnFolderContextSelect { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public string? InitialPath { get; set; }

        private List<string>? drives;
        private List<TreeItemData> treeItems = new();
        private string? selectedDrive;
        private string? currentPath;
        private string? errorMessage;
        private string? selectedTreeItem;
        private bool isLoadingDrives = true;
        private readonly DialogOptions _dialogOptions = new() { FullWidth = true, MaxWidth = MaxWidth.Large };

        protected override async Task OnParametersSetAsync()
        {
            if (IsVisible && drives == null)
            {
                await LoadDrives();

                if (!string.IsNullOrEmpty(InitialPath))
                {
                    currentPath = InitialPath;
                    selectedDrive = InitialPath;
                }
                else if (drives != null && drives.Any())
                {
                    selectedDrive = drives.FirstOrDefault(d => d.StartsWith("C:")) ?? drives.First();
                    currentPath = selectedDrive;
                }
                if (!string.IsNullOrEmpty(selectedDrive))
                {
                    await LoadDriveTree();
                }
            }
        }

        public async Task OpenAsync(string? initialPath = null)
        {
            if (!string.IsNullOrEmpty(initialPath))
            {
                InitialPath = initialPath;
            }

            IsVisible = true;
            await IsVisibleChanged.InvokeAsync(IsVisible);
            StateHasChanged();
        }

        public string? GetCurrentPath() => currentPath;

        public void SetViewPath(string path)
        {
            currentPath = path;
            selectedDrive = path;
            _ = LoadDriveTree();
        }

        private async Task Close()
        {
            IsVisible = false;
            await IsVisibleChanged.InvokeAsync(IsVisible);
            await OnClose.InvokeAsync();
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

        private async Task HandleContextMenuClick(string menuItemText, TreeItemData item)
        {
            var contextEvent = new FolderContextSelectEvent
            {
                MenuItemText = menuItemText,
                FileNode = item
            };

            await OnFolderContextSelect.InvokeAsync(contextEvent);
        }

        private RenderFragment RenderTreeNode(TreeItemData item) => builder =>
        {
            if ((item.IsDirectory && FolderContextMenuItems.Count() > 0) || (!item.IsDirectory && FileContextMenuItems.Count() > 0))
            { 
            builder.OpenComponent<MudMenu>(0);
            builder.AddAttribute(1, "ActivationEvent", MouseEvent.RightClick);
            builder.AddAttribute(2, "PositionAtCursor", true);
            builder.AddAttribute(3, "Style", "display: block; width: 100%;");
            builder.AddAttribute(4, "ActivatorContent", (RenderFragment)(activatorBuilder =>
            {
                RenderTreeViewItem(activatorBuilder, item);
            }));

            builder.AddAttribute(5, "ChildContent", (RenderFragment)(menuBuilder =>
            {
                    var childItems = item.IsDirectory ? FolderContextMenuItems : FileContextMenuItems;
                    for (int i = 0; i < childItems.Count; i++)
                    {
                        var menuItem = childItems[i];
                        var index = i;

                        menuBuilder.OpenComponent<MudMenuItem>(index);
                        menuBuilder.AddAttribute(1, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, async (MouseEventArgs args) =>
                        {
                            await HandleContextMenuClick(menuItem.Text, item);
                        }));
                        menuBuilder.AddAttribute(2, "ChildContent", (RenderFragment)((textBuilder) =>
                        {
                            if (!string.IsNullOrEmpty(menuItem.Icon))
                            {
                                textBuilder.OpenElement(0, "i");
                                textBuilder.AddAttribute(1, "class", $"{menuItem.Icon} me-2");
                                textBuilder.CloseElement();
                            }
                            textBuilder.AddContent(1, menuItem.Text);
                        }));
                        menuBuilder.CloseComponent();
                    }
                
            }));
                builder.CloseComponent();

            }
            else
            {

                builder.OpenElement(0,"div");
                builder.AddAttribute(1,"class", "mud-menu");
                builder.AddAttribute(2, "style", "display: block; width: 100%;");
                builder.AddMarkupContent(3, @$"
<div class=""mud -menu-activator"" tabindex=""0"" role=""button"" aria-haspopup=""true"">
<li class=""mud-treeview-item"">
<div class=""mud-treeview-item-content cursor-pointer mud-ripple"">    
<div class=""mud-treeview-item-arrow""></div>
<div class=""mud-treeview-item-icon"">
<span class=""mud-icon-root mud-icon-default mud-icon-size-medium {GetFileIcon(item.Extension)}"" aria-hidden=""true"" role=""img""></span></div>
<p class=""mud-typography mud-typography-body1 mud-treeview-item-label"">{item.Name}</p></div></li></div>");
                builder.CloseElement();


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
    }
}