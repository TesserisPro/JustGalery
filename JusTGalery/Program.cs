using JusTGalery.Services;
using LightStack.LightDesk;
using LightStack.LightDesk.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JusTGalery
{
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var engine = new Engine(new LightApplication("Jus-T-Galery")
            {
                Icon = Utils.GetResourceIcon("galery.ico")
            });
            //{
            //    ResourceProvider = new ApplicationResourcesResourceProvider()
            //});
            engine.Application.RegisterService<ExifService>();
            engine.Application.RegisterService<NavigationService>();
            engine.Application.RegisterService<ImageService>();
            engine.Application.ResolveService<ResourceService>().RegisterProvider(
                "images", 
                x => new MemoryStream(File.ReadAllBytes(Uri.UnescapeDataString(x))));
            engine.Run();
        }
    }
}
