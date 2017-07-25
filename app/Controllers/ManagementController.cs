using app.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Controllers
{
    public class ManagementController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [AcceptVerbs("POST")]
        public IActionResult Login(IFormCollection Data)
        {
            string user = Data["username"];
            string password = Data["password"];

            if(string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["error"] = "Missing login info";
                return View();

            }

            var logged = DBManager.ValidateAdmin(user, password);

            if (!logged)
            {
                ViewData["error"] = "Wrong user/password";
                return View();
            }

            HttpContext.Session.SetInt32("logged", 1);

            return Redirect("Channels");
        }

        public IActionResult Channels()
        {
            var log = HttpContext.Session.GetInt32("logged");

            if (log == null || log != 1)
                return Redirect("Login");

            return View();
        }
        
        [AcceptVerbs("GET")]
        [Route("Management/EditChannel/{ChannelNumber}")]
        public IActionResult EditChannel(int ChannelNumber)
        {
            var log = HttpContext.Session.GetInt32("logged");

            if (log == null || log != 1)
                return Redirect("../Login");

            ChannelData chan;

            if (ChannelNumber == -1)
                chan = new ChannelData { Id = -1 };
            else
                chan = DBManager.GetChannel(ChannelNumber);

            if(chan == null)
                return Redirect("../Channels");

            ViewData["channel"] = chan;

            return View();
        }

        [AcceptVerbs("POST")]
        [Route("Management/EditChannel/{ChannelNumber}")]
        public IActionResult EditChannel(int ChannelNumber, IFormCollection Data)
        {
            var log = HttpContext.Session.GetInt32("logged");

            if (log == null || log != 1)
                return Redirect("../Login");

            string name = Data["channelName"];
            string description = Data["channelDescription"];
            string link = Data["channelLink"];
            string copy = Data["channelCopyright"];

            if (ChannelNumber == -1)
            {
                var res = DBManager.CreateChannel(name, description, copy, link);

                if (res < 1)
                {
                    ViewData["error"] = "Error creating channel";
                    return View();

                }

                return Redirect("../Channels");
            }
            else
            {
                var up = DBManager.UpdateChannel(ChannelNumber, name, description, copy, link);

                if (!up)
                {
                    ViewData["error"] = "Error updating channel";
                    return View();
                }

                return Redirect("../Channels");
            }
        }
        
        [Route("Management/DeleteChannel/{ChannelNumber}")]
        public IActionResult DeleteChannel(int ChannelNumber)
        {
            var log = HttpContext.Session.GetInt32("logged");

            if (log == null || log != 1)
                return Redirect("../Login");

            DBManager.DeleteChannel(ChannelNumber);

            return Redirect("../Channels");
        }



        [Route("Management/Podcasts/{ChannelNumber}")]
        public IActionResult Podcasts(int ChannelNumber)
        {
            var log = HttpContext.Session.GetInt32("logged");

            if (log == null || log != 1)
                return Redirect("../Login");

            ViewData["channel"] = ChannelNumber;

            return View();
        }

        [AcceptVerbs("GET")]
        [Route("Management/EditPodcast/{ChannelNumber}/{PodcastNumber}")]
        public IActionResult EditPodcast(int ChannelNumber, int PodcastNumber)
        {
            var log = HttpContext.Session.GetInt32("logged");

            if (log == null || log != 1)
                return Redirect("../../Login");

            Podcast cast;

            if (PodcastNumber == -1)
            {
                cast = new Podcast { Id = -1, Channel = new ChannelData { Id = ChannelNumber } };
            }
            else
                cast = DBManager.GetPodcast(PodcastNumber);

            if (cast == null)
                return Redirect("../../Podcasts/" + ChannelNumber);

            ViewData["podcast"] = cast;

            return View();
        }

        [AcceptVerbs("POST")]
        [Route("Management/EditPodcast/{ChannelNumber}/{PodcastNumber}")]
        public IActionResult EditPodcast(int ChannelNumber, int PodcastNumber, IFormCollection Data)
        {
            var log = HttpContext.Session.GetInt32("logged");

            if (log == null || log != 1)
                return Redirect("../../Login");
            
            string title = Data["podcastTitle"];
            string description = Data["podcastDescription"];
            string link = Data["podcastLink"];
            
            if (PodcastNumber == -1)
            {
                if (Data.Files.Count < 1)
                    return View();

                var df = Data.Files[0];

                var str = System.IO.File.Create("wwwroot/podcasts/" + df.FileName);
                df.CopyTo(str);
                str.Dispose();

                var res = DBManager.CreatePodcast(ChannelNumber, title, link, description,  df.FileName);

                if (res < 1)
                {
                    ViewData["error"] = "Error creating podcast";
                    return View();

                }

                return Redirect("../../Podcasts/" + ChannelNumber);
            }
            else
            {
                var cast = DBManager.GetPodcast(PodcastNumber);

                if(cast == null)
                    return Redirect("../../Podcasts/" + ChannelNumber);

                string file = cast.File;

                if (Data.Files.Count > 0)
                {
                    System.IO.File.Delete("wwwroot/podcasts/" + cast.File);

                    var df = Data.Files[0];
                    
                    var str = System.IO.File.Create("wwwroot/podcasts/" + df.FileName);
                    df.CopyTo(str);
                    str.Dispose();

                    file = df.FileName;
                }

                var up = DBManager.UpdatePodcast(cast.Id, ChannelNumber, title, link, description, file);

                if (!up)
                {
                    ViewData["error"] = "Error updating podcast";
                    return View();
                }

                return Redirect("../../Podcasts/" + ChannelNumber);
            }
        }

        [Route("Management/DeletePodcast/{ChannelNumber}/{PodcastNumber}")]
        public IActionResult DeletePodcast(int ChannelNumber, int PodcastNumber)
        {
            var log = HttpContext.Session.GetInt32("logged");

            if (log == null || log != 1)
                return Redirect("../Login");
            
            DBManager.DeletePodcast(PodcastNumber);


            return Redirect("../../Podcasts/" + ChannelNumber);
        }
    }
}
