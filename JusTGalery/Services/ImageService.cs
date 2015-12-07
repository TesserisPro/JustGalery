using JusTGalery.Model;
using LightStack.LightDesk;
using LightStack.LightDesk.Services;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JusTGalery.Services
{
    public class ImageService
    {
        private ExifService exifService;

        private NavigationService navigationService;

        List<ImageInfo> selectedImages;

        public ImageService(ExifService exifService, NavigationService navigationService)
        {
            this.exifService = exifService;
            this.navigationService = navigationService;
        }
                
        public void GetSelectedImages(InteropService.Callback progress, InteropService.Callback addImage)
        {
            progress(new object[] { 1 });
            var result = new List<NavigationItem>();

            var selectedItem = navigationService.GetSelectedItem();
            if (selectedItem.Type == NavigationItem.ItemType.Image)
            {
                result.Add(selectedItem);
            }
            else
            {
                var folders = new List<NavigationItem>(new[] { selectedItem });
                while (folders.Any())
                {
                    foreach (var folder in folders.ToArray())
                    {
                        result.AddRange(folder.Children.Where(x => x.Type == NavigationItem.ItemType.Image));
                        folders.Remove(folder);
                        folders.AddRange(folder.Children.Where(x => x.Children.Any()));
                    }
                }
            }
            selectedImages = new List<ImageInfo>();
            Utils.ForeachAsyncWithProgress(result, x =>
                {
                    var fileInfo = new FileInfo(x.FilePath);
                    var image = new ImageInfo
                    {
                        Id = x.Id,
                        ImagePath = x.FilePath,
                        ThumbnailPath = EnsureThumbnailExist(x.FilePath),
                        Properties = exifService.GetProperties(x.FilePath),
                        Tags = exifService.GetTags(x.FilePath),
                        Title = exifService.GetTitle(x.FilePath),
                        Author = exifService.GetAuthor(x.FilePath),
                        Raiting = exifService.GetRating(x.FilePath)
                    };
                    selectedImages.Add(image);
                    addImage(new object[] { image });
                },
                progress);
        }

        public void SetRaiting(int raiting, InteropService.Callback progress)
        {
            Utils.ForeachAsyncWithProgress(
                this.selectedImages,
                image =>
                {
                    image.Raiting = raiting;
                    exifService.SetRaiting(image.ImagePath, raiting);
                },
                progress);
        }

        public void SetAuthor(string author, InteropService.Callback progress)
        {
            Utils.ForeachAsyncWithProgress(
                 this.selectedImages,
                 image =>
                 {
                     image.Author = author;
                     exifService.SetAuthor(image.ImagePath, author);
                 },
                 progress);
        }

        public void SetTitle(string title, InteropService.Callback progress)
        {
            Utils.ForeachAsyncWithProgress(
                 this.selectedImages,
                 image =>
                 {
                     image.Title = title;
                     exifService.SetTitle(image.ImagePath, title);
                 }, 
                 progress);
        }

        public void AddTag(string tag, InteropService.Callback progress)
        {
            Utils.ForeachAsyncWithProgress(
                 this.selectedImages,
                 image =>
                 {
                     var tags = image.Tags.ToList();
                     tags.Add(tag);
                     exifService.SetTags(image.ImagePath, string.Join(";", tags));
                     image.Tags = tags;
                 },
                 progress);
        }

        public void RemoveTag(string tag, InteropService.Callback progress)
        {
            Utils.ForeachAsyncWithProgress(
                 this.selectedImages,
                 image =>
                 {
                     var tags = image.Tags.ToList();
                     tags.Remove(tag);
                     exifService.SetTags(image.ImagePath, string.Join(";", tags));
                     image.Tags = tags;
                 },
                 progress);
        }

        public void DeleteFiles(InteropService.Callback progress)
        {
            Utils.ForeachAsyncWithProgress(
                  this.selectedImages.ToArray(),
                  image =>
                  {
                      File.Delete(image.ThumbnailPath);
                      FileSystem.DeleteFile(image.ImagePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                  },
                  progress);

            navigationService.SetSelectedItem(null);
            this.selectedImages = null;
        }

        public string EnsureThumbnailExist(string imageFileName)
        {
            try
            {
                var pathToFile = imageFileName;
                var fileName = Path.GetFileName(imageFileName);
                var pathToThumbDir = Path.Combine(Path.GetDirectoryName(pathToFile), ".galery");
                var pathToThumb = Path.Combine(pathToThumbDir, fileName);

                if (!File.Exists(pathToThumb))
                {
                    if (!Directory.Exists(pathToThumbDir))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(pathToThumbDir);
                        di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                    }

                    using (var stream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var image = System.Drawing.Bitmap.FromStream(stream))
                        {
                            var scale = 1.0;
                            var width = 512;
                            var height = 512;
                            if (image.Width > image.Height)
                            {
                                scale = 512.0 / ((double)image.Width);
                                height = (int)(((double)image.Height) * scale);
                            }
                            else
                            {
                                scale = 512.0 / ((double)image.Height);
                                width = (int)(((double)image.Width) * scale);
                            }

                            using (var thumbImage = new Bitmap(width, height))
                            {
                                using (var g = Graphics.FromImage(thumbImage))
                                {
                                    g.DrawImage(image, 0, 0, width, height);
                                }
                                thumbImage.Save(pathToThumb, System.Drawing.Imaging.ImageFormat.Jpeg);
                            }
                        }
                    }
                }

                return pathToThumb;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }
    }
}
