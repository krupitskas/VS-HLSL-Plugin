using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using ShaderlabVS.Data;

namespace ShaderlabVS
{
    #region Shaderlab Quickinfo Source
    internal class ShaderlabQuickInfoSource : IQuickInfoSource
    {
        private readonly ShaderlabQuickInfoSourceProvider _provider;
        private readonly ITextBuffer _textBuffer;
        private readonly Dictionary<string, string> _quickInfos;

        public ShaderlabQuickInfoSource(ShaderlabQuickInfoSourceProvider provider, ITextBuffer textBuffer)
        {
            _provider = provider;
            _textBuffer = textBuffer;
            _quickInfos = new Dictionary<string, string>();
            _isDisposed = false;
            QuickInfoInit();
        }

        private void QuickInfoInit()
        {
            ShaderlabDataManager.Instance.HLSLCGFunctions.ForEach((f) =>
                {
                    if (_quickInfos.ContainsKey(f.Name))
                    {
                        string info = _quickInfos[f.Name];
                        info += $"\nFunction: {f.Description}";
                        _quickInfos[f.Name] = info;
                    }
                    else
                    {
                        _quickInfos.Add(f.Name, f.Description);
                    }
                });

            ShaderlabDataManager.Instance.UnityBuiltinDatatypes.ForEach((d) =>
                {
                    if (_quickInfos.ContainsKey(d.Name))
                    {
                        _quickInfos[d.Name] = _quickInfos[d.Name] + $"\nUnity3d built-in balues: {d.Description}";
                    }
                    else
                    {
                        _quickInfos.Add(d.Name, d.Description);
                    }
                });

            ShaderlabDataManager.Instance.UnityBuiltinFunctions.ForEach((f) =>
                {
                    if (_quickInfos.ContainsKey(f.Name))
                    {
                        _quickInfos[f.Name] = _quickInfos[f.Name] + $"\nUnity3D built-in function: {f.Description}";
                    }
                    else
                    {
                        _quickInfos.Add(f.Name, f.Description);
                    }
                });

            ShaderlabDataManager.Instance.UnityBuiltinMacros.ForEach((f) =>
                {

                    string description = $"{string.Join(";\n", f.Synopsis)}\n{f.Description}";
                    if (_quickInfos.ContainsKey(f.Name))
                    {
                        _quickInfos[f.Name] = _quickInfos[f.Name] + $"\nUnity3D built-in macros: {description}";
                    }
                    else
                    {
                        _quickInfos.Add(f.Name, description);
                    }
                });

            ShaderlabDataManager.Instance.UnityKeywords.ForEach((k) =>
                {
                    if (_quickInfos.ContainsKey(k.Name))
                    {
                        _quickInfos[k.Name] = _quickInfos[k.Name] + $"\nUnity3D keywords: {k.Description}";
                    }
                    else
                    {
                        _quickInfos.Add(k.Name, k.Description);
                    }
                });

            ShaderlabDataManager.Instance.UnityBuiltinValues.ForEach((v) =>
                {
                    if (_quickInfos.ContainsKey(v.Name))
                    {
                        _quickInfos[v.Name] = _quickInfos[v.Name] + $"\nUnity3d built-in values: {v.VauleDescription}";
                    }
                    else
                    {
                        _quickInfos.Add(v.Name, v.VauleDescription);
                    }
                });
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out Microsoft.VisualStudio.Text.ITrackingSpan applicableToSpan)
        {
            SnapshotPoint? sp = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);

            if (!sp.HasValue)
            {
                applicableToSpan = null;
                return;
            }

            ITextSnapshot currentSnapshot = sp.Value.Snapshot;
            SnapshotSpan span = new SnapshotSpan(sp.Value, 0);

            ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_textBuffer);
            string keyText = navigator.GetExtentOfWord(sp.Value).Span.GetText().Trim();

            if (string.IsNullOrEmpty(keyText))
            {
                applicableToSpan = null;
                return;
            }

            _quickInfos.TryGetValue(keyText, out string info);

            if (!string.IsNullOrEmpty(info))
            {
                applicableToSpan = currentSnapshot.CreateTrackingSpan(span.Start.Position, 9, SpanTrackingMode.EdgeInclusive);
                quickInfoContent.Add(info);
                return;
            }

            applicableToSpan = null;
        }

        private bool _isDisposed;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }
    #endregion

    #region Shaderlab Quickinfo Source Provider
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("ShaderlabQuickInfoSourceProvider")]
    [Order(Before = "Default Quick Info Presenter")]
    [ContentType(Constants.ContentType)]
    internal class ShaderlabQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        public ITextStructureNavigatorSelectorService NavigatorService = null;

        [Import]
        public ITextBufferFactoryService TextBufferFactoryService = null;

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer) => new ShaderlabQuickInfoSource(this, textBuffer);
    }
    #endregion

    #region Shaderlab Quickinfo Controller

    internal class ShaderlabQuickInfoController : IIntellisenseController
    {
        private ITextView _textView;
        private readonly IList<ITextBuffer> _textBuffers;
        private readonly ShaderlabQuickInfoControllerProvider _controllerProvider;
        private IQuickInfoSession _quickInfoSession;

        public ShaderlabQuickInfoController(ITextView textView, IList<ITextBuffer> textBuffers, ShaderlabQuickInfoControllerProvider controllerProvider)
        {
            _textBuffers = textBuffers;
            _textView = textView;
            _controllerProvider = controllerProvider;

            textView.MouseHover += TextViewMouseHover;
        }

        private void TextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            SnapshotPoint? ssPoint = _textView.BufferGraph.MapDownToFirstMatch(new SnapshotPoint(_textView.TextSnapshot, e.Position),
                                                                            PointTrackingMode.Positive,
                                                                            snapshot => _textBuffers.Contains(snapshot.TextBuffer),
                                                                            PositionAffinity.Predecessor);

            if (ssPoint != null)
            {
                ITrackingPoint point = ssPoint.Value.Snapshot.CreateTrackingPoint(ssPoint.Value.Position, PointTrackingMode.Positive);

                if (!_controllerProvider.QuickInfoBroker.IsQuickInfoActive(_textView))
                {
                    _quickInfoSession = _controllerProvider.QuickInfoBroker.TriggerQuickInfo(_textView, point, true);
                }
            }
        }

        public void Detach(ITextView textView)
        {
            if (_textView == textView)
            {
                _textView.MouseHover -= TextViewMouseHover;
                _textView = null;
            }
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
    #endregion

    #region Shaderlab Quickinfo Controller Provoider

    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("ShaderlabQuickInfoControllerProvider")]
    [ContentType(Constants.ContentType)]
    internal class ShaderlabQuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        public IQuickInfoBroker QuickInfoBroker { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            return new ShaderlabQuickInfoController(textView, subjectBuffers, this);
        }
    }
    #endregion
}
