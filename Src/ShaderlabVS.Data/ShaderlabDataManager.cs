using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ShaderlabVS.Data;

/// <summary>
/// 
/// </summary>
public class ShaderlabDataManager
{
    #region Constants
    public const string HLSL_CG_DATATYPE_DEFINITIONFILE = @"Data\HLSL_CG_datatype.def";
    public const string HLSL_CG_FUNCTION_DEFINITIONFILE = @"Data\HLSL_CG_functions.def";
    public const string HLSL_CG_KEYWORD_DEFINITIONFILE = @"Data\HLSL_CG_Keywords.def";

    public const string UNITY3D_DATATYPE_DEFINITIONFILE = @"Data\Unity3D_datatype.def";
    public const string UNITY3D_FUNCTION_DEFINITIONFILE = @"Data\Unity3D_functions.def";
    public const string UNITY3D_KEYWORD_DEFINITIONFILE = @"Data\Unity3D_keywords.def";
    public const string UNITY3D_MACROS_DEFINITIONFILE = @"Data\Unity3D_macros.def";
    public const string UNITY3D_VALUES_DEFINITIONFILE = @"Data\Unity3D_values.def";
    #endregion

    #region Properties
    public List<HLSLCGFunction> HLSLCGFunctions { get; private set; }
    public List<string> HLSLCGBlockKeywords { get; private set; }
    public List<string> HLSLCGNonblockKeywords { get; private set; }
    public List<string> HLSLCGSpecialKeywords { get; private set; }
    public List<string> HLSLCGDatatypes { get; private set; }

    public List<UnityBuiltinDatatype> UnityBuiltinDatatypes { get; private set; }
    public List<UnityBuiltinFunction> UnityBuiltinFunctions { get; private set; }
    public List<UnityBuiltinMacros> UnityBuiltinMacros { get; private set; }
    public List<UnityKeywords> UnityKeywords { get; private set; }
    public List<UnityBuiltinValue> UnityBuiltinValues { get; private set; }
    #endregion

    #region Singleton
    private static readonly object s_lockObj = new();

    private static ShaderlabDataManager s_instance;

    public static ShaderlabDataManager Instance
    {
        get
        {
            if (s_instance is null)
            {
                lock (s_lockObj)
                {
                    s_instance ??= new ShaderlabDataManager();
                }
            }

            return s_instance;
        }
    }
    #endregion

    private ShaderlabDataManager()
    {
        string currentAssemblyDir = new FileInfo(Assembly.GetExecutingAssembly().CodeBase.Substring(8)).DirectoryName;
        HLSLCGFunctions = DefinitionDataProvider<HLSLCGFunction>.ProvideFromFile(Path.Combine(currentAssemblyDir, HLSL_CG_FUNCTION_DEFINITIONFILE));

        List<HLSLCGKeywords> hlslcgKeywords = DefinitionDataProvider<HLSLCGKeywords>.ProvideFromFile(Path.Combine(currentAssemblyDir, HLSL_CG_KEYWORD_DEFINITIONFILE));
        HLSLCGBlockKeywords = GetHLSLCGKeywordsByType(hlslcgKeywords, "block");
        HLSLCGNonblockKeywords = GetHLSLCGKeywordsByType(hlslcgKeywords, "nonblock");
        HLSLCGSpecialKeywords = GetHLSLCGKeywordsByType(hlslcgKeywords, "special");

        HLSLCGDataTypes dts = DefinitionDataProvider<HLSLCGDataTypes>.ProvideFromFile(Path.Combine(currentAssemblyDir, HLSL_CG_DATATYPE_DEFINITIONFILE)).First();

        if (dts is not null)
        {
            HLSLCGDatatypes = dts.DataTypes;
        }

        UnityBuiltinDatatypes = DefinitionDataProvider<UnityBuiltinDatatype>.ProvideFromFile(Path.Combine(currentAssemblyDir, UNITY3D_DATATYPE_DEFINITIONFILE));
        UnityBuiltinFunctions = DefinitionDataProvider<UnityBuiltinFunction>.ProvideFromFile(Path.Combine(currentAssemblyDir, UNITY3D_FUNCTION_DEFINITIONFILE));
        UnityBuiltinMacros = DefinitionDataProvider<UnityBuiltinMacros>.ProvideFromFile(Path.Combine(currentAssemblyDir, UNITY3D_MACROS_DEFINITIONFILE));
        UnityBuiltinValues = DefinitionDataProvider<UnityBuiltinValue>.ProvideFromFile(Path.Combine(currentAssemblyDir, UNITY3D_VALUES_DEFINITIONFILE));
        UnityKeywords = DefinitionDataProvider<UnityKeywords>.ProvideFromFile(Path.Combine(currentAssemblyDir, UNITY3D_KEYWORD_DEFINITIONFILE));

    }

    private List<string> GetHLSLCGKeywordsByType(List<HLSLCGKeywords> allTypes, string type)
    {
        HLSLCGKeywords kw = allTypes.First(k => k.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

        return kw is not null ? kw.Keywords : new List<string>();
    }
}
