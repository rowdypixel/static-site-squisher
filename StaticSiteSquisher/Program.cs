using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
namespace StaticSiteSquisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string folder = args[0];
            MinifyFolder(folder);
        }

        private static void MinifyFolder(string path)
        {
            Microsoft.Ajax.Utilities.Minifier mini = new Microsoft.Ajax.Utilities.Minifier();
            foreach (var file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) == ".css")
                {
                    var contents = File.ReadAllText(file);
                    var miniSheet = mini.MinifyStyleSheet(contents);


                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var minSheetPath = Path.Combine(path, string.Format("{0}.min.css", fileName));

                    File.WriteAllText(minSheetPath, miniSheet);
                }

                if (Path.GetExtension(file) == ".js")
                {
                    var contents = File.ReadAllText(file);
                    var miniSheet = mini.MinifyJavaScript(contents);


                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var minSheetPath = Path.Combine(path, string.Format("{0}.min.js", fileName));

                    File.WriteAllText(minSheetPath, miniSheet);
                }

                if (Path.GetExtension(file) == ".html" || Path.GetExtension(file) == ".htm")
                {
                    var contents = File.ReadAllText(file);
                    var miniSheet = MinifyHtml(contents);


                    var fileName = Path.GetFileNameWithoutExtension(file);

                    File.WriteAllText(file, miniSheet); // we want to replace html files.
                }

            }

            foreach(var folder in Directory.GetDirectories(path))
            {
                MinifyFolder(folder);
            }

            
            
        }

        private static string MinifyHtml(string html)
        {
            html = Regex.Replace(html, @"\s+", " ");
            html = Regex.Replace(html, @"\s*\n\s*", "\n");
            html = Regex.Replace(html, @"\s*\>\s*\<\s*", "><");
            html = Regex.Replace(html, @"<!--(.*?)-->", "");   //Remove comments

            // single-line doctype must be preserved 
            var firstEndBracketPosition = html.IndexOf(">");
            if (firstEndBracketPosition >= 0)
            {
                html = html.Remove(firstEndBracketPosition, 1);
                html = html.Insert(firstEndBracketPosition, ">");
            }

            html = html.Replace(".css", ".min.css");
            html = html.Replace(".js", ".min.js");

            return html;

        }
    }
}
