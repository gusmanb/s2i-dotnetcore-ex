using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Xml;
using app.Classes;
using MimeTypes;
using System.Globalization;

namespace app.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page CACACACACACACA.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Test()
        {
            return View();
        }

        [Route("Home/Channel/{channel}")]
        public IActionResult Channel(string channel)
        {
            var feed = BuildXmlFeed(channel);
            return Content(feed, "text/xml");
        }


        public string BuildXmlFeed(string channel)
        {

            var chan = DBManager.GetChannel(channel);

            if (chan == null)
                return "not found";

            var sets = DBManager.GetSettings();

            StringWriter parent = new StringWriter();
            using (XmlWriter writer = XmlWriter.Create(parent))
            {
                writer.WriteProcessingInstruction("xml-stylesheet", "title=\"XSL_formatting\" type=\"text/xsl\" href=\"/skins/default/controls/rss.xsl\"");

                writer.WriteStartElement("rss");
                writer.WriteAttributeString("version", "2.0");

                // write out 
                writer.WriteStartElement("channel");

                // write out -level elements
                writer.WriteElementString("title", chan.Name);
                writer.WriteElementString("description", chan.Description);
                writer.WriteElementString("link", chan.Link);
                writer.WriteElementString("language", "en-us");
                writer.WriteElementString("copyright", chan.Copyright);

                writer.WriteElementString("lastBuildDate", ComposeDate(chan.UpdateDate.ToUniversalTime()));
                writer.WriteElementString("pubDate", ComposeDate(chan.CreationDate.ToUniversalTime()));
                
                if (chan.Podcasts != null)
                {
                    foreach (var podcast in chan.Podcasts)
                    {
                        writer.WriteStartElement("item");

                        writer.WriteElementString("title", podcast.Title);
                        writer.WriteElementString("link", podcast.Link);
                        writer.WriteElementString("description", podcast.Description);

                        writer.WriteElementString("guid", sets.BasePath +"/podcasts/" + podcast.File);

                        var mime = MimeTypeMap.GetMimeType(Path.GetExtension(podcast.File));

                        writer.WriteStartElement("enclosure");

                        writer.WriteAttributeString("url", sets.BasePath + "/podcasts/" + podcast.File);
                        writer.WriteAttributeString("type", mime);
                        writer.WriteAttributeString("length", new FileInfo("wwwroot/podcasts/" + podcast.File).Length.ToString());
                        
                        writer.WriteEndElement();

                        writer.WriteElementString("category", "Podcasts");
                        writer.WriteElementString("pubDate", ComposeDate(podcast.Date.ToUniversalTime()));
                    }
                }

                // write out 
                writer.WriteEndElement();

                // write out 
                writer.WriteEndElement();
            }

            return parent.ToString();
        }

        static string ComposeDate(DateTime Date)
        {
            string day = Date.DayOfWeek.ToString().Substring(0, 3);
            string month = Date.ToString("MMMM", CultureInfo.InvariantCulture).Substring(0, 3);

            return $"{day}, {Date.Day} {month} {Date.Year} {Date.Hour.ToString().PadLeft(2, '0')}:{Date.Minute.ToString().PadLeft(2, '0')}:{Date.Second.ToString().PadLeft(2, '0')} GMT";
        }
    }
}

