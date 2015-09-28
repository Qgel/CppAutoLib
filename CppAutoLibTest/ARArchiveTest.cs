using CppAutoLib;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppAutoLibTest
{
    [TestFixture]
    class ARArchiveTest 
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
