using JusTGalery.Model;
using JusTGalery.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JusTGalery.Services
{
    public class NavigationService
    {
        private string path;

        private INavigationTreeProvider activeTreeProvider;

        private NavigationItem tree;

        private NavigationItem selectedItem;

        private ExifService exifService;

        private readonly INavigationTreeProvider[] treeProviders;

        public NavigationService(ExifService exifService)
        {
            this.exifService = exifService;
            this.treeProviders = new INavigationTreeProvider[]
            {
                new FoldersTreeProvider(),
                new TagsTreeProvider(this.exifService),
                new RaitingTreeProvider(this.exifService)
            };

            this.path = string.IsNullOrEmpty(Settings.Default.Path) ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) : Settings.Default.Path;

            if (!string.IsNullOrEmpty(Settings.Default.TreeProvider) && treeProviders.Any(x => x.Name == Settings.Default.TreeProvider))
            {
                SetTreeProvider(Settings.Default.TreeProvider);
            }
            else
            {
                SetTreeProvider(this.treeProviders.First().Name);
            }
        }

        public void SetPath(string path)
        {
            this.selectedItem = null;
            this.path = path;
            RefreshTree();
            Settings.Default.Path = path;
            Settings.Default.Save();
        }

        public NavigationItem GetSelectedItem()
        {
            return selectedItem ?? tree;
        }

        public NavigationItem Forward()
        {
            var item = GetSelectedItem();
            if(item.Type == NavigationItem.ItemType.Image)
            {
                var parent = GetParent(tree, item.Id);
                if (parent != null)
                {
                    var nextIndex = parent.Children.IndexOf(item) + 1;
                    if (nextIndex < parent.Children.Count)
                    {
                        selectedItem = parent.Children[nextIndex];
                    }
                }
            }

            return selectedItem;
        }

        public NavigationItem Back()
        {
            var item = GetSelectedItem();
            if (item.Type == NavigationItem.ItemType.Image)
            {
                var parent = GetParent(tree, item.Id);
                if (parent != null)
                {
                    var nextIndex = parent.Children.IndexOf(item) - 1;
                    if (nextIndex >= 0)
                    {
                        selectedItem = parent.Children[nextIndex];
                    }
                }
            }

            return selectedItem;
        }

        private NavigationItem GetParent(NavigationItem root, string id)
        {
            if (root.Id == id)
            {
                return root;
            }
            foreach (var child in root.Children)
            {
                if(child.Id == id)
                {
                    return root;
                }
            }

            foreach (var child in root.Children)
            {
                var item = GetParent(child, id);
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }

        public NavigationItem SetSelectedItem(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                this.selectedItem = null;
            }
            else
            {
                this.selectedItem = GetItemById(this.tree, id);
            }
            return this.selectedItem;
        }

        public NavigationItem GetNavigationTree()
        {
            return this.tree;
        }

        public IEnumerable<string> GetTreeProviders()
        {
            return this.treeProviders.Select(x => x.Name);
        }

        public void RefreshTree()
        {
            var expandedeItems = new List<string>();
            
            this.tree = this.activeTreeProvider.GetNavigationTree(path);

            if (this.selectedItem != null)
            {
                this.selectedItem = GetItemById(this.tree, this.selectedItem.Id);
            }
        }

        public void SetTreeProvider(string provider)
        {
            var p = this.treeProviders.FirstOrDefault(x => x.Name == provider);
            if (p != null)
            {
                this.activeTreeProvider = p;
                RefreshTree();
            }
            Settings.Default.TreeProvider = provider;
            Settings.Default.Save();
        }

        public string GetActiveTreeProvider()
        {
            return this.activeTreeProvider.Name;
        }

        private NavigationItem GetItemById(NavigationItem root, string id)
        {
            if (root.Id == id)
            {
                return root;
            }
            foreach (var child in root.Children)
            {
                var item = GetItemById(child, id);
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }
    }

    interface INavigationTreeProvider
    {
        string Name { get; }

        NavigationItem GetNavigationTree(string path);
    }

    abstract class TreeProvider
    {
        protected List<string> GetFlatFilesList(string path)
        {
            var result = new List<string>();
            result.AddRange(GetFiles(path));

            var folders = GetDirectories(path).ToList();
            while (folders.Any())
            {
                foreach (var folder in folders.ToArray())
                {
                    result.AddRange(GetFiles(folder));
                    folders.Remove(folder);
                    folders.AddRange(GetDirectories(folder));
                }
            }

            return result.Where(x => IsImage(x)).ToList();
        }

        protected bool IsImage(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            return extension == ".jpeg" || extension == ".jpg";
        }

        protected IEnumerable<string> GetDirectories(string path)
        {
            return Directory.GetDirectories(path).Where(x => !File.GetAttributes(x).HasFlag(FileAttributes.Hidden));
        }

        protected IEnumerable<string> GetFiles(string path)
        {
            return Directory.GetFiles(path).Where(x => !File.GetAttributes(x).HasFlag(FileAttributes.Hidden) && IsImage(x));
        }
    }

    class FoldersTreeProvider : TreeProvider, INavigationTreeProvider
    {
        public string Name
        {
            get { return "Folders"; }
        }

        public NavigationItem GetNavigationTree(string path)
        {
            var root = new NavigationItem("f4b468ed-4c11-425a-8e57-90e82eee4202")
            {
                Title = Name,
                Type = NavigationItem.ItemType.Folder,
                FilePath = path,
                Expanded = true
            };

            var directories = GetDirectories(path);
            foreach (var dir in directories)
            {
                root.Children.Add(GetNavigationDirectoryItem(dir));
            }

            var files = GetFiles(path);
            foreach (var file in files)
            {
                root.Children.Add(GetNavigationFileItem(file));
            }

            return root;
        }

        private NavigationItem GetNavigationDirectoryItem(string path)
        {
            var item = new NavigationItem(path)
            {
                Title = Path.GetFileName(path),
                Type = NavigationItem.ItemType.Folder,
                FilePath = path
            };

            var directories = GetDirectories(path);
            foreach (var dir in directories)
            {
                item.Children.Add(GetNavigationDirectoryItem(dir));
            }

            var files = GetFiles(path);
            foreach (var file in files)
            {
                item.Children.Add(GetNavigationFileItem(file));
            }

            return item;
        }

        private NavigationItem GetNavigationFileItem(string file)
        {
            return new NavigationItem(file)
            {
                Title = Path.GetFileName(file),
                Type = NavigationItem.ItemType.Image,
                FilePath = file
            };
        }
    }

    class TagsTreeProvider : TreeProvider, INavigationTreeProvider
    {
        private ExifService exifService;

        public string Name
        {
            get { return "Tags"; }
        }

        public TagsTreeProvider(ExifService exifService)
        {
            this.exifService = exifService;
        }

        public NavigationItem GetNavigationTree(string path)
        {
            var root = new NavigationItem("Tags-2f4adf11-910f-42bf-8b10-080a055e3c89")
            {
                Title = Name,
                Type = NavigationItem.ItemType.Folder,
                FilePath = path,
                Expanded = true
            };

            var files = GetFlatFilesList(path)
                .Select(x =>
                {
                    var tuple = new Tuple<string, List<string>>(x, this.exifService.GetTags(x).ToList());
                    if (!tuple.Item2.Any())
                    {
                        tuple.Item2.Add("[NoTag]");
                    }
                    return tuple;
                })
                .ToList();
            var tags = files.SelectMany(x => x.Item2).Distinct().ToList();

            foreach (var tag in tags)
            {
                var item = new NavigationItem("tag:/" + tag)
                {
                    Type = NavigationItem.ItemType.Tag,
                    Title = tag,
                    Children = files.Where(x => x.Item2.Contains(tag))
                                    .Select(x => new NavigationItem(x.Item1)
                                    {
                                        Type = NavigationItem.ItemType.Image,
                                        FilePath = x.Item1,
                                        Title = Path.GetFileName(x.Item1)
                                    })
                                    .ToList()
                };
                root.Children.Add(item);
            }


            return root;
        }
    }

    class RaitingTreeProvider : TreeProvider, INavigationTreeProvider
    {
        private ExifService exifService;

        public string Name
        {
            get { return "Raiting"; }
        }

        public RaitingTreeProvider(ExifService exifService)
        {
            this.exifService = exifService;
        }

        public NavigationItem GetNavigationTree(string path)
        {
            var root = new NavigationItem("Rating-64c16fb3-d3fd-4305-8688-714fa0bfe949")
            {
                Title = Name,
                Type = NavigationItem.ItemType.Folder,
                FilePath = path,
                Expanded = true
            };

            var files = GetFlatFilesList(path).Select(x => new Tuple<string, int>(x, this.exifService.GetRating(x))).ToList();
            var raitings = new int[] { 0, 1, 2, 3, 4, 5 };

            foreach (var raiting in raitings)
            {
                var item = new NavigationItem("rating:/" + raiting)
                {
                    Type = NavigationItem.ItemType.Raiting,
                    Title = raiting.ToString(),
                    Children = files.Where(x => x.Item2 == raiting)
                                    .Select(x => new NavigationItem(x.Item1)
                                    {
                                        Type = NavigationItem.ItemType.Image,
                                        FilePath = x.Item1,
                                        Title = Path.GetFileName(x.Item1)
                                    })
                                    .ToList()
                };
                root.Children.Add(item);
            }

            return root;
        }
    }
}
