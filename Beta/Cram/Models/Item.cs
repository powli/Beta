using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta.Cram.Models
{
    class Item
    {
        [Key]
        public int ItemID { get; set; }        
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public double ItemCost { get; set; }

        public Item(Item item)
        {
            ItemName = item.ItemName;
            ItemDescription = item.ItemDescription;
            ItemCost = item.ItemCost;
        }

        public Item (string name, string description, double cost)
        {
            ItemName = name;
            ItemDescription = description;
            ItemCost = cost;
        }

        public Item()
        {

        }
    }
}
