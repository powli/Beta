using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Beta
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
            Messages = new List<Message>();
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

        public MessageRepository LoadFromDisk()
        {
            MessageRepository dictionary;
            if ( File.Exists(_Filename) )
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MessageRepository));
                using (FileStream file = new FileStream(_Filename, FileMode.Open))
                {
                    dictionary = serializer.Deserialize(file) as MessageRepository;
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
    }
}
