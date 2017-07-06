using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta.Cram.Models
{
    [Table("CharacterSkills")]
    class CharacterSkill : Skill
    {
        public int CharacterID { get; set; }

        [ForeignKey("CharacterID")]
        public virtual Character Character { get; set; }
        public CharacterSkill(string name, string desc) : base( name, desc)
        {

        }

        public CharacterSkill()
        {

        }

    }
}
