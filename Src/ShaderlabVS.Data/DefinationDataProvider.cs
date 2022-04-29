using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ShaderlabVS.Data
{
    public class DefinationDataProvider<T> where T : ModelBase
    {
        public static List<T> ProvideFromFile(string defFileName)
        {
            DefinationReader dr = new DefinationReader(defFileName);
            dr.Read();
            List<T> list = new List<T>();
            Type t = typeof(T);

            if (t is null)
            {
                throw new TypeLoadException($"Cannot find Type {t}");
            }

            foreach (Dictionary<string, string> section in dr.Sections)
            {
                if (!(t.Assembly.CreateInstance(t.ToString()) is T tInstance))
                {
                    throw new TypeLoadException($"Create Type {t} failed");
                }

                // Set the property value to the instance of T
                foreach (PropertyInfo property in t.GetProperties())
                {
                    // Get DefinationKey attribute
                    if (property.GetCustomAttributes(typeof(DefinationKeyAttribute)).FirstOrDefault() is DefinationKeyAttribute dkattr)
                    {
                        // get value in 
                        //
                        if (section.ContainsKey(dkattr.Name))
                        {
                            string value = section[dkattr.Name];
                            property.SetValue(tInstance, value);
                        }
                    }
                }

                ModelBase mb = tInstance;
                mb.PrepareProperties();

                list.Add(tInstance);
            }

            return list;
        }
    }
}
