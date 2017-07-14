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
            if (srvr != null && chnl != null)
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
        }

        public bool VerifyChannelExists(ulong id)
        {
            if (GetChannelState(id) != null) return true;
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
        public bool MOTDEnabled { get; set; } = false;

        [XmlAttribute]
        public bool RollEnabled { get; set; } = false;

        [XmlAttribute]
        public bool AskEnabled { get; set; } = false;

        [XmlAttribute]
        public bool TableUnflipEnabled { get; set; } = false;

        [XmlAttribute]
        public bool QuoteModuleEnabled { get; set; } = false;

        [XmlAttribute]
        public bool TwitterModuleEnabled { get; set; } = false;

        [XmlAttribute]
        public bool ComicModuleEnabled { get; set; } = false;

        public List<string> ChattyRepo { get; set; }

        [XmlAttribute]
        public bool GamertagModuleEnabled { get; set; } = false;

        [XmlAttribute]
        public bool NoteModuleEnabled { get; set; } = false;

        [XmlAttribute]
        public bool PoliticsEnabled { get; set; } = false;

        [XmlAttribute]
        public bool ChatBattleEnabled { get; set; } = false;

        [XmlAttribute]
        public bool ChattyModeEnabled { get; set; } = false;

        [XmlAttribute]
        public bool MarkovListenerEnabled { get; set; } = false;
        [XmlAttribute]
        public bool BetaCanSpeak { get; set; } = true;
        [XmlAttribute]
        public bool CramEnabled { get; set; } = false;
        [XmlAttribute]
        public bool MemeGenEnabled { get; set; } = false;
        [XmlAttribute]
        public bool ScrumEnabled { get; set; } = false;
        [XmlAttribute]
        public DateTime ScrumReminderDateTIme { get; set; }

        public void EnableBetaSpeak()
        {
            BetaCanSpeak = true;
        }

        public void DisableBetaSpeak()
        {
            BetaCanSpeak = false;
        }

        public bool ToggleFeatureBool(string module)
        {
            switch (module)
            {
                case "ask":
                    this.AskEnabled = !this.AskEnabled;
                    return this.AskEnabled;
                case "motd":
                    this.MOTDEnabled = !this.MOTDEnabled;
                    return this.MOTDEnabled;
                case "roll":
                    this.RollEnabled = !this.RollEnabled;
                    return this.RollEnabled;
                case "quote":
                    this.QuoteModuleEnabled = !this.QuoteModuleEnabled;
                    return this.QuoteModuleEnabled;
                case "table":
                    this.TableUnflipEnabled = !this.TableUnflipEnabled;
                    return this.TableUnflipEnabled;
                case "twitter":
                    this.TwitterModuleEnabled = !this.TwitterModuleEnabled;
                    return this.TwitterModuleEnabled;
                case "comic":
                    this.ComicModuleEnabled = !this.ComicModuleEnabled;
                    return this.ComicModuleEnabled;
                case "gamertag":
                    this.GamertagModuleEnabled = !this.GamertagModuleEnabled;
                    return this.GamertagModuleEnabled;
                case "note":
                    this.NoteModuleEnabled = !this.NoteModuleEnabled;
                    return this.NoteModuleEnabled;
                case "politics":
                    PoliticsEnabled = !PoliticsEnabled;
                    return PoliticsEnabled;
                case "battle":
                    ChatBattleEnabled = !ChatBattleEnabled;
                    return ChatBattleEnabled;
                case "chatty":
                    ChattyModeEnabled = !ChattyModeEnabled;
                    return ChattyModeEnabled;
                case "markov":
                    MarkovListenerEnabled = !MarkovListenerEnabled;
                    return MarkovListenerEnabled;
                case "cram":
                    CramEnabled = !CramEnabled;
                    return CramEnabled;
                case "meme":
                    MemeGenEnabled = !MemeGenEnabled;
                    return MemeGenEnabled;
                default:
                    return false;
            }

        }

        public void AddMessageToChattyRepo(string msg)
        {
            if (msg.StartsWith("$")) return; //Ignore Commands
            
            ChattyRepo.Add(msg);
            if (ChattyRepo.Count > 400)
            {
                ChattyRepo.RemoveAt(0);
            }                                              
        }
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
