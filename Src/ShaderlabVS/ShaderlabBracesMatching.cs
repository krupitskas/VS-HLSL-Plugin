using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace ShaderlabVS
{
    internal class ShaderlabBracesTagger : ITagger<TextMarkerTag>
    {
        private readonly ITextView _textView;
        private readonly ITextBuffer _textBuffer;
        private readonly Dictionary<char, char> _braces;
        private SnapshotPoint? _currentCharSnapPoint;

        public ShaderlabBracesTagger(ITextView textView, ITextBuffer buffer)
        {
            _textView = textView;
            _textBuffer = buffer;

            _braces = new Dictionary<char, char>()
            {
                {'{', '}'},
                {'(', ')'},
                {'[', ']'}
            };

            _currentCharSnapPoint = null;

            _textView.Caret.PositionChanged += CaretPositionChanged;
            _textView.LayoutChanged += TextViewLayoutChanged;
        }

        private void TextViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.NewSnapshot != e.OldSnapshot)
            {
                UpdateAtCaretPosition(_textView.Caret.Position);
            }
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e) => UpdateAtCaretPosition(e.NewPosition);

        private void UpdateAtCaretPosition(CaretPosition caretPos)
        {
            _currentCharSnapPoint = caretPos.Point.GetPoint(_textBuffer, caretPos.Affinity);

            if (_currentCharSnapPoint.HasValue)
            {
                if (TagsChanged != null)
                {
                    ITextSnapshot currentSnapshot = _textBuffer.CurrentSnapshot;
                    TagsChanged.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(currentSnapshot, 0, currentSnapshot.Length)));
                }
            }
        }

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0
                || _currentCharSnapPoint == null
                || _currentCharSnapPoint.Value.Snapshot.Length <= _currentCharSnapPoint.Value.Position)
            {
                yield break;
            }


            // if in commandline, we do nothing
            if (Utilities.IsInCommentLine(_currentCharSnapPoint.Value))
            {
                yield break;
            }

            // For open char, the matched state trigger when the caret at the right side of the brace char.
            // For closed char, the matched state trigger when the caret at right side of the brace char.
            char currentChar = _currentCharSnapPoint.Value.GetChar();
            SnapshotPoint? lastCharSnapShot = _currentCharSnapPoint == 0 ? _currentCharSnapPoint : _currentCharSnapPoint - 1;
            char lastChar = lastCharSnapShot.Value.GetChar();

            SnapshotPoint? matchedPosition = null;

            if (IsOpenBrace(currentChar))
            {
                char closedChar = GetClosedCharByOpenBrace(currentChar);
                FindMatchingBrace(_currentCharSnapPoint.Value, currentChar, closedChar, true, ref matchedPosition);

                if (matchedPosition.HasValue)
                {
                    yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(_currentCharSnapPoint.Value, 1), new TextMarkerTag(Constants.ShaderlabBracesMarker));
                    yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(matchedPosition.Value, 1), new TextMarkerTag(Constants.ShaderlabBracesMarker));
                }
            }

            if (IsCloseBrace(lastChar))
            {
                char openChar = GetOpenCharByClosedBrace(lastChar);
                FindMatchingBrace(_currentCharSnapPoint.Value, openChar, lastChar, false, ref matchedPosition);

                if (matchedPosition.HasValue)
                {
                    yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(lastCharSnapShot.Value, 1), new TextMarkerTag(Constants.ShaderlabBracesMarker));
                    yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(matchedPosition.Value, 1), new TextMarkerTag(Constants.ShaderlabBracesMarker));
                }
            }

        }

        private void FindMatchingBrace(SnapshotPoint startPoint, char openChar, char closedChar, bool isMatchingClosedChar, ref SnapshotPoint? matchedPosition)
        {
            SnapshotPoint currentCheckPos = startPoint - 1;
            int step = -1;

            if (isMatchingClosedChar)
            {
                currentCheckPos = startPoint;
                step = 1;
            }

            int matchIndex = 1;

            while (currentCheckPos > 0 && currentCheckPos.Position <= currentCheckPos.Snapshot.Length - 1)
            {
                currentCheckPos += step;

                if (currentCheckPos < 0)
                {
                    break;
                }

                char currentCheckChar = currentCheckPos.GetChar();

                if (isMatchingClosedChar)
                {
                    if (currentCheckChar == openChar && !Utilities.IsInCommentLine(currentCheckPos))
                    {
                        ++matchIndex;
                    }

                    if (currentCheckChar == closedChar && !Utilities.IsInCommentLine(currentCheckPos))
                    {
                        --matchIndex;
                    }
                }
                else
                {
                    if (currentCheckChar == closedChar && !Utilities.IsInCommentLine(currentCheckPos))
                    {
                        ++matchIndex;
                    }

                    if (currentCheckChar == openChar && !Utilities.IsInCommentLine(currentCheckPos))
                    {
                        --matchIndex;
                    }
                }

                // if match index equals to 0, we think we find the matched brace char out.
                if (matchIndex == 0)
                {
                    break;
                }
            }

            if (matchIndex == 0)
            {
                matchedPosition = currentCheckPos;
                return;
            }
        }

        private bool IsOpenBrace(char c) => _braces.ContainsKey(c);

        private bool IsCloseBrace(char c) => _braces.ContainsValue(c);

        private char GetOpenCharByClosedBrace(char closedChar) => _braces.First(b => b.Value.Equals(closedChar)).Key;

        private char GetClosedCharByOpenBrace(char openChar)
        {
            _braces.TryGetValue(openChar, out char closedChar);
            return closedChar;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }

    [Export(typeof(IViewTaggerProvider))]
    [ContentType(Constants.ContentType)]
    [TagType(typeof(TextMarkerTag))]
    internal class ShaderlabBraceMatchingTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return (textView is null) || (buffer is null) ? null : new ShaderlabBracesTagger(textView, buffer) as ITagger<T>;
        }
    }
}
