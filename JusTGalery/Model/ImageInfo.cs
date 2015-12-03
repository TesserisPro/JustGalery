using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;

namespace JusTGalery.Model
{
    public class ImageInfo
    {
        public string Id { get; set; }

        public string ImagePath { get; set; }

        public string ThumbnailPath { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public int Raiting { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
