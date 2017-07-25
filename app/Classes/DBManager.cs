using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Classes
{
    public static class DBManager
    {
        static LiteDatabase db;
        
        static DBManager()
        {
            db = new LiteDatabase("wpcasts.db");

            var col1 = Admins;
            var col3 = Podcasts;

        }

        static LiteCollection<Settings> CurrentSettings { get { return db.GetCollection<Settings>("settings"); } }
        static LiteCollection<AdminUser> Admins { get { return db.GetCollection<AdminUser>("users"); } }
        static LiteCollection<ChannelData> Channels { get { return db.GetCollection<ChannelData>("channels"); } }
        static LiteCollection<Podcast> Podcasts { get { return db.GetCollection<Podcast>("podcasts"); } }

        public static Settings GetSettings()
        {
            return CurrentSettings.FindAll().FirstOrDefault();
        }

        public static void UpdateSettings(string BasePath)
        {
            var sets = CurrentSettings;

            var item = CurrentSettings.FindAll().FirstOrDefault();

            if (item != null)
            {
                item.BasePath = BasePath;
                sets.Update(item);
            }
            else
            {
                sets.Insert(new Settings { BasePath = BasePath });
            }
        }

        public static bool ValidateAdmin(string User, string Password)
        {
            var adm = Admins;
            bool exists = adm.Exists(a => a.User == User && a.Password == Password);

            if (!exists)
            {
                if (adm.Count() > 0)
                    return false;

                adm.Insert(new AdminUser { Password = Password, User = User });
            }

            return true;
        }

        #region ChannelManagement

        public static int CreateChannel(string Name, string Description, string Copyright, string Link)
        {
            var chn = Channels;

            var all = chn.FindAll();

            if (chn.Find(c => c.Name == Name).Count() > 0)
                return -1;

            var id = chn.Insert(new ChannelData { Name = Name, Description = Description, Copyright = Copyright, CreationDate = DateTime.Now, UpdateDate = DateTime.Now, Link = Link });

            return id;
        }

        public static bool UpdateChannel(int Id, string Name, string Description, string Copyright, string Link)
        {
            var chn = Channels;

            var channel = chn.FindOne(c => c.Id == Id);

            if (channel == null)
                return false;

            channel.Name = Name;
            channel.Description = Description;
            channel.Copyright = Copyright;
            channel.UpdateDate = DateTime.Now;
            channel.Link = Link;

            chn.Update(channel);

            return true;
        }

        public static bool DeleteChannel(int Id)
        {
            var chn = Channels;
            
            return chn.Delete(c => c.Id == Id) > 0;

        }

        public static ChannelData[] GetChannels()
        {
            return Channels.FindAll().ToArray();
        }

        public static ChannelData GetChannel(int Id)
        {
            var chn = Channels;

            return chn.FindOne(c => c.Id == Id);
        }

        public static ChannelData GetChannel(string Name)
        {
            var chn = Channels;

            var chan = chn.FindOne(c => c.Name == Name);

            if (chan != null)
                chan.Podcasts = Podcasts.Find(p => p.Channel.Id == chan.Id).ToList();

            return chan;
        }
        
        public static int CreatePodcast(int ChannelId, string Title, string Link, string Description, string File)
        {
            var chan = Channels.FindOne(c => c.Id == ChannelId);

            if (chan == null)
                return -1;

            var podcast = new Podcast { Channel = chan, Title = Title, Link = Link, Description = Description, File = File, Date = DateTime.Now };

            var id = Podcasts.Insert(podcast);

            if (id > 0)
            {
                chan.UpdateDate = DateTime.Now;
                Channels.Update(chan);
            }

            return id;
        }

        public static bool UpdatePodcast(int Id, int ChannelId, string Title, string Link, string Description, string File)
        {
            var cast = Podcasts.FindOne(p => p.Id == Id);

            if (cast == null)
                return false;

            var chan = Channels.FindOne(c => c.Id == ChannelId);

            if (chan == null)
                return false;

            cast.Channel = chan;
            cast.Title = Title;
            cast.Link = Link;
            cast.Description = Description;
            cast.File = File;
            cast.Date = DateTime.Now;

            Podcasts.Update(cast);

            chan.UpdateDate = DateTime.Now;
            Channels.Update(chan);

            return true;
        }

        public static bool DeletePodcast(int Id)
        {
            return Podcasts.Delete(p => p.Id == Id) > 0;
        }

        public static Podcast[] GetPodcasts(int ChannelId)
        {
            var pods = Podcasts;

            return pods.Find(p => p.Channel.Id == ChannelId).ToArray();
        }

        public static Podcast GetPodcast(int PodcastId)
        {
            var pods = Podcasts;

            return pods.FindOne(p => p.Id == PodcastId);
        }

        #endregion
    }

    #region Entities
    public class AdminUser
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class ChannelData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string Link { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<Podcast> Podcasts { get; set; }
    }

    public class Podcast
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string File { get; set; }
        public DateTime Date { get; set; }
        [BsonRef("channels")]
        public ChannelData Channel { get; set; }
    }

    public class Settings
    {
        public int Id { get; set; }
        public string BasePath { get; set; }
    }

    #endregion
}
