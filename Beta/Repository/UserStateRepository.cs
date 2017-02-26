using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Discord;
using Discord.Commands;

namespace Beta.Repository
{ //d9b81799de621722acdba808d34d9123c99bdee8

    [Serializable]
    public class UserStateRepository
    {
        public const string _Filename = "UserStates.xml";

        public UserStateRepository()
        {
            UserStates = new List<UserState>();
            NPCUserStates = new List<NPCUserState>();
        }

        [XmlArrayItem("UserState")]
        public List<UserState> UserStates { get; set; }
        [XmlArrayItem("NPCUserState")]
        public List<NPCUserState> NPCUserStates { get; set; }

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
                    RPGMaxHP = (int)25 + r.Next(1, 25),
                    RPGGold = r.Next(1, 25),
                    RPGLevel = 1,
                    RPGLosses = 0,
                    RPGWins = 0,
                    RPGXP = 0


                });
                Save();
            }
        }

        public void AddUser(User usr, string type)
        {
            var r = new Random();
            int level = r.Next(1, 10);
            if (!VerifyUsersExists(usr.Id))
            {
                switch (type)
                {
                    case "beta":
                        level = 10;
                        NPCUserStates.Add(new BetaUserState()
                        {
                            UserName = "Beta",
                            RPGLevel = level,
                            TableFlipPoints = 0,
                            BetaAbusePoints = 0,
                            IsImmortal = true,
                            RPGMaxHP = level * (25 + r.Next(1, 25)),
                            RPGGold = level * (r.Next(1, 25)),
                            RPGLosses = 0,
                            RPGWins = 0,
                            RPGXP = 0
                        });
                        break;
                    case "bot":
                        level = r.Next(7, 10);
                        NPCUserStates.Add(new NPCUserState()
                        {                            
                            UserName = usr.Name,
                            RPGLevel = level,
                            TableFlipPoints = 0,
                            BetaAbusePoints = 0,
                            IsImmortal = true,
                            RPGMaxHP = level * (25 + r.Next(1, 25)),
                            RPGGold = level * (r.Next(1, 25)),
                            RPGLosses = 0,
                            RPGWins = 0,
                            RPGXP = 0
                        });
                        break;
                    default:
                        level = r.Next(1, 10);
                        NPCUserStates.Add(new NPCUserState()
                        {
                            UserId = usr.Id,
                            UserName = usr.Name,
                            RPGLevel = level,
                            TableFlipPoints = 0,
                            BetaAbusePoints = 0,
                            IsImmortal = false,
                            RPGMaxHP = level*(25 + r.Next(1, 25)),
                            RPGGold = level*(r.Next(1, 25)),                            
                            RPGLosses = 0,
                            RPGWins = 0,
                            RPGXP = 0
                        });
                        break;
                }


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
            var serializer = new XmlSerializer(typeof(UserStateRepository));
            using (var file = File.Create(_Filename))
            {
                serializer.Serialize(file, this);
            }
        }

        public static UserStateRepository LoadFromDisk()
        {
            UserStateRepository dictionary;
            if (File.Exists(_Filename))
            {
                var serializer = new XmlSerializer(typeof(UserStateRepository));
                using (var file = new FileStream(_Filename, FileMode.Open))
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
            foreach (var usr in UserStates)
            {
                var minStaminaIncrease = (decimal) .25;
                decimal minHealthIncrease = 1;
                if (usr.RPGLevel/2 > minStaminaIncrease) minStaminaIncrease = usr.RPGLevel/2;
                if (usr.RPGLevel/2 > minHealthIncrease) minHealthIncrease = usr.RPGLevel/2;
                if (usr.RPGStamina < usr.RPGMaxStamina) usr.RPGStamina += minStaminaIncrease;
                if (usr.RPGHitpoints < usr.RPGMaxHP && usr.Alive) usr.RPGHitpoints += (int) minHealthIncrease;
                if (usr.RPGHitpoints <= 0)
                {
                    usr.Alive = false;
                    usr.RPGHitpoints = 0;
                }
                if (usr.RPGHitpoints > usr.RPGMaxHP) usr.RPGHitpoints = usr.RPGMaxHP;
                if (usr.RPGStamina > usr.RPGMaxStamina) usr.RPGStamina = usr.RPGMaxStamina;
                try
                {
                    usr.CheckLevelUp(beta.GetUser(usr.UserId));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[UserStateRepository] Checking level-up for user failed.");
                    Console.WriteLine("[UserStateRepository] Error Message:" + ex.Message);
                    Console.WriteLine("[UserStateRepository] User ID: " + usr.UserId + " Username: " + usr.UserName);
                }
            }
            Save();
        }
    }

    public class NPCUserState : UserState
    {
        [XmlAttribute] public bool IsImmortal;
    }

    public class BetaUserState : NPCUserState
    {
    }

    public class UserState
    {
        [XmlAttribute] public static int BanditGold = 14;

        [XmlIgnore] private readonly Random r = new Random();

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
        public decimal RPGStamina { get; set; } = 3;

        [XmlAttribute]
        public int RPGMaxStamina { get; set; } = 3;

        [XmlAttribute]
        public int RPGHealingPotions { get; set; }

        [XmlAttribute]
        public int RPGStaminaPotions { get; set; }

        [XmlAttribute]
        public bool Alive { get; set; } = true;

        public void CheckLevelUp(CommandEventArgs e)
        {
            while (RPGXP >= RPGLevel*RPGLevel)
            {
                var increasedStats = LevelUp();
                e.Channel.SendMessage(
                    string.Format(
                        "LEVEL UP! {0}'s level has gone up to {1}, and they have gained {2} HP and {3} Stamina!",
                        UserName, RPGLevel, increasedStats[0], increasedStats[1]));
            }
        }

        public void CheckLevelUp(User usr)
        {
            while (RPGXP >= RPGLevel*RPGLevel)
            {
                var increasedStats = LevelUp();
                usr.SendMessage(
                    string.Format(
                        "LEVEL UP! {0}'s level has gone up to {1}, and they have gained {2} HP and {3} Stamina!",
                        UserName, RPGLevel, increasedStats[0], increasedStats[1]));
            }
        }

        private List<string> LevelUp()
        {
            var HP = r.Next(1, 25)*RPGLevel;
            var Stamina = r.Next(1, 3);
            RPGXP = RPGXP - RPGLevel*RPGLevel;
            RPGLevel++;
            RPGMaxHP += HP;
            RPGHitpoints += HP;
            RPGStamina += Stamina;
            RPGMaxStamina += Stamina;
            return new List<string>
            {
                HP.ToString(),
                Stamina.ToString()
            };
        }

        public Spoils ScoreKill(UserState target, CommandEventArgs e)
        {
            var healthPot = 0;
            var stamPot = 0;
            var xp = 1;
            var gold = target.RPGLevel*r.Next(1, 25);
            if (target.RPGLevel > RPGLevel) xp = 1 + target.RPGLevel - RPGLevel;
            else if (target.RPGLevel - RPGLevel < -3) xp = 0;
            RPGWins++;
            if (r.Next(1000) == 3) stamPot++;
            if (r.Next(1000) == 7) healthPot++;
            if (r.Next(1000) == 10)
            {
                stamPot++;
                healthPot++;
            }

            return new Spoils(gold, xp, healthPot, stamPot);
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
            var stam = r.Next(2, 4);
            RPGStamina += stam;
            if (RPGStamina > RPGMaxStamina) RPGStamina = RPGMaxStamina;
            RPGStamina--;
            return stam;
        }

        public int DrinkHealingPotion()
        {
            var healing = r.Next(12, 25);
            RPGHitpoints += healing;
            if (RPGHitpoints > RPGMaxHP) RPGHitpoints = RPGMaxHP;
            RPGHealingPotions--;
            return healing;
        }
    }

    public struct Spoils
    {
        public int Gold;
        public int XP;
        public int HealthPot;
        public int StamPot;


        public Spoils(int gold, int xp, int health, int stam)
        {
            Gold = gold;
            XP = xp;
            HealthPot = health;
            StamPot = stam;
        }
    }
}