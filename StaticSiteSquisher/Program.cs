using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
namespace StaticSiteSquisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string folder = args[0];
			Console.WriteLine ("Minifying {0}", folder);
            MinifyFolder(folder);
        }

        private static void MinifyFolder(string path)
        {
            Microsoft.Ajax.Utilities.Minifier mini = new Microsoft.Ajax.Utilities.Minifier();
            foreach (var file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) == ".css" && !(Path.GetFileName(file).Contains(".min.css")))
                {
                    var contents = File.ReadAllText(file);
                    var miniSheet = mini.MinifyStyleSheet(contents);


                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var minSheetPath = Path.Combine(path, string.Format("{0}.min.css", fileName));

					Console.WriteLine ("\tMinifying {0}", file);
                    File.WriteAllText(minSheetPath, miniSheet);
                }

                if (Path.GetExtension(file) == ".js" && !(Path.GetFileName(file).Contains(".min.js")))
                {
                    var contents = File.ReadAllText(file);
                    var miniSheet = mini.MinifyJavaScript(contents);


                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var minSheetPath = Path.Combine(path, string.Format("{0}.min.js", fileName));

					Console.WriteLine ("\tMinifying {0}", file);
                    File.WriteAllText(minSheetPath, miniSheet);
                }

               if (Path.GetExtension(file) == ".html" || Path.GetExtension(file) == ".htm")
                {
                    var contents = File.ReadAllText(file);
                    var miniSheet = MinifyHtml(contents);

					Console.WriteLine ("\tMinifying {0}", file);
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
            html = Regex.Replace(html, @"(?s)\s+(?!(?:(?!</?pre\b).)*</pre>)", " ");
            html = Regex.Replace(html, @"(?s)\s*\n\s*(?!(?:(?!</?pre\b).)*</pre>)", "\n");
            html = Regex.Replace(html, @"(?s)\s*\>\s*\<\s*(?!(?:(?!</?pre\b).)*</pre>)", "><");
            html = Regex.Replace(html, @"(?s)<!--((?:(?!</?pre\b).)*?)-->(?!(?:(?!</?pre\b).)*</pre>)", "");

            // single-line doctype must be preserved 
            var firstEndBracketPosition = html.IndexOf(">");
            if (firstEndBracketPosition >= 0)
            {
                html = html.Remove(firstEndBracketPosition, 1);
                html = html.Insert(firstEndBracketPosition, ">");
            }


            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var scripts = doc.DocumentNode.SelectNodes("//script");
            if (scripts != null)
            {
                foreach (var script in scripts)
                {
                    HtmlAttribute attr = script.Attributes["src"];
                    if (attr != null)
                        attr.Value = attr.Value.Replace(".js", ".min.js");
                    else
                    {
                        Microsoft.Ajax.Utilities.Minifier mini = new Microsoft.Ajax.Utilities.Minifier();
                        var minified = mini.MinifyJavaScript(script.InnerText);
                        script.InnerHtml = minified;
                    }
                }
            }

            var linkedStylesheets = doc.DocumentNode.SelectNodes("//link[@href]");
            if (linkedStylesheets != null)
            {
                foreach (var stylesheet in linkedStylesheets)
                {
                    HtmlAttribute attr = stylesheet.Attributes["href"];
                    attr.Value = attr.Value.Replace(".css", ".min.css");
                }
            }

            var embeddedStyleSheets = doc.DocumentNode.SelectNodes("//style");
            if (embeddedStyleSheets != null)
            {
                foreach (var stylesheet in embeddedStyleSheets)
                {
                    Microsoft.Ajax.Utilities.Minifier mini = new Microsoft.Ajax.Utilities.Minifier();
                    var minified = mini.MinifyStyleSheet(stylesheet.InnerText);
                    stylesheet.InnerHtml = minified;
                }
            }

            html = doc.DocumentNode.OuterHtml;
            return html;
        }
    }
}
