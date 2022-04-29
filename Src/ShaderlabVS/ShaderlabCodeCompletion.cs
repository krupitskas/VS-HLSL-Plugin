using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using ShaderlabVS.Data;

namespace ShaderlabVS
{
    #region Shaderlab Completion Source
    public class ShaderlabCompletionSource : ICompletionSource
    {
        private static readonly HashSet<string> s_wordsInDocuments = new HashSet<string>();
        private readonly ShaderlabCompletionSourceProvider _sourceProvider;
        private readonly ITextBuffer _textBuffer;
        private readonly ITextDocument _textDocument;
        private static readonly ImageSource s_functionsImage;
        private static readonly ImageSource s_datatypeImage;
        private static readonly ImageSource s_keywordsImage;
        private static readonly ImageSource s_valuesImage;

        static ShaderlabCompletionSource()
        {
            s_functionsImage = GetImageFromAssetByName("Method.png");
            s_datatypeImage = GetImageFromAssetByName("Structure.png");
            s_keywordsImage = GetImageFromAssetByName("Keywords.png");
            s_valuesImage = GetImageFromAssetByName("Values.png");
        }

        public ShaderlabCompletionSource(ShaderlabCompletionSourceProvider completonSourceProvider, ITextBuffer textBuffer, ITextDocument document)
        {
            _sourceProvider = completonSourceProvider;
            _textBuffer = textBuffer;
            _textDocument = document;
        }

        public static void SetWordsInDocuments(string text)
        {
            StringReader reader = new StringReader(text);

            string line = reader.ReadLine();

            while (line != null)
            {
                if (Utilities.IsCommentLine(line))
                {
                    line = reader.ReadLine();
                    continue;
                }

                string[] words = line.Split(
                    new char[] { '{', '}', ' ', '\t', '(', ')', '[', ']', '+', '-', '*', '/', '%', '^', '>', '<', ':',
                                '.', ';', '\"', '\'', '?', '\\', '&', '|', '`', '$', '#', ','},
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (string word in words)
                {
                    s_wordsInDocuments.Add(word);
                }

                line = reader.ReadLine();
            }
        }

        public static void ClearWordsInDocuments() => s_wordsInDocuments.Clear();

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession completionSession)
        {
            SnapshotPoint ssPoint = completionSession.TextView.Caret.Position.BufferPosition - 1;
            ITextStructureNavigator navigator = _sourceProvider.TextNavigatorService.GetTextStructureNavigator(_textBuffer);
            TextExtent textExtent = navigator.GetExtentOfWord(ssPoint);
            return ssPoint.Snapshot.CreateTrackingSpan(textExtent.Span, SpanTrackingMode.EdgeInclusive);
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            List<Completion> completionList = new List<Completion>();

            HashSet<string> keywords = new HashSet<string>();

            // Add functions into auto completion list
            ShaderlabDataManager.Instance.HLSLCGFunctions.ForEach(f =>
            {
                completionList.Add(new Completion(f.Name, f.Name, f.Description, s_functionsImage, null));
                keywords.Add(f.Name);
            });

            // Datatypes
            ShaderlabDataManager.Instance.HLSLCGDatatypes.ForEach(d =>
            {
                completionList.Add(new Completion(d, d, string.Empty, s_datatypeImage, null));
                keywords.Add(d);
            });

            // Keywords
            ShaderlabDataManager.Instance.HLSLCGBlockKeywords.ForEach(k =>
            {
                completionList.Add(new Completion(k, k, string.Empty, s_keywordsImage, null));
                keywords.Add(k);
            });

            ShaderlabDataManager.Instance.HLSLCGNonblockKeywords.ForEach(k =>
            {
                completionList.Add(new Completion(k, k, string.Empty, s_keywordsImage, null));
                keywords.Add(k);
            });

            ShaderlabDataManager.Instance.HLSLCGSpecialKeywords.ForEach(k =>
            {
                completionList.Add(new Completion(k, k, string.Empty, s_keywordsImage, null));
                keywords.Add(k);
            });

            if (_textDocument != null && !Utilities.IsInCGOrHLSLFile(_textDocument.FilePath))
            {
                // Unity data types
                ShaderlabDataManager.Instance.UnityBuiltinDatatypes.ForEach(d =>
                {
                    completionList.Add(new Completion(d.Name, d.Name, d.Description, s_datatypeImage, null));
                    keywords.Add(d.Name);
                });

                // Unity Functions
                ShaderlabDataManager.Instance.UnityBuiltinFunctions.ForEach(f =>
                {
                    completionList.Add(new Completion(f.Name, f.Name, f.Description, s_functionsImage, null));
                    keywords.Add(f.Name);
                });


                ShaderlabDataManager.Instance.UnityKeywords.ForEach(k =>
                {
                    completionList.Add(new Completion(k.Name, k.Name, k.Description, s_keywordsImage, null));
                    keywords.Add(k.Name);
                });

                // Unity values/enums
                ShaderlabDataManager.Instance.UnityBuiltinValues.ForEach(v =>
                {
                    completionList.Add(new Completion(v.Name, v.Name, v.VauleDescription, s_valuesImage, null));
                    keywords.Add(v.Name);
                });

                // Unity Macros
                ShaderlabDataManager.Instance.UnityBuiltinMacros.ForEach(m =>
                {
                    string description = $"{string.Join(";\n", m.Synopsis)}\n{m.Description}";

                    completionList.Add(m.Synopsis.Count > 0
                        ? new Completion(m.Name, m.Name, description, s_functionsImage, null)
                        : new Completion(m.Name, m.Name, description, s_valuesImage, null));

                    keywords.Add(m.Name);
                });
            }

            // Add words in current file
            foreach (string word in s_wordsInDocuments)
            {
                if (!keywords.Contains(word))
                {
                    completionList.Add(new Completion(word, word, string.Empty, s_valuesImage, null));
                }
            }

            completionSets.Add(new CompletionSet("Token", "Token", FindTokenSpanAtPosition(session.GetTriggerPoint(_textBuffer), session), completionList, null));
        }

        private static ImageSource GetImageFromAssetByName(string imageFileName)
        {
            string currentAssemblyDir = new FileInfo(Assembly.GetExecutingAssembly().CodeBase.Substring(8)).DirectoryName;
            return new BitmapImage(new Uri(Path.Combine(currentAssemblyDir, "Assets", imageFileName), UriKind.Absolute));
        }

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }
    #endregion

    #region Shaderlab Completion Source Provider
    [Export(typeof(ICompletionSourceProvider))]
    [Name("CompletionSourceProvider")]
    [ContentType(Constants.ContentType)]
    public class ShaderlabCompletionSourceProvider : ICompletionSourceProvider, IWpfTextViewCreationListener
    {

        [Import]
        public ITextStructureNavigatorSelectorService TextNavigatorService { get; set; }

        [Import]
        public ITextDocumentFactoryService textDocumentFactoryService { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            textDocumentFactoryService.TryGetTextDocument(textBuffer, out ITextDocument textDocument);
            return new ShaderlabCompletionSource(this, textBuffer, textDocument);
        }
    }
    #endregion

    #region Shaderlab Completion Command Handler
    public class ShaderlabCompletionCommandHandlder : IOleCommandTarget
    {
        private readonly IOleCommandTarget _nextCommandHandler;
        private readonly ITextView _textView;
        private readonly ShaderlabCompletionHandlerPrvider _completionHandlerProvider;
        private ICompletionSession _completionSession;

        public ShaderlabCompletionCommandHandlder(IVsTextView textViewAdapter, ITextView textView, ShaderlabCompletionHandlerPrvider handlerProvider)
        {
            _textView = textView;
            _completionHandlerProvider = handlerProvider;

            textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);

        }

        private bool TriggerCompletion()
        {
            SnapshotPoint? caretPoint = _textView.Caret.Position.Point.GetPoint(textBuffer => !textBuffer.ContentType.IsOfType("projection"), PositionAffinity.Predecessor);

            if (caretPoint.HasValue)
            {
                _completionSession = _completionHandlerProvider.CompletionBroker.CreateCompletionSession(_textView, caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive), true);
                _completionSession.Dismissed += CompletionSessionDismissed;
                _completionSession.Start();
                return true;
            }

            return false;
        }

        private void CompletionSessionDismissed(object sender, EventArgs e)
        {
            _completionSession.Dismissed -= CompletionSessionDismissed;
            _completionSession = null;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            uint cmdID = nCmdID;
            char typedChar = char.MinValue;

            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN || cmdID == (uint)VSConstants.VSStd2KCmdID.TAB)
            {
                if (_completionSession != null && !_completionSession.IsDismissed)
                {
                    if (_completionSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        _completionSession.Commit();
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        _completionSession.Dismiss();
                    }
                }
            }

            int returnValue = _nextCommandHandler.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            bool isHandled = false;

            if (!typedChar.Equals(char.MinValue))
            {
                if ((_completionSession is null) || _completionSession.IsDismissed)
                {
                    TriggerCompletion();

                    if (_completionSession != null)
                    {
                        _completionSession.Filter();
                    }
                }
                else
                {
                    _completionSession.Filter();
                }

                isHandled = true;
            }
            else if (cmdID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE || cmdID == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (_completionSession != null && !_completionSession.IsDismissed)
                {
                    _completionSession.Filter();
                }

                isHandled = true;
            }

            if (isHandled)
            {
                return VSConstants.S_OK;
            }

            return returnValue;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
    #endregion

    #region Shaderlab Completion Handler Provider
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(Constants.ContentType)]
    [Name("ShaderlabCompletionHandlerPrvider")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    public class ShaderlabCompletionHandlerPrvider : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService AdapterService = null;

        [Import]
        public ICompletionBroker CompletionBroker { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);

            if (textView is null)
            {
                return;
            }

            textView.Properties.GetOrCreateSingletonProperty(() => new ShaderlabCompletionCommandHandlder(textViewAdapter, textView, this));
        }
    }
    #endregion

    #region BraceCompletion
    [Export(typeof(IBraceCompletionContextProvider))]
    [ContentType(Constants.ContentType)]
    [BracePair('(', ')')]
    [BracePair('[', ']')]
    [BracePair('{', '}')]
    [BracePair('"', '"')]
    [BracePair('\'', '\'')]
    internal sealed class ShaderlabVSBraceCompletionContextProvider : IBraceCompletionContextProvider
    {
        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context)
        {
            context = null;

            if (IsValidBraceCompletionContext(openingPoint))
            {
                context = new ShaderlabBraceCompletionContext();
                return true;
            }

            return false;
        }

        private bool IsValidBraceCompletionContext(SnapshotPoint openingPoint) => openingPoint.Position >= 0;
    }

    [Export(typeof(IBraceCompletionContext))]
    internal sealed class ShaderlabBraceCompletionContext : IBraceCompletionContext
    {
        public bool AllowOverType(IBraceCompletionSession session) => true;

        public void Finish(IBraceCompletionSession session)
        {
        }

        public void OnReturn(IBraceCompletionSession session)
        {
        }

        public void Start(IBraceCompletionSession session)
        {
        }
    }
    #endregion
}
