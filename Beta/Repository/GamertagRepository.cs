using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;


namespace Beta.Repository
{
    [Serializable]
    public class GamertagRepository
    {
        private const string _Filename = "Gamertags.xml";

        [XmlArrayItem("Gamertag")]
        public List<Gamertag> Gamertags { get; set; }

        public GamertagRepository()
        {
            Gamertags = new List<Gamertag>();
        }

        public void NewGamertag(string tag, string type, string username, ulong userid)
        {
            Gamertag.SetGamertagID(this.GetLastGamertagID());
            Gamertag.GamertagID++;
            Gamertags.Add(new Gamertag()
            {
                GTag = tag,
                GamertagType = type,
                DiscordUsername = username,
                DiscordID = userid,
                ID = Gamertag.GamertagID,
            });
            this.Save();
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GamertagRepository));
            using (FileStream file = File.Create(_Filename))
            {
                serializer.Serialize(file, this);
            }
        }

        public static GamertagRepository LoadFromDisk()
        {
            GamertagRepository dictionary;

            if (File.Exists(_Filename))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GamertagRepository));
                using (FileStream file = new FileStream(_Filename, FileMode.Open))
                {
                    dictionary = serializer.Deserialize(file) as GamertagRepository;
                }
            }
            else
            {
                dictionary = new GamertagRepository();
                dictionary.Save();
            }

            return dictionary;

        }

        public int GetLastGamertagID()
        {
            int result = 1;
            foreach (Gamertag gtag in Gamertags)
            {
                if (gtag.ID > result) result = gtag.ID;
            }

            return result;
        }
    }

    public class Gamertag
    {
        public string GTag { get; set; }
        public string GamertagType { get; set; }
        public string DiscordUsername { get; set; }
        public ulong DiscordID { get; set; }
        public int ID { get; set; }
        public static int GamertagID = 0;

        public static int GetGamertagID()
        {
            return GamertagID;
        }

        public static void SetGamertagID(int num)
        {
            GamertagID = num;
        }
    }

}