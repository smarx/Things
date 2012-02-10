using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Diagnostics;
using System.Text;
using System.IO;
using MarkdownSharp;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Things_Web.Controllers
{
    public class HomeController : Controller
    {
        CloudBlobContainer GetContainer()
        {
            return (RoleEnvironment.IsAvailable
                    ? CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"))
                    : CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=things;AccountKey=GAwZemN6Q3Iek6Tqw4XKWY/d3sDW8dK1XeqfWdEZhMgrXMMULROfGzWwvPtCsFvIJI9Towkpu1lJjE+tvTGLNA==")//CloudStorageAccount.DevelopmentStorageAccount
                   ).CreateCloudBlobClient().GetContainerReference("things");
        }

        public ActionResult Index()
        {
            var things = GetContainer().ListBlobs().OfType<CloudBlobDirectory>().Select(d => Uri.UnescapeDataString(d.Uri.Segments.Last().TrimEnd('/')));
            return View(things);
        }

        Tuple<string, IHtmlString> Pygmentize(CloudBlob blob)
        {
            var filename = Uri.UnescapeDataString(blob.Uri.Segments.Last());

            var extension = Path.GetExtension(filename).ToLower();

            if (extension == ".mdown")
            {
                return new Tuple<string, IHtmlString>(null, new MvcHtmlString(new Markdown().Transform(blob.DownloadText())));
            }

            var formatters = new Dictionary<string, string>()
            {
                { ".cscfg", "xml" },
                { ".csdef", "xml" },
                { ".config", "xml" },
                { ".xml", "xml" },
                { ".cmd", "bat" },
                { ".rb", "ruby" },
                { ".cs", "csharp" },
                { ".html", "html" },
                { ".cshtml", "html" },
                { ".css", "css" },
                { ".erb", "erb" },
                { ".haml", "haml" },
                { ".js", "javascript" },
                { ".php", "php" },
                { ".py", "python" },
                { ".yaml", "yaml" },
                { ".yml", "yaml" },
                { ".txt", "text" }
            };

            var executable = "pygmentize";
            if (RoleEnvironment.IsAvailable)
            {
                executable = Path.Combine(RoleEnvironment.GetLocalResource("Python").RootPath, @"scripts\pygmentize.exe");
            }

            if (!formatters.ContainsKey(extension))
            {
                extension = ".txt";
            }

            var startInfo = new ProcessStartInfo(executable, string.Format("-f html -l {0}", formatters[extension]))
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                };
            var proc = Process.Start(startInfo);
            proc.StandardInput.Write(blob.DownloadText().Trim(new char[]{'\uFEFF'}));
            proc.StandardInput.Close();
            return new Tuple<string, IHtmlString>(filename, new MvcHtmlString(proc.StandardOutput.ReadToEnd()));
        }

        public ActionResult Item(string id)
        {
            ViewBag.Title = Uri.UnescapeDataString(id);
            var files = GetContainer().GetDirectoryReference(id).ListBlobs(new BlobRequestOptions { UseFlatBlobListing = false }).OfType<CloudBlob>()
                .Where(b => !b.Uri.AbsoluteUri.EndsWith("$$$.$$$")).Select(Pygmentize);
            return View(files);
        }
    }
}