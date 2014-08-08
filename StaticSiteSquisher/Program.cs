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
			Console.WriteLine ("Minifying {0}", folder);
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

					Console.WriteLine ("\tMinifying {0}", file);
                    File.WriteAllText(minSheetPath, miniSheet);
                }

                if (Path.GetExtension(file) == ".js")
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
			// This is removing the ending } on my JS, so don't run this regex.
            // html = Regex.Replace(html, @"(?s)\s+(?!(?:(?!</?pre\b).)*</pre>)", " ");
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

            html = html.Replace(".css", ".min.css");
            html = html.Replace(".js", ".min.js");

            // fix for Google Analytics
            html = html.Replace("//www.google-analytics.com/analytics.min.js", "//www.google-analytics.com/analytics.js");

			// fix for Disqus
			html = html.Replace("dsq.src = '//' + disqus_shortname + '.disqus.com/embed.min.js'", "dsq.src = '//' + disqus_shortname + '.disqus.com/embed.js'");
            
            return html;

        }
    }
}
