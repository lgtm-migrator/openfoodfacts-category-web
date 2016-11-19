namespace OffLangParser.UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CultureTests
    {
        [TestMethod]
        public void TestDeGetsCorrectCulture()
        {
            var expected = "de";

            var culture = Culture.FromIsoName(expected);
            var actual = culture.Name;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestZzGetsDummyCulture()
        {
            var expectedName = "zz";

            var culture = Culture.FromIsoName(expectedName);

            Assert.IsNotNull(culture);
            Assert.AreEqual(expectedName, culture.Name);
            Assert.AreEqual("zz", culture.CompareInfo.Name);
        }
    }
}
