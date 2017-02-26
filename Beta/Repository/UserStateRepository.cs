using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Beta.Modules;
using Discord.Commands;
using Discord.Modules;
using Tweetinvi;
using User = Discord.User;

namespace Beta.Repository
{//d9b81799de621722acdba808d34d9123c99bdee8
    [Serializable]
    public class UserStateRepository 
    {
        public const string _Filename = "UserStates.xml";

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

        public void UpdateUserStates(Beta beta)
        {
            foreach (UserState usr in UserStates)
            {
                if (usr.RPGStamina < usr.RPGMaxStamina) usr.RPGStamina++;
                if (usr.RPGHitpoints < usr.RPGMaxHP && usr.Alive) usr.RPGHitpoints++;
                if (usr.RPGHitpoints <= 0)
                {
                    usr.Alive = false;
                    usr.RPGHitpoints = 0;
                }
                try
                {
                    usr.CheckLevelUp(beta.GetUser(usr.UserId));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[UserStateRepository] Checking level-up for user failed.");
                    Console.WriteLine("[UserStateRepository] Error Message:"+ex.Message);
                    Console.WriteLine("[UserStateRepository] User ID: " + usr.UserId+" Username: "+usr.UserName);
                }
                
            }
            Save();
        }
    }

    public class UserState
    {
        [XmlIgnore]
        private Random r = new Random();

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
        public int RPGHealingPotions { get; set; } = 0;

        [XmlAttribute]
        public int RPGStaminaPotions { get; set; } = 0;

        [XmlAttribute]
        public bool Alive { get; set; } = true;

        [XmlAttribute]
        public static int BanditGold = 14;

        public void CheckLevelUp(CommandEventArgs e)
        {
            while (RPGXP >= (RPGLevel*RPGLevel))
            {
                List<string> increasedStats = LevelUp();
                e.Channel.SendMessage(String.Format("LEVEL UP! {0}'s level has gone up to {1}, and they have gained {2} HP and {3} Stamina!", UserName, RPGLevel, increasedStats[0], increasedStats[1]));
            }
        }

        public void CheckLevelUp(User usr)
        {
            while (RPGXP >= (RPGLevel * RPGLevel))
            {                
                List<string> increasedStats = LevelUp();
                usr.SendMessage(String.Format("LEVEL UP! {0}'s level has gone up to {1}, and they have gained {2} HP and {3} Stamina!", UserName, RPGLevel, increasedStats[0], increasedStats[1]));
            }
        }

        private List<string> LevelUp()
        {
            int HP = r.Next(1, 25) * RPGLevel;
            int Stamina = r.Next(1, 3);
            RPGXP = RPGXP - (RPGLevel * RPGLevel);
            RPGLevel++;
            RPGMaxHP += HP;
            RPGHitpoints += HP;
            RPGStamina += Stamina;
            RPGMaxStamina += Stamina;
            return new List<string>()
            {
                HP.ToString(),
                Stamina.ToString()
            };
        }

        public XPandGold ScoreKill(UserState enemy, CommandEventArgs e)
        {
            int xp = 1;
            int gold = (enemy.RPGLevel)*r.Next(1, 25);
            if (enemy.RPGLevel > RPGLevel) xp = 1 + enemy.RPGLevel - RPGLevel;
            else if (enemy.RPGLevel - RPGLevel < -3) xp = 0;
            RPGWins++;
            return new XPandGold(gold,xp);
        }

        public void Die()
        {
            RPGLosses++;
            Alive = false;
            RPGHitpoints = 0;
            if (UserName == "Beta") Res(0);
        }

        public void Res(int cost)
        {
            Alive = true;
            RPGHitpoints = RPGMaxHP;
            RPGGold -= cost;
        }

        public int DrinkStaminaPotion()
        {
            int stam = r.Next(2, 4);
            RPGStamina += stam;
            if (RPGStamina > RPGMaxStamina) RPGStamina = RPGMaxStamina;
            RPGStamina--;
            return stam;
        }

        public int DrinkHealingPotion()
        {
            int healing = r.Next(12, 25);
            RPGHitpoints += healing;
            if (RPGHitpoints > RPGMaxHP) RPGHitpoints = RPGMaxHP;
            RPGHealingPotions--;
            return healing;
        }

    }

    public struct XPandGold
    {
        public int Gold;
        public int XP;

        public XPandGold(int gold, int xp)
        {
            Gold = gold;
            XP = xp;
        }
    }
}
