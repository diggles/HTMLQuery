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
        /// Inner Text
        /// </summary>

        [Test]
        public void InnerText()
        {
            Assert.AreEqual("Copyright", _query.Select("#footer").First().InnerText());
        }

        [Test]
        public void InnerHtml()
        {
            Assert.AreEqual("<i>Copyright</i>", _query.Select("#footer").First().InnerHtml().Source);
        }


    }
}