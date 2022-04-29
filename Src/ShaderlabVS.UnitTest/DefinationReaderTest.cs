using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaderlabVS.Data;

namespace ShaderlabVS.UnitTest
{
    [TestClass]
    public class DefinationReaderTest
    {
        [TestMethod]
        public void ParseTest()
        {
            DefinationReader dr = new DefinationReader(@"Data\test.def");
            dr.Read();
            StringBuilder sb = new StringBuilder();

            foreach (System.Collections.Generic.Dictionary<string, string> section in dr.Sections)
            {
                Console.WriteLine("-----------------Sections--------------------");
                sb.AppendLine("-----------------Sections--------------------");

                foreach (System.Collections.Generic.KeyValuePair<string, string> pair in section)
                {
                    Console.WriteLine("({0})=({1})", pair.Key, pair.Value);
                    sb.AppendLine($"({pair.Key})=({pair.Value})");
                }
            }

            string result = sb.ToString();
            Assert.IsTrue(result.Contains(@"(Name)=(abs)"));
            Assert.IsTrue(result.Contains(@"(Synopsis)="));
            Assert.IsTrue(result.Contains(@"(returns absolute value of scalars and vectors.)"));
            Assert.IsTrue(result.Contains(@"(Escape#Chars)=(\#=>#, \$=>$, \{=>{, \}=}, \==>=, \%=>%)"));
        }
    }
}
