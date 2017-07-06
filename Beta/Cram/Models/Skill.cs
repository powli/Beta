using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta.Cram.Models
{
    class Skill
    {
        [Key]
        public int SkillID { get; set; }
        public string SkillName { get; set; }
        public string SkillDescription { get; set; }

        public Skill (string Name, string Description)
        {            
            SkillName = Name;
            SkillDescription = Description;
        }
        public Skill()
        {

        }
    }
}
