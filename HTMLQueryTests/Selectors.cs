using HTMLQuery;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace HTMLQueryTests
{
    [TestFixture]
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

        [Test]
        public void IdSelect()
        {
            Assert.AreEqual(1, _query.Select("#anId").Count());
        }

        [Test]
        public void IdSelectFail()
        {
            Assert.AreEqual(0, _query.Select("#aClass").Count());
        }

        [Test]
        public void ChildIdSelect()
        {
            Assert.AreEqual(1, _query.Select("#anotherId").Count());
        }

        [Test]
        public void ChildIdSelectFail()
        {
            Assert.AreEqual(0, _query.Select("#anotherId", false).Count());
        }

        /// <summary>
        /// Class Selection tests
        /// </summary>

        [Test]
        public void ClassSelect()
        {
            Assert.AreEqual(1, _query.Select(".aClass").Count());
        }

        [Test]
        public void ClassSelectFail()
        {
            Assert.AreEqual(0, _query.Select(".inId").Count());
        }


        /// <summary>
        /// Element name Selection tests
        /// </summary>

        [Test]
        public void ElementNameSelect()
        {
            Assert.AreEqual(1, _query.Select("body").Count());
        }

        [Test]
        public void MultipleElementNameSelect()
        {
            Assert.AreEqual(3, _query.Select("div").Count());
        }

        [Test]
        public void ElementNameSelectFail()
        {
            Assert.AreEqual(0, _query.Select("invalid").Count());
        }


        /// <summary>
        /// Element property Selection tests
        /// </summary>


        [Test]
        public void ElementPropertySelect()
        {
            Assert.AreEqual("Option 2", _query.Select("[selected]selected").First().InnerText());
        }

        [Test]
        public void ElementPropertySelectFail()
        {
            Assert.AreEqual(0, _query.Select("[selected]invalid").Count());
        }

        [Test]
        public void ToplevelFlatten()
        {
            Assert.AreEqual(string.Empty, _query.Select("html").First().Flatten().InnerHtml().Source.Trim());
        }

    }
}
