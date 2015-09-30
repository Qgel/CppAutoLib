using CppAutoLib;
using NUnit.Framework;

namespace CppAutoLibTest
{
    [TestFixture]
    class LibArchiveTest 
    {
        [Test]
        public void TestFTDILib()
        {
            string path = "ftd2xx.lib";
            LibArchive lib = new LibArchive(path);
            Assert.IsTrue(lib.IsValid);
            Assert.AreEqual(lib.MangledNames.Count, 185);
        }
    }
}
