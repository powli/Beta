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
    [Table("CharacterItems")]
    class CharacterItem 
    {
        [Key]
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public double ItemCost { get; set; }
        //The ID of the item from the Item Table        
        public int ItemParentID { get; set; }
        public int QuantityOwned { get; set; }        

        public int CharacterId { get; set; }
        [ForeignKey("CharacterId")]
        public virtual Character character { get; set; }
        public CharacterItem(string name, string desc, double cost, int id, int quantity)
        {
            ItemName = name;
            ItemDescription = desc;
            ItemCost = cost;
            ItemParentID = id;
            QuantityOwned = quantity;            
        }        

        public CharacterItem()
        {

        }
    }
}
