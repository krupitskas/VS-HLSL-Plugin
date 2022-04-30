using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaderlabVS.Data;

namespace ShaderlabVS.UnitTest
{
    [TestClass]
    public class DataProviderTest
    {
        [TestMethod]
        public void TestUnityBuiltInValues()
        {
            string defFileName = @"Data\Unity3D_values.def";
            DefinitionReader dr = new DefinitionReader(defFileName);
            dr.Read();
            Console.WriteLine(dr.Sections.Count);

            List<UnityBuiltinValue> dataList = DefinitionDataProvider<UnityBuiltinValue>.ProvideFromFile(defFileName);
            Console.WriteLine(dataList.Count);

            foreach (UnityBuiltinValue val in dataList)
            {
                Console.WriteLine("-----------------Values--------------------");
                Console.WriteLine("Name={0}", val.Name);
                Console.WriteLine("Type={0}", val.Type);
                Console.WriteLine("Description={0}", val.VauleDescription);
            }

        }

        [TestMethod]
        public void TestHLSLCGFunctions()
        {
            string file = "Data\\HLSL_CG_functions.def";
            List<HLSLCGFunction> funList = DefinitionDataProvider<HLSLCGFunction>.ProvideFromFile(file);

            foreach (HLSLCGFunction fun in funList)
            {
                Console.WriteLine("-----------------Function--------------------");
                Console.WriteLine("Name={0}", fun.Name);
                Console.WriteLine("Synopsis count={0}", fun.Synopsis.Count);
                Console.WriteLine("Synopsis = {0}", string.Join("\n", fun.Synopsis));
                Console.WriteLine("Description = {0}", string.Join("\n", fun.Description));
            }
        }

        [TestMethod]
        public void TestHLSLKeywords()
        {
            string file = "Data\\HLSL_CG_Keywords.def";
            List<HLSLCGKeywords> funList = DefinitionDataProvider<HLSLCGKeywords>.ProvideFromFile(file);

            foreach (HLSLCGKeywords fun in funList)
            {
                Console.WriteLine("-----------------Function--------------------");
                Console.WriteLine("Name={0}", fun.Type);
                Console.WriteLine("Keywords = {0}", string.Join("\n", fun.Keywords));
            }
        }

        [TestMethod]
        public void TestHLSLDatatype()
        {
            string file = "Data\\HLSL_CG_datatype.def";
            List<HLSLCGDataTypes> list = DefinitionDataProvider<HLSLCGDataTypes>.ProvideFromFile(file);

            foreach (HLSLCGDataTypes dt in list)
            {
                Console.WriteLine("-----------------datatype--------------------");
                Console.WriteLine("datatype = {0}", string.Join("\n", dt.DataTypes));
            }
        }

        [TestMethod]
        public void TestUnitydatatype()
        {
            string file = "Data\\Unity3D_datatype.def";
            List<UnityBuiltinDatatype> list = DefinitionDataProvider<UnityBuiltinDatatype>.ProvideFromFile(file);

            foreach (UnityBuiltinDatatype item in list)
            {
                Console.WriteLine("-----------------datatype--------------------");
                Console.WriteLine("Name = {0}", string.Join("\n", item.Name));
                Console.WriteLine("Description = {0}", string.Join("\n", item.Description));
            }
        }

        [TestMethod]
        public void TestUnityFunctions()
        {
            string file = "Data\\Unity3D_functions.def";
            List<UnityBuiltinFunction> funList = DefinitionDataProvider<UnityBuiltinFunction>.ProvideFromFile(file);

            foreach (UnityBuiltinFunction fun in funList)
            {
                Console.WriteLine("-----------------Function--------------------");
                Console.WriteLine("Name={0}", fun.Name);
                Console.WriteLine("Synopsis count={0}", fun.Synopsis.Count);
                Console.WriteLine("Synopsis = {0}", string.Join("\n", fun.Synopsis));
                Console.WriteLine("Description = {0}", string.Join("\n", fun.Description));
            }
        }

        [TestMethod]
        public void TestUnityMacros()
        {
            string file = "Data\\Unity3D_macros.def";
            List<UnityBuiltinMacros> list = DefinitionDataProvider<UnityBuiltinMacros>.ProvideFromFile(file);

            foreach (UnityBuiltinMacros item in list)
            {
                Console.WriteLine("-----------------Macros--------------------");
                Console.WriteLine("Name={0}", item.Name);
                Console.WriteLine("Synopsis count={0}", item.Synopsis.Count);
                Console.WriteLine("Synopsis = {0}", string.Join("\n", item.Synopsis));
                Console.WriteLine("Description = {0}", string.Join("\n", item.Description));
            }
        }

        [TestMethod]
        public void TestDataManger() => Console.WriteLine(string.Join("\r", ShaderlabDataManager.Instance.HLSLCGBlockKeywords));
    }
}
