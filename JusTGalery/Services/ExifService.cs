using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JusTGalery.Services
{
    public class ExifService
    {
        public int GetRating(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var parameters = new Dictionary<string, string>();

                var reader = ImageMetadataReader.ReadMetadata(stream);
                var exif = reader.OfType<ExifIfd0Directory>().FirstOrDefault();
                if (exif != null)
                {
                    return int.Parse(exif.GetDescription(ExifDirectoryBase.TagRating) ?? "0");
                }

                return 0;
            }
        }

        public IEnumerable<string> GetTags(string fileName)
        {
            try {
                using (var stream = File.OpenRead(fileName))
                {
                    var parameters = new Dictionary<string, string>();

                    var reader = ImageMetadataReader.ReadMetadata(stream);
                    var exif = reader.OfType<ExifIfd0Directory>().FirstOrDefault();
                    if (exif != null)
                    {
                        return exif.GetDescription(ExifDirectoryBase.TagWinKeywords)
                            ?.Split(';')
                            ?.Select(x => x.Trim())
                            ?.Where(x => !string.IsNullOrEmpty(x))
                            ?.ToArray()
                            ?? new string[0];
                    }

                    return new string[0];
                }
            }
            catch
            {
                return new string[] { "ERROR!" };
            }
        }

        public DateTime GetDate(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var parameters = new Dictionary<string, string>();

                var reader = ImageMetadataReader.ReadMetadata(stream);
                var exifSub = reader.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (exifSub != null)
                {
                    return DateTime.Parse(exifSub.GetDescription(ExifDirectoryBase.TagDateTimeOriginal));
                }

                return DateTime.MinValue;
            }
        }

        public string GetAuthor(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var parameters = new Dictionary<string, string>();

                var reader = ImageMetadataReader.ReadMetadata(stream);
                var exif = reader.OfType<ExifIfd0Directory>().FirstOrDefault();
                if (exif != null)
                {
                    return exif?.GetDescription(ExifDirectoryBase.TagWinAuthor);
                }

                return string.Empty;
            }
        }

        public string GetTitle(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var parameters = new Dictionary<string, string>();

                var reader = ImageMetadataReader.ReadMetadata(stream);
                var exif = reader.OfType<ExifIfd0Directory>().FirstOrDefault();
                if (exif != null)
                {
                    return exif?.GetDescription(ExifDirectoryBase.TagWinTitle);
                }

                return string.Empty;
            }
        }

        public Dictionary<string,string> GetProperties(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var parameters = new Dictionary<string, string>();

                var reader = ImageMetadataReader.ReadMetadata(stream);
                var exif = reader.OfType<ExifIfd0Directory>().FirstOrDefault();
                var exifSub = reader.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                
                //parameters.Add("Title", exif?.GetDescription(ExifDirectoryBase.TagWinTitle));
                //parameters.Add("Raiting", exif?.GetDescription(ExifDirectoryBase.TagRating));
                //parameters.Add("Author", exif?.GetDescription(ExifDirectoryBase.TagWinAuthor));
                //parameters.Add("Tags", exif?.GetDescription(ExifDirectoryBase.TagWinKeywords));
                parameters.Add("Камера", exif?.GetDescription(ExifDirectoryBase.TagModel));
                parameters.Add("Дата та час", exifSub?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal));
                parameters.Add("Витримка (S)", exifSub?.GetDescription(ExifDirectoryBase.TagExposureTime));
                parameters.Add("Діафрагма (A)", exifSub?.GetDescription(ExifDirectoryBase.TagMaxAperture));
                parameters.Add("Чутливість (ISO)", exifSub?.GetDescription(ExifDirectoryBase.TagIsoEquivalent));
                parameters.Add("Спалах", exifSub?.GetDescription(ExifDirectoryBase.TagFlash));
                parameters.Add("Фокусна відстань", exifSub?.GetDescription(ExifDirectoryBase.Tag35MMFilmEquivFocalLength));
                parameters.Add("Експокорекція", exifSub?.GetDescription(ExifDirectoryBase.TagExposureBias));
                parameters.Add("Режим експозиції", exifSub?.GetDescription(ExifDirectoryBase.TagExposureMode));
                
                if (exif?.GetString(ExifDirectoryBase.TagModel)?.ToLower()?.StartsWith("nikon") == true)
                {
                    var nikon = reader.OfType<NikonType2MakernoteDirectory>().FirstOrDefault();
                    if (nikon != null && !nikon.Errors.Any())
                    {
                        parameters.Add("Фокус", nikon?.GetDescription(NikonType2MakernoteDirectory.TagAfType));
                        parameters.Add("Об'ектив", nikon?.GetDescription(NikonType2MakernoteDirectory.TagLens));
                        parameters.Remove("Експокорекція");
                        parameters.Add("Експокорекція", nikon?.GetDescription(NikonType2MakernoteDirectory.TagExposureTuning));
                        parameters.Remove("Спалах");
                        parameters.Add("Спалах", nikon?.GetDescription(NikonType2MakernoteDirectory.TagFlashUsed));
                    }
                }

                parameters.Add("Розмір", Math.Round(((double)stream.Length) / 1024.0 / 1024.0, 2).ToString() + " Мб");
                parameters.Add("File Name", Path.GetFileName(fileName));
                parameters.Add("Path", fileName);

                return parameters;
            }
        }

        public void SetRaiting(string fileName, int rating)
        {
            var file = ExifLibrary.ImageFile.FromFile(fileName);
            file.Properties.Set(ExifLibrary.ExifTag.Rating, rating);
            file.Save(fileName);
        }

        public void SetTitle(string fileName, string author)
        {
            var file = ExifLibrary.ImageFile.FromFile(fileName);
            file.Properties.Set(ExifLibrary.ExifTag.WindowsTitle, author);
            file.Save(fileName);
        }

        public void SetAuthor(string fileName, string title)
        {
            var file = ExifLibrary.ImageFile.FromFile(fileName);
            file.Properties.Set(ExifLibrary.ExifTag.WindowsAuthor, title);
            file.Save(fileName);
        }

        public void SetTags(string fileName, string tags)
        {
            var file = ExifLibrary.ImageFile.FromFile(fileName);
            file.Properties.Set(ExifLibrary.ExifTag.WindowsKeywords, tags);
            file.Save(fileName);
        }
    }
}
