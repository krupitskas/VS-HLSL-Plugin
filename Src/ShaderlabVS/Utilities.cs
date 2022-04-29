using Microsoft.VisualStudio.Text;

namespace ShaderlabVS
{
    internal class Utilities
    {
        public static bool IsCommentLine(string lineText)
        {
            string checkText = lineText.Trim();

            return checkText.StartsWith("//") || checkText.StartsWith("/*") || checkText.EndsWith("*/");
        }

        public static bool IsInCommentLine(SnapshotPoint position)
        {
            string lineText = position.GetContainingLine().GetText();
            return IsCommentLine(lineText);
        }

        public static int IndexOfNonWhitespaceCharacter(string text)
        {
            for (int i = 0; i < text.Length; ++i)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool IsInCGOrHLSLFile(string filePath)
        {
            string lower = filePath.ToLower();
            return lower.EndsWith(".cg") || lower.EndsWith(".hlsl");
        }
    }
}
