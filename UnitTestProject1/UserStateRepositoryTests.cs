using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Beta.Repository;

namespace BetaTests.Repositories
{
    [TestClass]
    public class UserStateRepositoryTests
    {
        [TestMethod]
        public void KappaViolationExpiresTime()
        {
            UserState user = new UserState();

            user.KappaViolations.Add(new KappaViolation()
            {
                VioltionDateTime = DateTime.Now.AddMinutes(61)
            });
            user.EvaluateKappaViolations();            
            Assert.AreEqual(user.KappaViolations.Count,0);            
        }
    }
}
