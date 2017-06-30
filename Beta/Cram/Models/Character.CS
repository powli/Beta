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
        [Key()]
        public int CharacterID { get; set; }
        public string Name { get; set; }
        public int PHY { get; set; }
        public int MEN { get; set; }
        public int VIT { get; set; }
        public int LUC { get; set; }
        public double Cash { get; set; }
        public int SkillPoints { get; set; }
        public List<Item> Items { get; set; }

        public List<Skill> Skills { get; set; }

    }

    class CharacterContext : DbContext
    {
        public CharacterContext(): base("name=DefaultConnection") { }

        public DbSet<Character> Characters { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Skill> Skills { get; set; }
    }
}
