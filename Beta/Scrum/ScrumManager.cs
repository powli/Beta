using Beta.Scrum.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta.Scrum
{
    class ScrumManager
    {
        public static void AddNewUpdate(string update, string updater, ulong channelId)
        {
            using (ScrumContext db = new ScrumContext())
            {
                db.Updates.Add(new ScrumUpdate(update, updater, channelId));
                db.SaveChanges();
            }
        }
    }

    class ScrumContext : DbContext
    {
        public ScrumContext() : base("name=ScrumDB") { }

        public DbSet<ScrumUpdate> Updates { get; set; }
    }
}
