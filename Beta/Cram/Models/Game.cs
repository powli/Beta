using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta.Cram.Models
{
    class Game
    {
        public int GameID { get; set; }
        public virtual List<Character> Characters { get; set; }
        public virtual List<Item> Items { get; set; }
        public virtual List<Skill> Skills { get; set; }        
    }

}
