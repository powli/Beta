using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Discord;
using Discord.Commands;

namespace Beta.Repository
{
    [Serializable]
    public class UserStateRepository
    {
        private const string _Filename = "UserStates.xml";

        [XmlArrayItem("UserState")]
        public List<UserState> UserStates
        {
            get;
            set;
        }

        public UserStateRepository()
        {
            UserStates = new List<UserState>();
        }

        public void AddUser(User usr)
        {
            if (!VerifyUsersExists(usr.Id))
            {
                Random r = new Random();
                UserStates.Add(new UserState()
                {
                    UserId = usr.Id,
                    UserName = usr.Name,
                    TableFlipPoints = 0,
                    BetaAbusePoints = 0,
                    RPGMaxHP = (int) 25 + r.Next(1,25),                    
                    RPGGold = r.Next(1,25),
                    RPGLevel = 1,
                    RPGLosses = 0,
                    RPGWins = 0,
                    RPGXP = 0
                    
                    
                });
                Save();
            }
        }

        public bool VerifyUsersExists(ulong id)
        {
            if (UserStates.FirstOrDefault(us => us.UserId == id) != null) return true;
            return false;
        }

        internal int IncrementTableFlipPoints(ulong UsrId, int Points)
        {
            UserStates.FirstOrDefault(us => us.UserId == UsrId).TableFlipPoints += Points;
            Save();
            return GetUserState(UsrId).TableFlipPoints;
        }

        internal int IncrementBetaAbusePoints(ulong UsrId, int Points)
        {
            UserStates.FirstOrDefault(us => us.UserId == UsrId).BetaAbusePoints += Points;
            Save();
            return GetUserState(UsrId).BetaAbusePoints;
        }

        public UserState GetUserState(ulong id)
        {
            return UserStates.FirstOrDefault(us => us.UserId == id);
        }

        public void ResUser(int cost, ulong id)
        {
            GetUserState(id).Res(cost);
            Save();
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserStateRepository));
            using (FileStream file = File.Create(_Filename))
            {
                serializer.Serialize(file, this);
            }
        }

        public static UserStateRepository LoadFromDisk()
        {
            UserStateRepository dictionary;
            if (File.Exists(_Filename))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(UserStateRepository));
                using (FileStream file = new FileStream(_Filename, FileMode.Open))
                {
                    dictionary = serializer.Deserialize(file) as UserStateRepository;
                }
            }
            else
            {
                dictionary = new UserStateRepository();
                dictionary.Save();
            }
            return dictionary;
        }

        public void RefreshStamina()
        {
            foreach (UserState usr in UserStates)
            {
                if (usr.RPGStamina < usr.RPGMaxStamina)
                {
                    usr.RPGStamina++;
                }
            }
            Save();
        }
    }

    public class UserState
    {
        
        [XmlAttribute]
        public ulong UserId { get; set; }

        [XmlAttribute]
        public string UserName { get; set; }       

        [XmlAttribute]
        public int TableFlipPoints { get; set; }        

        [XmlAttribute]
        public int BetaAbusePoints { get; set; }

        [XmlAttribute]
        public int RPGMaxHP { get; set; }

        [XmlAttribute]
        public int RPGHitpoints { get; set; } = -1;

        [XmlAttribute]  
        public int RPGLevel { get; set; }

        [XmlAttribute]
        public int RPGGold { get; set; }

        [XmlAttribute]
        public int RPGXP { get; set; }

        [XmlAttribute]
        public int RPGWins { get; set; }

        [XmlAttribute]
        public int RPGLosses { get; set; }

        [XmlAttribute]
        public int RPGStamina { get; set; } = 3;

        [XmlAttribute]    
        public int RPGMaxStamina { get; set; } = 3;

        [XmlAttribute]
        public bool Alive { get; set; } = true;

        [XmlAttribute]
        public static int BanditGold = 14;

        public void CheckLevelUp(CommandEventArgs e)
        {
            if (this.RPGXP >= (this.RPGLevel*10))
            {
                Random r = new Random();
                int HP = r.Next(1, 25)*this.RPGLevel;
                int Stamina = r.Next(1, 3);
                this.RPGXP = this.RPGXP - (this.RPGLevel*10);
                this.RPGLevel++;
                this.RPGMaxHP += HP;
                this.RPGHitpoints += HP;
                this.RPGStamina += Stamina;
                this.RPGMaxStamina += Stamina;
                e.Channel.SendMessage(String.Format("LEVEL UP! {0}'s level has gone up to {1}, and they have gained {2} HP and {3} Stamina!", e.User.Name, this.RPGLevel, HP, Stamina));


            }
        }

        public void Res(int cost)
        {
            Alive = true;
            RPGHitpoints = RPGMaxHP;
            RPGGold -= cost;

        }

    }
}
