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
    class CharacterItem : Item
    {
        //The ID of the item from the Item Table        
        public int ItemParentID { get; set; }
        public int QuantityOwned { get; set; }        

        public CharacterItem(Item item, int quantity, int charID):base(item)
        {
            ItemParentID = item.ItemID;
            QuantityOwned = quantity;            
        }        

        public CharacterItem()
        {

        }
    }
}
