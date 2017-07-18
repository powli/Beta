using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Beta;
using Beta.Repository;
using System.Collections.Generic;

namespace BetaTests
{
    [TestClass]
    public class ScrumTests
    {
        [TestMethod]
        public void TestFireScrumCheck()
        {
            ChannelState chnl_1 = new ChannelState();            
            ChannelState chnl_2 = new ChannelState();            

            chnl_1.ScrumEnabled = true;
            chnl_2.ScrumEnabled = true;

            chnl_1.ScrumReminderDateTime = DateTime.Now.AddHours(2);
            chnl_2.ScrumReminderDateTime = DateTime.Now.AddMinutes(-5);
            
            Assert.AreEqual(Beta.Beta.FireScrumCheck(chnl_1), false);
            Assert.AreEqual(Beta.Beta.FireScrumCheck(chnl_2), true);
        }

        public ChannelState CreateChannelState()
        {
            ChannelState chnl = new ChannelState();
            chnl.ScrumEnabled = true;
            chnl.ScrumerIds = new List<ulong>();
            chnl.UpdatedScrumerIds = new List<ulong>();

            return chnl;
        }

        [TestMethod]
        public void TestGetUnupdatedScrummers()
        {
            ChannelState chnl_1 = CreateChannelState();
            ChannelState chnl_2 = CreateChannelState();
            ChannelState chnl_3 = CreateChannelState();


            ulong userSeed = new ulong();
            userSeed += 437354;

            chnl_1.ScrumEnabled = true;
            chnl_2.ScrumEnabled = true;
            chnl_3.ScrumEnabled = true;

            chnl_1.ScrumerIds.Add(userSeed);
            chnl_1.ScrumerIds.Add(userSeed * 2);
            chnl_1.ScrumerIds.Add(userSeed * 4);
            chnl_1.ScrumerIds.Add(userSeed * 5);
            chnl_1.UpdatedScrumerIds.Add(userSeed);

            chnl_2.ScrumerIds.Add(userSeed);
            chnl_2.UpdatedScrumerIds.Add(userSeed);

            chnl_3.ScrumerIds.Add(userSeed * 2);
            chnl_3.ScrumerIds.Add(userSeed);

            Assert.AreEqual(chnl_3.GetUnupdatedScrumers().Count, 2);
            Assert.AreEqual(chnl_2.GetUnupdatedScrumers().Count, 0);
            Assert.AreEqual(chnl_1.GetUnupdatedScrumers().Count, 3);



        }
    }


}
