using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta.Cram.Models
{
    [Table("Characters")]
    class Character
    {
        [Key]
        public int CharacterID { get; set; }
        public string Name { get; set; }
        public int PHY { get; set; }
        public int MEN { get; set; }
        public int VIT { get; set; }
        public int LUC { get; set; }
        public double Cash { get; set; }
        public int SkillPoints { get; set; }
        public Guid OwnerGUID { get; set; }

        public virtual List<CharacterItem> CharacterItems { get; set; }

        public virtual List<CharacterSkill> CharacterSkills { get; set; }

        public Character (string name, int phy, int men, int vit, int luc, int sp, Guid guid)
        {
            Name = name;
            PHY = phy;
            MEN = men;
            VIT = vit;
            LUC = luc;
            Cash = LUC * 15;
            SkillPoints = sp;
            CharacterItems = new List<CharacterItem>();
            CharacterSkills = new List<CharacterSkill>();
            OwnerGUID = guid;
        }
        public Character()
        {
            CharacterContext db = new CharacterContext();
            CharacterSkills = db.CharacterSkills.ToList<CharacterSkill>();
            CharacterItems = db.CharacterItems.ToList<CharacterItem>();
        }
        //Adds the item to the character,
        public void AddItem(Item item, int quantity)
        {
            if (this.CharacterItems.FirstOrDefault(i => i.ItemParentID == item.ItemID) != null)
            {
                CharacterItems.FirstOrDefault(i => i.ItemParentID == item.ItemID).QuantityOwned += quantity;
            }
            else
            {
                CharacterItems.Add(new CharacterItem(item, quantity, CharacterID));
            }

        }
    
    }

    
}
