using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ShaderlabVS
{
    #region Format definition
    /// <summary>
    /// Text
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ShaderlabText)]
    [Name(Constants.ShaderlabText)]
    [UserVisible(true)] //this should be visible to the end user
    [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
    internal sealed class ShaderlabTextFormat : ClassificationFormatDefinition
    {
        /// <summary>
        /// Defines the visual format for the "Shaderlab-Text" classification type
        /// </summary>
        public ShaderlabTextFormat() => DisplayName = Constants.ShaderlabText;
    }

    /// <summary>
    /// Strings
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ShaderlabStrings)]
    [Name(Constants.ShaderlabStrings)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class ShaderlabStrings : ClassificationFormatDefinition
    {
        /// <summary>
        /// Defines the visual format for the "Shaderlab-Text" classification type
        /// </summary>
        public ShaderlabStrings()
        {
            DisplayName = Constants.ShaderlabStrings;
            ForegroundColor = Colors.DarkRed;
        }
    }

    /// <summary>
    /// Functions
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ShaderlabFunction)]
    [Name(Constants.ShaderlabFunction)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class ShaderlabFunctionFormat : ClassificationFormatDefinition
    {
        public ShaderlabFunctionFormat()
        {
            DisplayName = Constants.ShaderlabFunction;
            ForegroundColor = Colors.DarkMagenta;
        }
    }

    /// <summary>
    /// Comments
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ShaderlabComment)]
    [Name(Constants.ShaderlabComment)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class ShaderlabComment : ClassificationFormatDefinition
    {
        public ShaderlabComment()
        {
            DisplayName = Constants.ShaderlabComment;
            ForegroundColor = Colors.ForestGreen;
        }
    }

    /// <summary>
    /// keywords
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ShaderlabHLSLCGKeyword)]
    [Name(Constants.ShaderlabHLSLCGKeyword)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class ShaderlabKeyword : ClassificationFormatDefinition
    {
        public ShaderlabKeyword()
        {
            DisplayName = Constants.ShaderlabHLSLCGKeyword;
            ForegroundColor = Colors.Blue;
        }
    }

    /// <summary>
    /// Unity3D keywords
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ShaderlabUnityKeywords)]
    [Name(Constants.ShaderlabUnityKeywords)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class ShaderlabUnityBlockKeywords : ClassificationFormatDefinition
    {
        public ShaderlabUnityBlockKeywords()
        {
            DisplayName = Constants.ShaderlabUnityKeywords;
            ForegroundColor = Colors.Blue;
        }
    }

    /// <summary>
    /// Unity3D keywords begin with #
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ShaderlabUnityKeywordsPara)]
    [Name(Constants.ShaderlabUnityKeywordsPara)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class ShaderlabUnityKeywordsPara : ClassificationFormatDefinition
    {
        public ShaderlabUnityKeywordsPara()
        {
            DisplayName = Constants.ShaderlabUnityKeywordsPara;
            ForegroundColor = Colors.DarkMagenta;
        }
    }

    /// <summary>
    /// Data type
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ShaderlabDataType)]
    [Name(Constants.ShaderlabDataType)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class ShaderlabDataType : ClassificationFormatDefinition
    {
        public ShaderlabDataType()
        {
            DisplayName = Constants.ShaderlabDataType;
            ForegroundColor = Colors.Blue;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.ShaderlabBracesMarker)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class BraceMarkerDefination : MarkerFormatDefinition
    {
        public BraceMarkerDefination()
        {
            DisplayName = Constants.ShaderlabBracesMarker;
            BackgroundColor = Colors.Gray;
        }
    }
    #endregion //Format definition
}
