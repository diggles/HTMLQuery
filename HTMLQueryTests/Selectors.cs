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
        private Query _query;

        public Selectors()
        {
            Reset();
        }

        private void Reset()
        {
            _query = new Query(File.ReadAllText("Selectors.html"));
        }

        /// <summary>
        /// ID Selection tests
        /// </summary>

        [TestMethod]
        public void IdSelect()
        {
            Assert.AreEqual(1, _query.Select("#anId").Count());
        }

        [TestMethod]
        public void IdSelectFail()
        {
            Assert.AreEqual(0, _query.Select("#aClass").Count());
        }

        [TestMethod]
        public void ChildIdSelect()
        {
            Assert.AreEqual(1, _query.Select("#anotherId").Count());
        }

        [TestMethod]
        public void ChildIdSelectFail()
        {
            Assert.AreEqual(0, _query.Select("#anotherId", false).Count());
        }

        /// <summary>
        /// Class Selection tests
        /// </summary>

        [TestMethod]
        public void ClassSelect()
        {
            Assert.AreEqual(1, _query.Select(".aClass").Count());
        }

        [TestMethod]
        public void ClassSelectFail()
        {
            Assert.AreEqual(0, _query.Select(".inId").Count());
        }


        /// <summary>
        /// Element name Selection tests
        /// </summary>

        [TestMethod]
        public void ElementNameSelect()
        {
            Assert.AreEqual(1, _query.Select("body").Count());
        }

        [TestMethod]
        public void MultipleElementNameSelect()
        {
            Assert.AreEqual(3, _query.Select("div").Count());
        }

        [TestMethod]
        public void ElementNameSelectFail()
        {
            Assert.AreEqual(0, _query.Select("invalid").Count());
        }


        /// <summary>
        /// Element property Selection tests
        /// </summary>


        [TestMethod]
        public void ElementPropertySelect()
        {
            Assert.AreEqual("Option 2", _query.Select("[selected]selected").First().InnerText());
        }

        [TestMethod]
        public void ElementPropertySelectFail()
        {
            Assert.AreEqual(0, _query.Select("[selected]invalid").Count());
        }

        [TestMethod]
        public void ToplevelFlatten()
        {
            Assert.AreEqual(string.Empty, _query.Select("html").First().Flatten().InnerHtml().Source.Trim());
        }

    }
}
