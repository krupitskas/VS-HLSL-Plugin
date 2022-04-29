using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ShaderlabVS.Data
{
    /// <summary>
    /// Class use to read .def file
    /// 
    /// The .def syntax:
    ///     * Pair : 
    ///         - Description: Pair is something like KeyValuePair in C#, it has key and value defination.
    ///         - Format:  %$ (Key) = {(Value)}$%
    ///             - (Key) and (Value) are placeholders.
    ///             - Pair begin mark is "%$"
    ///             - Pair end mark is "$%"
    ///             - (Value) is in brace.
    ///             
    ///     * Section :
    ///         - Description: Section like a Dictionary in C#, it can be empty or has single or multiple Pair
    ///         - Format:  {% [Paris..] %}
    ///             - [Paris...] is placeholder for Pair
    ///             - Section begin mark is "{%"
    ///             - Section end mark is "%}"
    ///             - In same section, Pair with same key is not allowed
    ///             - A .def file can contains as many sections as you want
    ///             
    ///     * Comments :
    ///         - Description: lines are started with "#"
    /// 
    ///     * Escape Chars: 
    ///         chars  =>  escaped chars
    ///          {     =>   \{
    ///          }     =>   \}
    ///          $     =>   \$
    ///          =     =>   \=
    ///          %     =>   \%
    ///          #     =>   \#
    /// 
    /// </summary>
    public class DefinationReader
    {
        /// <summary>
        /// Gets the Sections defined in the .def file 
        /// </summary>
        public List<Dictionary<string, string>> Sections { get; }

        private readonly string _defFileName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defFileName">The .def file name</param>
        public DefinationReader(string defFileName)
        {
            Sections = new List<Dictionary<string, string>>();
            _defFileName = defFileName;
        }

        /// <summary>
        /// Read data from file
        /// </summary>
        public void Read()
        {
            if (!File.Exists(_defFileName))
            {
                throw new FileNotFoundException($"{_defFileName} is not founded");
            }

            string content = RemoveAllCommentsLines(File.ReadAllLines(_defFileName)).Trim();

            foreach (Match match in Regex.Matches(content, @"\{%[\s\S]*?%\}"))
            {
                Sections.Add(ParseSectionFromeText(match.ToString()));
            }
        }

        private string RemoveAllCommentsLines(string[] lines)
        {
            StringBuilder newContent = new StringBuilder();

            foreach (string line in lines)
            {
                if (line.Trim().StartsWith("#"))
                {
                    continue;
                }

                newContent.AppendLine(line);
            }

            return newContent.ToString();
        }

        private Dictionary<string, string> ParseSectionFromeText(string sectionText)
        {
            Dictionary<string, string> sectionDict = new Dictionary<string, string>();

            foreach (Match match in Regex.Matches(sectionText, @"%\$(?<key>[\s\S]+?)=\s*?\{(?<value>[\s\S]*?)\}\s*?\$%"))
            {
                string key = match.Groups["key"].Value.ToString().Trim();
                string value = match.Groups["value"].Value.ToString().Trim();

                if (!string.IsNullOrEmpty(key))
                {
                    sectionDict.Add(Escape(key), Escape(value));
                }
            }

            return sectionDict;
        }

        private string Escape(string input)
        {
            return input
                .Replace(@"\#", "#")
                .Replace(@"\{", "{")
                .Replace(@"\}", "}")
                .Replace(@"\$", "$")
                .Replace(@"\=", "=")
                .Replace(@"\%", "%");
        }
    }
}
