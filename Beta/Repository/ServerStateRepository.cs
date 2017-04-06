using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Discord;

namespace Beta.Repository
{
    [Serializable]
    public class ServerStateRepository
    {
        private const string _Filename = "ServerStates.xml";

        [XmlArrayItem("ServerState")]
        public List<ServerState> ServerStates { get; set; }

        public ServerStateRepository()
        {
            ServerStates = new List<ServerState>();
        }

        public void AddServer(Server srvr)
        {
            if (!VerifyStateExists(srvr.Id))
            {
                ServerStates.Add(new ServerState()
                {
                  ServerID   = srvr.Id,
                  ServerName = srvr.Name,
                  NoteRepository = new NoteRepository()

                });
            }
            Save();
        }

        public bool VerifyStateExists(ulong id)
        {
            if (ServerStates.FirstOrDefault(ss => ss.ServerID == id) != null) return true;
            return false;
        }

        public ServerState GetServerState(ulong id)
        {
            return ServerStates.FirstOrDefault(ss => ss.ServerID == id);
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ServerStateRepository));
            using (FileStream file = File.Create(_Filename))
            {
                serializer.Serialize(file, this);
            }
        }

        public static ServerStateRepository LoadFromDisk()
        {
            ServerStateRepository dictionary;
            if (File.Exists(_Filename))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ServerStateRepository));
                using (FileStream file = new FileStream(_Filename, FileMode.Open))
                {
                    dictionary = serializer.Deserialize(file) as ServerStateRepository;
                }
            }
            else
            {
                
                    dictionary = new ServerStateRepository();
                    dictionary.Save();                
            }
            return dictionary;
        }
    }

    public class ServerState
    {        
        [XmlAttribute]
        public ulong ServerID { get; set;}

        [XmlAttribute]
        public string ServerName { get; set; }

        [XmlAttribute]
        public bool MOTDEnabled { get; set; } = false;

        [XmlAttribute]
        public bool RollEnabled { get; set; } = false;

        [XmlAttribute]
        public bool AskEnabled { get; set; } = false;

        [XmlAttribute]
        public ulong KappaChannel { get; set; } = 0;

        [XmlAttribute]
        public bool TableUnflipEnabled { get; set; } = false;

        [XmlAttribute]
        public bool QuoteModuleEnabled { get; set; } = false;

        [XmlAttribute]
        public bool TwitterModuleEnabled { get; set; } = false;

        [XmlAttribute]
        public bool ComicModuleEnabled { get; set; } = false;

        [XmlAttribute]
        public bool GamertagModuleEnabled { get; set; } = false;

        [XmlAttribute]
        public bool NoteModuleEnabled { get; set; } = false;

        [XmlElement]
        public NoteRepository NoteRepository { get; set; }
        [XmlAttribute]
        public bool PoliticsEnabled { get; set; } = false;

        [XmlAttribute]
        public bool ChatBattleEnabled { get; set; } = false;

        [XmlAttribute]
        public bool ChattyModeEnabled { get; set; } = false;

        [XmlAttribute]
        public bool MarkovListenerEnabled { get; set; } = false;        

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
                default:
                    return false;
            }
                
        }
    }    
}
