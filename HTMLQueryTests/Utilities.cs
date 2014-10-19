using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Text.RegularExpressions;
using HTMLQuery;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace HTMLQueryTests
{   
    [TestFixture]
    public class Utilities
    {
        private readonly Query _query;

        public Utilities(){
            _query = new Query(File.ReadAllText("Markup.html"));
        }

        [Test]
        public void BetweenParse()
        {
            Assert.AreEqual("bg.jpg", _query.Select("#wrapper").First().Value("style").ToString().Between('(', ')'));
        }

        [Test]
        public void ToplevelFlatten()
        {
            Assert.AreEqual(string.Empty, _query.Select("html").First().Flatten().InnerHtml().Source.Trim());
        }

        [Test]
        public void FlattenPass()
        {
            Query flat = _query.Select("p").First().InnerHtml();
            Assert.AreEqual(flat.ToString(), flat.Flatten().ToString());
        }

        [Test]
        public void StripHtml()
        {
            Assert.AreEqual("Content", _query.Select("p").First().StripHtml().Trim());
        }




    }
}