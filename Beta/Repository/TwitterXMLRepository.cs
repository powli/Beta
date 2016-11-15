using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Discord;

namespace Beta.Repository
{
    [Serializable]
    public class TwitterXMLRepository
    {
        private const string _Filename = "tweets.xml";

        [XmlArrayItem("Tweets")]
        public List<Tweet> Tweets
        {
            get; set;
        }

        public TwitterXMLRepository()
        {
            Tweets = new List<Tweet>();
        }

        public void AddTweet()
        {
            Tweets.Add(new Tweet()
            {
                handle = "DongMuncher",
                is_retweet = false,
                text = "This is a sick ass tweet, yo."
            });
        }

        public static TwitterXMLRepository LoadFromDisk()
        {
            TwitterXMLRepository dictionary;
            Console.WriteLine(File.Exists(_Filename));
            if (File.Exists(_Filename))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TwitterXMLRepository));
                using (FileStream file = new FileStream(_Filename, FileMode.Open))
                {
                    dictionary = serializer.Deserialize(file) as TwitterXMLRepository;                                        
                }
                

            }
            else
            {
                Console.WriteLine("Made a new TwitterXMLRepo!");
                dictionary = new TwitterXMLRepository();
                dictionary.Save();
            }
            dictionary.Save();
            return dictionary;            
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TwitterXMLRepository));
            using (FileStream file = File.Create("TweetRepo.xml"))
            {
                serializer.Serialize(file, this);
            }
        }
    }

    public class Tweet
    {
        [XmlAttribute]
        public string handle { get; set; }        

        [XmlAttribute]
        public bool is_retweet { get; set; }

        [XmlAttribute]
        public string text { get; set; }
       
    }


}