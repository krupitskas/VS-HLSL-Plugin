using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaderlabVS.Data;

#region Common
public class ModelBase
{
    /// <summary>
    /// Do something to modify the property values
    /// </summary>
    public virtual void PrepareProperties()
    {
    }
}

public class FunctionBase : ModelBase
{
    [DefinitionKey("Name")]
    public string Name { get; set; }

    [DefinitionKey("Synopsis")]
    public string RawSynopsisData { get; set; }

    public List<string> Synopsis { get; set; }

    [DefinitionKey("Description")]
    public string Description { get; set; }

    public FunctionBase()
    {
        Name = string.Empty;
        RawSynopsisData = string.Empty;
        Synopsis = new List<string>();
        Description = string.Empty;
    }

    public override void PrepareProperties()
    {
        base.PrepareProperties();

        if (!string.IsNullOrEmpty(RawSynopsisData.Trim()))
        {
            Synopsis.Clear();
            Synopsis.AddRange(RawSynopsisData.Trim().Split(new char[] { ';', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList());
            Synopsis.ForEach(static s => s = s.Trim());
        }
    }
}

#endregion

#region HLSL/CG
public class HLSLCGFunction : FunctionBase
{
}

public class HLSLCGKeywords : ModelBase
{
    [DefinitionKey("Type")]
    public string Type { get; set; }

    [DefinitionKey("AllKeywords")]
    public string RawKeywordsData { get; set; }

    public List<string> Keywords { get; set; }

    public HLSLCGKeywords()
    {
        Type = string.Empty;
        RawKeywordsData = string.Empty;
        Keywords = new List<string>();
    }

    public override void PrepareProperties()
    {
        base.PrepareProperties();
        Keywords.Clear();
        HashSet<string> keywordSet = new();
        List<string> kwlist = RawKeywordsData.Split(new char[] { ',', ';', '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        kwlist.ForEach(s => keywordSet.Add(s));
        Keywords.AddRange(keywordSet.ToList());
        Keywords.ForEach(static s => s = s.Trim());
    }
}

public class HLSLCGDataTypes : ModelBase
{
    [DefinitionKey("Alltypes")]
    public string RawDataTypeData { get; set; }

    public List<string> DataTypes { get; set; }

    public HLSLCGDataTypes()
    {
        RawDataTypeData = string.Empty;
        DataTypes = new List<string>();
    }

    public override void PrepareProperties()
    {
        base.PrepareProperties();
        DataTypes.Clear();
        HashSet<string> datatypes = new();
        List<string> list = RawDataTypeData.Split(new char[] { ',', ';', '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        list.ForEach(s => datatypes.Add(s));
        DataTypes.AddRange(datatypes.ToList());
        DataTypes.ForEach(static s => s = s.Trim());
    }
}

#endregion

#region Unity3D
public class UnityBuiltinValue : ModelBase
{
    [DefinitionKey("Name")]
    public string Name { get; set; }

    [DefinitionKey("Type")]
    public string Type { get; set; }

    [DefinitionKey("Value")]
    public string VauleDescription { get; set; }

    public UnityBuiltinValue()
    {
        Name = string.Empty;
        Type = string.Empty;
        VauleDescription = string.Empty;
    }
}

public class UnityBuiltinFunction : FunctionBase
{
}

public class UnityBuiltinMacros : FunctionBase
{
}

public class UnityKeywords : ModelBase
{
    [DefinitionKey("Name")]
    public string Name { get; set; }

    [DefinitionKey("Description")]
    public string Description { get; set; }

    [DefinitionKey("Format")]
    public string Format { get; set; }

    public UnityKeywords()
    {
        Description = string.Empty;
        Format = string.Empty;
    }
}

public class UnityBuiltinDatatype : ModelBase
{
    [DefinitionKey("Name")]
    public string Name { get; set; }

    [DefinitionKey("Description")]
    public string Description { get; set; }

    public UnityBuiltinDatatype()
    {
        Name = string.Empty;
        Description = string.Empty;
    }
}
#endregion
