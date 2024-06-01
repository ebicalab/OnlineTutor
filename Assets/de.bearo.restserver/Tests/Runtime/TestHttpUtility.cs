using NUnit.Framework;
using RestServer.NetCoreServer;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestHttpUtility {
        [Test]
        public void Test_GoodCase() {
            const string url = "?a=b&c=d";

            var qs = HttpUtility.ParseAndFilter(url);

            Assert.AreEqual(2, qs.Count);
            Assert.AreEqual("b", qs["a"]);
            Assert.AreEqual("d", qs["c"]);
        }

        [Test]
        public void Test_GoodCase2() {
            const string url = "?a=b&a=c";

            var qs = HttpUtility.ParseAndFilter(url);

            Assert.AreEqual(1, qs.Count);
            Assert.IsNotNull(qs.GetValues("a"));
            Assert.AreEqual(2, qs.GetValues("a").Length);
            Assert.AreEqual("b", qs.GetValues("a")[0]);
            Assert.AreEqual("c", qs.GetValues("a")[1]);
        }

        [Test]
        public void Test_MissingValue() {
            const string url = "a&c=";

            var qs = HttpUtility.ParseAndFilter(url);

            Assert.AreEqual(2, qs.Count);
            Assert.AreEqual(null, qs["a"]);
            Assert.AreEqual(string.Empty, qs["c"]);
        }

        [Test]
        public void Test_MissingValue2() {
            const string url = "?a";

            var qs = HttpUtility.ParseAndFilter(url);

            Assert.AreEqual(1, qs.Count);
            Assert.AreEqual(null, qs["a"]);
        }
    }
}