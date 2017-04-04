using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Beta.Repository;

namespace UnitTestProject2
{
    [TestClass]
    public class UserStateRepository
    {
        [TestMethod]
        public void KappaViolationEvaluationTime()
        {
            UserState user = new UserState();
            //user.AddKappaViolation();
            user.KappaViolations.Add(new KappaViolation()
            {
                VioltionDateTime = DateTime.Now.AddMinutes(-61)
            });

            user.EvaluateKappaViolations();

            Assert.AreEqual(user.KappaViolations.Count,0);
        }

        [TestMethod]
        public void KappaViolationEvaluationMessageCount()
        {
            UserState user = new UserState();
            //user.AddKappaViolation();
            user.KappaViolations.Add(new KappaViolation()
            {
                MessageCount = 201
            });

            user.EvaluateKappaViolations();

            Assert.AreEqual(user.KappaViolations.Count, 0);
        }
    }
}
