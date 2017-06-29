using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta.Cram.Models
{
    class Item
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public double ItemCost { get; set; }
        public int QuantityOwned { get; set; }
    }
}
