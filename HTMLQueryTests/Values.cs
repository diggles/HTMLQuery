using HTMLQuery;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace HTMLQueryTests
{
    [TestFixture]
    public class Values
    {
        private Query _query;

        public Values()
        {
            Reset();
        }

        private void Reset()
        {
            _query = new Query(File.ReadAllText("Values.html"));
        }

        /// <summary>
        /// ID Selection tests
        /// </summary>

        [Test]
        public void IdSelect()
        {
            Assert.AreEqual(1, _query.Select("#anId").Count());
        }
    }
}