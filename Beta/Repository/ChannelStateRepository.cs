using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Discord;

namespace Beta.Repository
{
    [Serializable]
    public class ChannelStateRepository
    {
        private const string _Filename = "ChannelStates.xml";

        [XmlArrayItem("ChannelState")]
        public List<ChannelState> ChannelStates
        {
            get;
            set;
        }

        public ChannelStateRepository()
        {
            ChannelStates = new List<ChannelState>();
        }

        public void AddChannel(Channel chnl, Server srvr)
        {
            if (!VerifyChannelExists(chnl.Id))
            {
                ChannelStates.Add(new ChannelState()
                {
                    ChannelID = chnl.Id,
                    ChannelName = chnl.Name,
                    ChannelType = chnl.Type.Value,
                    ServerID = srvr.Id,
                    ServerName = srvr.Name,
                });
                Save();
            }
        }

        public bool VerifyChannelExists(ulong id)
        {
            if (ChannelStates.FirstOrDefault(cs => cs.ChannelID == id) != null) return true;
            else return false;
        }

        public ChannelState GetChannelState(ulong id)
        {
            return ChannelStates.FirstOrDefault(cs => cs.ChannelID == id);           
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ChannelStateRepository));
            using ( FileStream file = File.Create(_Filename) )
            {
                serializer.Serialize(file, this);
            }
        }

        public static ChannelStateRepository LoadFromDisk()
        {
            ChannelStateRepository dictionary;
            if ( File.Exists(_Filename) )
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ChannelStateRepository));
                using (FileStream file = new FileStream(_Filename, FileMode.Open))
                {
                    dictionary = serializer.Deserialize(file) as ChannelStateRepository;
                }
            }
            else
            {
                dictionary = new ChannelStateRepository();
                dictionary.Save();
            }
            return dictionary;
        }
    }

    public class ChannelState
    {
        [XmlAttribute]
        public ulong ChannelID {get; set;}

        [XmlAttribute]
        public string ChannelName {get; set;}

        [XmlAttribute]
        public string ChannelType { get; set; }

        [XmlAttribute]
        public ulong ServerID {get; set;}

        [XmlAttribute]
        public string ServerName {get; set;}

        [XmlAttribute]
        public string MOTD { get; set; }

        [XmlAttribute]
        public bool MOTDSet { get; set; } = false;

        [XmlAttribute]
        public bool GreetMode { get; set; } = false;

        /*public List<string> Greetings { get; set; } = new List<string>()
        {
            "Sup, homie?",
            "Hey how's it going, {0}",
            "Ugh, not this asshole again... Oh, hi {0}! I didn't see you there...",
            "here come dat boi!!!!!! o shit waddup!",
            "Greetings, {0}",
            "WHADDUP {0}?!",
        };*/


    }
}
