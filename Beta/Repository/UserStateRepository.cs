using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Beta.Modules;
using Discord;
using Discord.Commands;

namespace Beta.Repository
{ //d9b81799de621722acdba808d34d9123c99bdee8
    [XmlInclude(typeof(BetaUserState))]
    [Serializable]
    public class UserStateRepository
    {
        public const string _Filename = "UserStates.xml";

        

        [XmlIgnore] public static List<string> NPCNames = new List<string>()
        {
            "Storm Trooper",
            "Jedi",
            "Imperial Officer",
            "Republic Soldier",
            "Republic Officer",
            "Sith",
            "Gerblin",
            "Orc",
            "Bounty Hunter",
            "Elf",
            "Dwarf",
            "Vulcan",
            "Rabbit",
            "Deer",
            "Ewok",
            "Wookie",
            "Hutt",
            "Tusken Raiders",
            "Romulan",
            "Ferengi",
            "Borg",
            "Klingon",
            "Tribble",
            "Besalisk",
            "Bith",
            "Twi'lek",
            "Jawa",
            "Sand Person",
            "Rodian",
            "Gamorrean",
            "Mon Calamari",
            "Gungan",

        };

        public UserStateRepository()
        {
            UserStates = new List<UserState>();
            NPCUserStates = new List<NPCUserState>();
        }

        [XmlArrayItem("UserState")]
        public List<UserState> UserStates { get; set; }
        [XmlArrayItem("NPCUserState")]
        public List<NPCUserState> NPCUserStates { get; set; }
        
        public void IncrementKappaMessageCount(ulong id)
        {
            UserState usr = GetUserState(id);
            usr.KappaViolations[0].MessageCount++;
        }

        public void EvaluateKappaViolations()
        {
            foreach (UserState user in UserStates)
            {
                user.EvaluateKappaViolations();
            }
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
                    Favorability = 0,
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

        public void AddUser(string name, string type)
        {
            var r = new Random();
            int level = r.Next(1, 10);
            if (!VerifyNPCExists(name))
            {

                int hp = (level * (25 + r.Next(1, 25))); switch (type)
                {
                    case "beta":
                        level = 10;
                        NPCUserStates.Add((NPCUserState)new BetaUserState()
                        {
                            UserName = "Beta",
                            RPGLevel = level,
                            TableFlipPoints = 0,
                            Favorability = 0,
                            IsImmortal = true,
                            CanRun = false,
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
                            UserName = name,
                            RPGLevel = level,
                            TableFlipPoints = 0,
                            IsImmortal = true,
                            CanRun = false,
                            RPGMaxHP = hp,
                            RPGHitpoints = hp,
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
                            UserName = name,
                            RPGLevel = level,
                            TableFlipPoints = 0,
                            IsImmortal = false,
                            CanRun = true,
                            RPGMaxHP = hp,
                            RPGHitpoints = hp,
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
            return UserStates.FirstOrDefault(us => us.UserId == id) != null;
        }

        internal int IncrementTableFlipPoints(ulong UsrId, int Points)
        {
            UserStates.FirstOrDefault(us => us.UserId == UsrId).TableFlipPoints += Points;
            Save();
            return GetUserState(UsrId).TableFlipPoints;
        }

        internal double ModifyUserFavorability(ulong UsrId, double Amount)
        {
            UserStates.FirstOrDefault(us => us.UserId == UsrId).Favorability += Amount;
            Save();
            return GetUserState(UsrId).Favorability;
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

        public bool VerifyNPCExists(string name)
        {
            return NPCUserStates.FirstOrDefault(nu => nu.UserName == name) != null;
        }

        internal void SpawnNPCs()
        {
            Random r = new Random();
            int spawnNumber = r.Next(0, 1);
            while (spawnNumber > 0 || NPCUserStates.Count <= 12)
            {
                AddUser(NPCNames.GetRandom(),"npc");
                spawnNumber--;
            }
        }
    }

    public class NPCUserState : UserState
    {

        private Random r = new Random();

        [XmlAttribute] public bool IsImmortal;
        [XmlAttribute] public bool CanRun;
        [XmlAttribute] public bool RanAway;

        public bool RunAwayCheck()
        {
            if (CanRun) return r.Next(1, 20) == 1;
            return false;

        }

        public override bool IsBot()
        {
            return true;
        }

        public void RunAway()
        {
            RanAway = true;
        }

        public override Spoils CalculateSpoils(UserState attacker)
        {
            int xp = 2;
            int gold = 100;
            int health = 0;
            int stam = 0;
            if (r.Next(10000) == 3) health++;
            if (r.Next(10000) == 7) stam++;
            if (attacker.RPGLevel > RPGLevel + 1)
            {
                xp = 0;
                gold = 1;
                health = 0;
                stam = 0;
            }
            if (RPGLevel > attacker.RPGLevel)
            {
                int diff = RPGLevel - attacker.RPGLevel;
                xp += diff;
                gold += diff*50;
            }
            return new Spoils(gold,xp,health,stam);
        }
    }

    public class BetaUserState : NPCUserState
    {
        Random r = new Random();

        new public Result Attack(UserState target, CommandEventArgs e)
        {
            int dmg = (int)((target.RPGLevel * .25) * r.Next(8, 100));
            target.RPGHitpoints -= dmg;
            if (target.RPGHitpoints <= 0)
            {
                Spoils spoils = target.Die(this);
                return new Result(true, dmg, this.ScoreKill(spoils));
            }
            else
            {
                return new Result(false, dmg);
            }
        }

        public Result Attack(UserState target, CommandEventArgs e, Result combatResult)
        {
            int dmg = (int)((target.RPGLevel * .25) * r.Next(8, 100));
            target.RPGHitpoints -= dmg;
            if (target.RPGHitpoints <= 0)
            {
                Spoils spoils = target.Die(this);
                if (combatResult.TargetDead)
                {
                    spoils.Gold = 0;
                    spoils.HealthPot = 0;
                    spoils.StamPot = 0;
                } 
                
                return new Result(true, dmg, this.ScoreKill(spoils));
            }
            else
            {
                return new Result(false, dmg);
            }
        }

        public async void CounterAttack(UserState target, CommandEventArgs e, Result combatResult)
        {
            Result betaResult = Attack(target, e, combatResult);

            await e.Channel.SendMessage(String.Format("I struck back with my trusty {0} {1} of {2} for {3} points of damage!",
                                                ChatBattleModule.WeaponPrefixList.GetRandom(), ChatBattleModule.WeaponList.GetRandom(),
                                                ChatBattleModule.WeaponSuffixList.GetRandom(), betaResult.Damage));

            if (combatResult.TargetDead && betaResult.TargetDead)
            {

                await
                    e.Channel.SendMessage(
                        String.Format(
                            "I levied a counter attack, felling {0}. However I was fatal wounded, and died shortly thereafter. I shall return...", target.UserName));
                await
                    e.Channel.SendMessage(String.Format("I gained {0} XP! {1} gained {2} XP!",
                        betaResult.Spoils.XP, target.UserName, combatResult.Spoils.XP));
                target.CheckLevelUp(e);
                /* Due to the ScoreKill method currently
                 * adding spoils we must undo "pickup" gains
                 * of gold and potions.
                 */
                target.RPGHealingPotions -= combatResult.Spoils.HealthPot;
                target.RPGStaminaPotions -= combatResult.Spoils.StamPot;
                target.RPGGold -= combatResult.Spoils.Gold;
                CheckLevelUp(e);
            }
            //Original attacker dies and Beta survives
            else if (!combatResult.TargetDead && betaResult.TargetDead)
            {
                await
                    e.Channel.SendMessage(
                        String.Format(
                            "I sure taught {0} a lesson! I earned {1} XP and was able to loot {2} gold from the corpse!", target.UserName, betaResult.Spoils.XP, betaResult.Spoils.Gold));
                target.CheckLevelUp(e);
            }
            //Beta dies and original attacker survives
            else if (combatResult.TargetDead && !betaResult.TargetDead)
            {
                int num = r.Next(1, 3);
                await
                    e.Channel.SendMessage(
                        string.Format(
                            "{0} has taken me down! They gained {1} XP and found {2} gold on my corpse! Don't get too cocky, I'll be back.",
                            target.UserName, combatResult.Spoils.XP, combatResult.Spoils.Gold));
                target.CheckLevelUp(e);
                if (combatResult.Spoils.HealthPot > 0 || combatResult.Spoils.StamPot > 0)
                {
                    await
                        e.Channel.SendMessage(
                            string.Format("Oh! Looks like you also got {0} Healing Potions and {1} Stamina Potions",
                                combatResult.Spoils.HealthPot, combatResult.Spoils.StamPot));
                    ;
                }

            }

        }

        public async void DefendBot(UserState target, UserState attacker, CommandEventArgs e, Result combatResult)
        {
            Result betaResult = Attack(target, e, combatResult);
            await
                                        e.Channel.SendMessage(
                                            String.Format(
                                                "Sensing my fellow bot is in danger I leap into action, stricking back with my trusty {0} {1} of {2} for {3} points of damage!",
                                                ChatBattleModule.WeaponPrefixList.GetRandom(), ChatBattleModule.WeaponList.GetRandom(),
                                                ChatBattleModule.WeaponSuffixList.GetRandom(), betaResult.Damage));
            //Both targets have died
            if (combatResult.TargetDead && betaResult.TargetDead)
            {
                await
                    e.Channel.SendMessage(
                        String.Format(
                            "I levied a counter attack, felling {0}. However I was too late, {1} died shortly thereafter. I shall miss you, old friend...",
                            attacker.UserName, target.UserName));
                await
                    e.Channel.SendMessage(String.Format("I gained {0} XP! {1} gained {2} XP!",
                        betaResult.Spoils.XP, attacker.UserName, combatResult.Spoils.XP));
                attacker.RPGXP += combatResult.Spoils.XP;
                attacker.CheckLevelUp(e);
                /* Due to the ScoreKill method currently
                 * adding spoils we must undo "pickup" gains
                 * of gold and potions.
                 */
                attacker.RPGHealingPotions -= combatResult.Spoils.HealthPot;
                attacker.RPGStaminaPotions -= combatResult.Spoils.StamPot;
                attacker.RPGGold -= combatResult.Spoils.Gold;

                RPGXP += betaResult.Spoils.XP;
                CheckLevelUp(e);
            }
            //Original attacker dies and R2 survives
            else if (!combatResult.TargetDead && betaResult.TargetDead)
            {
                await
                    e.Channel.SendMessage(
                        String.Format(
                            "I sure taught {0} a lesson! Leave those poor defenseless bots alone! I earned {1} XP and was able to loot {2} gold from the corpse!",
                            attacker.UserName, betaResult.Spoils.XP, betaResult.Spoils.Gold));
                target.RPGXP += betaResult.Spoils.XP;
                target.RPGGold += betaResult.Spoils.Gold;
                target.CheckLevelUp(e);
            }
            //Beta dies and original attacker survives
            else if (combatResult.TargetDead && !betaResult.TargetDead)
            {
                int num = r.Next(1, 3);
                await
                    e.Channel.SendMessage(
                        String.Format(
                            "{0} has taken {4} down, and I was unable to put a stop to them! They gained {1} XP and found {2} gold on R2's corpse! Don't get too cocky, I'll get you next time...",
                            attacker.UserName, combatResult.Spoils.XP, combatResult.Spoils.Gold, target.UserName));
                attacker.RPGXP += combatResult.Spoils.XP;
                attacker.RPGGold += combatResult.Spoils.Gold;
                attacker.CheckLevelUp(e);
                
            }
        }



        public override Spoils CalculateSpoils(UserState attacker)
        {
            int xp = 3;
            int healthPot = 0;
            var stamPot = 0;
            var gold = attacker.RPGLevel * r.Next(50, 100);            
            if (r.Next(1000) == 3) stamPot++;
            if (r.Next(1000) == 7) healthPot++;
            if (r.Next(1000) == 10)
            {
                stamPot++;
                healthPot++;
            }
            return new Spoils(gold, xp, healthPot, stamPot);
        }

    }

    public class R2UserState : NPCUserState
    {
        
    }

    public class UserState
    {
        #region Attributes
        [XmlAttribute] public static int BanditGold = 14;

        [XmlIgnore] private readonly Random r = new Random();

        [XmlAttribute]
        public ulong UserId { get; set; }

        [XmlAttribute]
        public string UserName { get; set; }

        [XmlAttribute]
        public int TableFlipPoints { get; set; }

        [XmlAttribute]
        public double Favorability { get; set; } = 0;

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

        [XmlArrayItem("KappaViolations")]
        public List<KappaViolation> KappaViolations {get; set;} = new List<KappaViolation>();

        [XmlAttribute]
        public int SelectedCharacter { get; set; } = 0;

        [XmlAttribute]
        public int SelectedGameCharacter { get; set; } = 0;

        [XmlAttribute]
        public int SelectedGame { get; set; } = 0;

        [XmlAttribute]
        public string SelectedCharacterName { get; set; }

        [XmlAttribute]
        public string SelectedGameCharacterName { get; set; }

        [XmlAttribute]
        public string SelectedGameName { get; set; }
        #endregion

        public void AddKappaViolation()
        {
            KappaViolations.Add(new KappaViolation()
            {
                VioltionDateTime = DateTime.Now
            });
        }

        public void EvaluateKappaViolations()
        {            
            List<KappaViolation> forgivenViolations = new List<KappaViolation>();
            foreach (KappaViolation violation in KappaViolations)
            {                
                if ((violation.VioltionDateTime.AddHours(1) < DateTime.Now) || (violation.MessageCount > 200))
                {
                    forgivenViolations.Add(violation);                    
                }
            }
            foreach (KappaViolation violation in forgivenViolations)
            {
                KappaViolations.Remove(violation);
            }            
        }
        
        public bool HasKappaViolations()
        {
            return KappaViolations.Count > 0;
        }

        public virtual bool IsBot()
        {
            return false;
        }

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
                
        public Spoils ScoreKill(Spoils spoils)
        {
            RPGWins++;
            RPGGold += spoils.Gold;
            RPGXP += spoils.XP;
            RPGStaminaPotions += spoils.StamPot;
            RPGHealingPotions += spoils.HealthPot;

            return spoils;
        }        

        public Spoils Die(UserState attacker)
        {
            Die();
            return CalculateSpoils(attacker);

        }

        public virtual Spoils CalculateSpoils(UserState attacker)
        {
            int xp = 1;
            int healthPot = 0;
            var stamPot = 0;
            var gold = attacker.RPGLevel * r.Next(1, 25);
            if (attacker.RPGLevel < RPGLevel) xp = 1 + RPGLevel - attacker.RPGLevel;
            else if (RPGLevel - attacker.RPGLevel < -3) xp = 0;
            if (r.Next(1000) == 3) stamPot++;
            if (r.Next(1000) == 7) healthPot++;
            if (r.Next(1000) == 10)
            {
                stamPot++;
                healthPot++;
            }
            return new Spoils(gold,xp,healthPot,stamPot);
        }
                                                                
        public void Die()
        {
            RPGLosses++;
            Alive = false;
            RPGHitpoints = 0;
            if (UserName == "Beta") Res();
        }

        private int CalculateBetaGold(UserState attacker)
        {
            return r.Next(50, 100) * attacker.RPGLevel;
        }

        private int CalculateBetaXP(UserState attacker)
        {
            return 3;
        }

        public void Res()
        {
            Alive = true;
            RPGHitpoints = RPGMaxHP;
        }

        public void Res(int cost)
        {
            Res();
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

        public Result Attack(UserState target, CommandEventArgs e)
        {
            if (target.RPGHitpoints == -1) target.RPGHitpoints = target.RPGMaxHP;
            int dmg = (int) ((RPGLevel*.25)*r.Next(4, 50));
            target.RPGHitpoints -= dmg;
            if (target.RPGHitpoints <= 0)
            {
                Spoils spoils = target.Die(this);
                return new Result(true, dmg, this.ScoreKill(spoils));
            }
            else
            {
                return new Result(false, dmg);
            }
        }

        public void ClearAllKappas()
        {
            KappaViolations = new List<KappaViolation>();
        }
    }

    public class KappaViolation
    {
        [XmlAttribute]
        public DateTime VioltionDateTime {get; set;}
        
        [XmlAttribute]
        public int MessageCount = 0;        
    }

    public struct Spoils
    {
        public int Gold;
        public int XP;
        public int HealthPot;
        public int StamPot;


        public Spoils(int gold, int xp, int healthPot, int stamPot)
        {
            Gold = gold;
            XP = xp;
            HealthPot = healthPot;
            StamPot = stamPot;
        }
    }

    public class Result
    {
        public bool TargetDead;
        public int Damage;
        public Spoils Spoils;

        public Result(bool dead, int dmg)
        {
            TargetDead = dead;
            Damage = dmg;
        }

        public Result(bool dead, int dmg, Spoils spoils)
        {
            TargetDead = dead;
            Damage = dmg;
            Spoils = spoils;
        }
    }
}