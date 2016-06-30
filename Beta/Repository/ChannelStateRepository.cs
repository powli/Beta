using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Beta.Repository
{
    [Serializable]
    public class ChannelStateRepository
    {
        private const string _Filename = "ChannelStates.xml";

        [XmlArrayItem("ChannelState")]
        public List<ChannelState> ChannelState
        {
            get;
            set;
        }

        public ChannelStateRepository()
        {
            ChannelState = new List<ChannelState>();
        }

        public void AddChannel()
        {

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
        public ulong ServerID {get; set;}

        [XmlAttribute]
        public string ServerName {get; set;}

        [XmlAttribute]
        public string MOTD { get; set; }

        [XmlAttribute]
        public bool MOTDSet { get; set; }

        [XmlAttribute]
        public bool GreetMode { get; set; }

        [XmlAttribute]
        public string ChannelType { get; set; }
    }
}
