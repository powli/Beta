using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Beta
{
    [Serializable]
    public class MessageRepository
    {
        public string ChannelName;
        public ulong ChannelID;
        

        [XmlArrayItem("Message")]
        public List<Message> Messages
        {
            get;
            set;
        }

        public MessageRepository(string channel, ulong id)
        {
            Messages = new List<Message>();
            this.ChannelName = channel;
            this.ChannelID = id;            
        }

        public void AddMessage(string arthur, string message)
        {
            Message msg = new Message
            {
                Text = message,
                ChannelName = ChannelName,
                Arthur = arthur,
                DateAdded = DateTime.Now,
            };
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MessageRepository));
            using (FileStream file = File.Create(Path.Combine(Environment.CurrentDirectory, "Channels", ChannelID + "Log.xml")))
            {
                serializer.Serialize(file, this);
            }
        }

        public MessageRepository LoadFromDisk()
        {
            MessageRepository dictionary;
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Channels", ChannelID + "Log.xml")))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MessageRepository));
                using (FileStream file = new FileStream(Path.Combine(Environment.CurrentDirectory, "Channels", ChannelID + "Log.xml"), FileMode.Open))
                {
                    dictionary = serializer.Deserialize(file) as MessageRepository;
                }
            }
            else
            {
                dictionary = new MessageRepository(ChannelName, ChannelID);
                dictionary.Save();
            }
            return dictionary;
        }
    }

    public class Message
    {
        [XmlAttribute]
        public string Text { get; set; }

        [XmlAttribute]
        public string ChannelName { get; set; }

        [XmlAttribute]
        public string Arthur { get; set; }

        [XmlAttribute]
        public DateTime DateAdded { get; set; }

    }
}