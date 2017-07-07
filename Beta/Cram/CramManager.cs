using System;
using System.Collections.Generic;
using System.Data.Entity;
using Beta.Cram.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta.Cram
{
    class CramManager
    {
        internal static void InitializeCharacterDatabase()
        {
            using (CharacterContext db = new CharacterContext())
            {
                //Add default items to CharacterDB
                foreach (Item item in GenerateNewItemList())
                {
                    db.Items.Add(item);
                }

                //Add default Skills to CharacterDB
                foreach(Skill skill in GenerateNewSkillList())
                {
                    db.Skills.Add(skill);
                }
                db.SaveChanges();
            }
        }
        internal static List<Item> GenerateNewItemList()
        {
            return new List<Item>
            {
                new Item("Melee +1","PHY + 1 or 5 combat dice.",5.00),
                new Item("Melee +2","PHY + 2 or 8 combat dice.",25.00),
                new Item("Melee +3","PHY + 3 or 11 combat dice.",75.00),
                new Item("Projectile + 1","PHY + 1 or 5 combat dice.",50.00),
                new Item("Projectile + 2","PHY + 2 or 8 combat dice.",100.00),
                new Item("Projectile + 3","PHY + 3 or 11 combat dice.",250.00),
                new Item("Projectile + 4","PHY + 4 or 14 combat dice.",500.00),
                new Item("Light Armor","Damage Reduction: 1, Reduces wearer's PHY by 1.",50.00),
                new Item("Medium Armor","Damage Reduction: 2, Reduces wearer's PHY by 2.",300.00),
                new Item("Heavy Armor","Damage Reduction: 3, Reduces wearer's PHY by 5.",1500.00)
            };
        }

        internal static List<Skill> GenerateNewSkillList()
        {
            return new List<Skill>
            {
                new Skill("Athletics","Feats of endurance, brute strength, and acrobatics."),
                new Skill("Lore","History, folklore, languages, religion, philosophy, and the occult."),
                new Skill("Martial","All forms of armed and unarmed combat."),
                new Skill("Medicine","Healthcare, pharmacology, and surgery."),
                new Skill("Clairvoyance","Predictions, visions, and ESP."),
                new Skill("Telekinesis","Moving objects with your mind."),
                new Skill("Telepathy","Mind-reading, empathy, and thought transference"),
                new Skill("Rhetoric","Persuasive speaking, negotiation, and diplomacy."),
                new Skill("Science"," Mathematics, physics, chemistry, biology, etc."),
                new Skill("Subterfuge","Disguise, legerdemain, security, stealth, and streetwise."),
                new Skill("Survival","Hunting, trapping, tracking and foraging outdoors"),
                new Skill("Vocation","a specialized trade, e.g. Pilot, Soldier, or Tailor.")
            };        
        }

        internal static void AddNewCharacter(string name, int phy, int men, int vit, int luc, ulong userId)
        {
            using(CharacterContext db = new CharacterContext())
            {
                db.Characters.Add(new Character(name, phy, men, vit, luc, 2, userId));
                db.SaveChanges();
            }
        }

        internal static void AddNewGameCharacter(string name, int phy, int men, int vit, int luc, ulong userId, int gameId)
        {
            using(GameContext db = new GameContext())
            {
                Game game = db.Games.Find(gameId);
                game.Characters.Add(new Character(name, phy, men, vit, luc, 2, userId));
                db.SaveChanges();
            }
        }

        internal static void AddExistingGameCharacter(int characterId, int gameId)
        {

        }

        internal static string GetCharacters(string id)
        {
            string msg = "\n";
            using(CharacterContext db = new CharacterContext())
            {
                List<Character> chars = db.Characters.ToList<Character>();
                foreach (Character chr in chars)
                {
                    if (chr.UserId == id)
                    {
                        msg += chr.CharacterID+" | "+ chr.Name + " | " + chr.PHY + " | " + chr.MEN + " | " + chr.VIT + " | " + chr.LUC + " | " + chr.Cash + " | " + chr.SkillPoints +"\n";
                    }
                }
            }
            return msg;
            
        }

        internal static string GetSkills()
        {
            string msg = "\n";
            using (CharacterContext db = new CharacterContext())
            {
                List<Skill> skills = db.Skills.ToList<Skill>();
                foreach(Skill skill in skills)
                {
                    msg += skill.SkillID + " | " + skill.SkillName + " | " + skill.SkillDescription + "\n";
                }

            }
            return msg;
        }

        internal static string GetItems()
        {
            string msg = "\n";
            using (CharacterContext db = new CharacterContext())
            {
                List<Item> items = db.Items.ToList<Item>();
                foreach (Item item in items)
                {                                       
                    msg += item.ItemID + " | " + item.ItemName + " | " + item.ItemDescription + " | " + item.ItemCost + "\n";                    
                }
            }
            return msg;
        }

        internal static string GetCharacterItems(int selectedCharacter)
        {
            string msg = "\n";
            using (CharacterContext db = new CharacterContext())
            {
                List<CharacterItem> items = db.CharacterItems.ToList<CharacterItem>();
                foreach (CharacterItem item in items)
                {
                    if(item.CharacterId == selectedCharacter)
                    {
                        msg += item.ItemID + " | " + item.ItemName + " | " + item.ItemDescription + " | " + item.ItemCost + " | " + item.QuantityOwned + "\n";
                    }
                }
            }
            return msg;
        }
    }

    

    class GameContext : CharacterContext
    {
        public GameContext() : base("name=GameDB") { }

        public DbSet<Game> Games { get; set; }
    }
    class CharacterContext : DbContext
    {
        public CharacterContext() : base("name=CharacterDB") { }
        public CharacterContext(string dbName) : base(dbName) { }

        public DbSet<Item> Items { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<CharacterItem> CharacterItems { get; set; }
        public DbSet<CharacterSkill> CharacterSkills { get; set; }
    }
}
