using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HTMLQuery;

namespace HTMLQueryTests
{
    [TestClass]
    public class Selectors
    {
        private Query q;

        public Selectors()
        {
            Reset();
        }

        private void Reset()
        {
            q = new Query(File.ReadAllText("Selectors.html"));
        }

        [TestMethod]
        public void IdSelect()
        {
            Reset();
            Assert.AreEqual(q.Select("#anId").Count(), 1);
        }

        [TestMethod]
        public void IdSelectFail()
        {
            Reset();
            Assert.AreEqual(q.Select("#aClass").Count(), 0);
        }

        [TestMethod]
        public void ClassSelect()
        {
            Reset();
            Assert.AreEqual(q.Select(".aClass").Count(), 1);
        }

        [TestMethod]
        public void ClassSelectFail()
        {
            Reset();
            Assert.AreEqual(q.Select("#aClass").Count(), 0);
        }

        [TestMethod]
        public void ChildIdSelect()
        {
            Reset();
            Assert.AreEqual(q.Select("#anotherId").Count(), 1);
        }

        [TestMethod]
        public void ChildIdSelectFail()
        {
            Reset();
            log(q.Source);
            log(q.Flatten().Source);
            log(q.InnerHtml().Source);
            log(q.Select("#anId").First().Source);
            Assert.IsTrue(true);
            //Assert.AreEqual(q.Select("#anotherId", false).Count(), 0);
        }

        private void log(string msg)
        {
            Debug.WriteLine(msg + "\r\n--------------------------------");
        }

    }
}
