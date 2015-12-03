using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JusTGalery.Model
{
    public class NavigationItem
    {
        public enum ItemType { Image, Folder, Tag, Raiting }

        public NavigationItem(string id)
        {
            this.Id = id;
        }

        public string Id { get; private set; }

        public string Title { get; set; }

        public string FilePath { get; set; }

        public ItemType Type { get; set; }

        public List<NavigationItem> Children { get; set; } = new List<NavigationItem>();

        public bool Expanded { get; set; }
    }
}
