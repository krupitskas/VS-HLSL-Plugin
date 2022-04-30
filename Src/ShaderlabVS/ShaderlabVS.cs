using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderlabVS.Lexer;

namespace ShaderlabVS
{
    #region Provider definition

    /// <summary>
    /// Apply settings for all .shader files
    /// </summary>
    [Export(typeof(ITaggerProvider))]
    [ContentType(Constants.ContentType)]
    [TagType(typeof(ClassificationTag))]
    internal sealed class ShaderlabVSClassifierProvider : ITaggerProvider
    {
        [Export]
        [Name(Constants.ContentType)]
        [BaseDefinition(Constants.BaseDefinition)]
        public static ContentTypeDefinition ShaderlabContentType = null;

        [Export]
        [FileExtension(Constants.ShaderFileNameExt)]
        [ContentType(Constants.ContentType)]
        public static FileExtensionToContentTypeDefinition ShaderlabFileType = null;

        [Export]
        [FileExtension(Constants.ComputeShaderFileNameExt)]
        [ContentType(Constants.ContentType)]
        public static FileExtensionToContentTypeDefinition ComputeShaderFileType = null;

        [Export]
        [FileExtension(Constants.CGIncludeFileExt)]
        [ContentType(Constants.ContentType)]
        public static FileExtensionToContentTypeDefinition CgIncludeFileType = null;

        [Export]
        [FileExtension(Constants.GLSLIncludeFileExt)]
        [ContentType(Constants.ContentType)]
        public static FileExtensionToContentTypeDefinition GLSLIncludeFileType = null;

        [Export]
        [FileExtension(Constants.CGFile)]
        [ContentType(Constants.ContentType)]
        public static FileExtensionToContentTypeDefinition cgFileType = null;

        [Export]
        [FileExtension(Constants.HLSLFile)]
        [ContentType(Constants.ContentType)]
        public static FileExtensionToContentTypeDefinition hlslFileType = null;

        [Import]
        internal IClassificationTypeRegistryService classificationTypeRegistry = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new ShaderlabClassifier(buffer, classificationTypeRegistry) as ITagger<T>;
        }
    }
    #endregion //provider def

    #region Classifier
    internal sealed class ShaderlabClassifier : ITagger<ClassificationTag>
    {
        static ShaderlabClassifier() => Scanner.LoadTableDataFromLex();

        private readonly Dictionary<ShaderlabToken, IClassificationType> _classTypeDict;
        private readonly Scanner _scanner;
        private readonly ITextBuffer _textBuffer;

        public ShaderlabClassifier(ITextBuffer buffer, IClassificationTypeRegistryService registerService)
        {
            _textBuffer = buffer;
            _scanner = new Scanner();

            _classTypeDict = new Dictionary<ShaderlabToken, IClassificationType>
            {
                { ShaderlabToken.TEXT, registerService.GetClassificationType(Constants.ShaderlabText) },
                { ShaderlabToken.COMMENT, registerService.GetClassificationType(Constants.ShaderlabComment) },
                { ShaderlabToken.HLSLCGDATATYPE, registerService.GetClassificationType(Constants.ShaderlabDataType) },
                { ShaderlabToken.HLSLCGFUNCTION, registerService.GetClassificationType(Constants.ShaderlabFunction) },
                { ShaderlabToken.HLSLCGKEYWORD, registerService.GetClassificationType(Constants.ShaderlabHLSLCGKeyword) },
                { ShaderlabToken.HLSLCGKEYWORDSPECIAL, registerService.GetClassificationType(Constants.ShaderlabHLSLCGKeyword) },
                { ShaderlabToken.UNITYKEYWORD, registerService.GetClassificationType(Constants.ShaderlabUnityKeywords) },
                { ShaderlabToken.UNITYKEYWORD_PARA, registerService.GetClassificationType(Constants.ShaderlabUnityKeywordsPara) },
                { ShaderlabToken.UNITYDATATYPE, registerService.GetClassificationType(Constants.ShaderlabDataType) },
                { ShaderlabToken.UNITYFUNCTION, registerService.GetClassificationType(Constants.ShaderlabFunction) },
                { ShaderlabToken.STRING_LITERAL, registerService.GetClassificationType(Constants.ShaderlabStrings) },
                { ShaderlabToken.UNITYVALUES, registerService.GetClassificationType(Constants.ShaderlabUnityKeywords) },
                { ShaderlabToken.UNDEFINED, registerService.GetClassificationType(Constants.ShaderlabText) }
            };
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            ShaderlabCompletionSource.ClearWordsInDocuments();
            ShaderlabCompletionSource.SetWordsInDocuments(spans[0].Snapshot.GetText());

            string text = " " + spans[0].Snapshot.GetText().ToLower();
            _scanner.SetSource(text, 0);
            int token;

            do
            {
                token = _scanner.NextToken();
                int pos = _scanner.GetPos();
                int length = _scanner.GetLength();

                if (pos < 0 || length < 0 || pos > text.Length)
                {
                    continue;
                }

                if (pos + length > text.Length)
                {
                    length = text.Length - pos;
                }

                if (_classTypeDict.TryGetValue((ShaderlabToken)token, out IClassificationType cf))
                {
                    switch ((ShaderlabToken)token)
                    {
                        case ShaderlabToken.HLSLCGKEYWORD:
                        case ShaderlabToken.UNITYKEYWORD:
                        case ShaderlabToken.UNITYKEYWORD_PARA:
                        case ShaderlabToken.HLSLCGDATATYPE:
                        case ShaderlabToken.HLSLCGFUNCTION:
                        case ShaderlabToken.UNITYFUNCTION:
                        case ShaderlabToken.UNITYMACROS:
                        case ShaderlabToken.UNITYDATATYPE:
                        case ShaderlabToken.UNITYVALUES:
                            length -= 2;
                            _scanner.PushbackText(length + 1);
                            break;
                        case ShaderlabToken.HLSLCGKEYWORDSPECIAL:
                            --pos;
                            --length;
                            _scanner.PushbackText(length);
                            break;
                        case ShaderlabToken.STRING_LITERAL:
                        case ShaderlabToken.COMMENT:
                            --pos;
                            break;
                    }

                    if (pos < 0 || length < 0 || pos > text.Length)
                    {
                        continue;
                    }

                    yield return new TagSpan<ClassificationTag>(new SnapshotSpan(spans[0].Snapshot, new Span(pos, length)), new ClassificationTag(cf));

                }

            }
            while (token > (int)Tokens.EOF);

        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }
    }
    #endregion //Classifier
}
