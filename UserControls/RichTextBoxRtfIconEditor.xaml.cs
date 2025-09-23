using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using wf = System.Windows.Forms;              // WinForms alias
using sd = System.Drawing;                   // System.Drawing alias
using System.Windows.Input;
using System.Windows.Documents;

namespace JiME.UserControls
{
    /// <summary>
    /// Interaction logic for RichTextBoxRtfIconEditor.xaml
    /// </summary>
    public partial class RichTextBoxRtfIconEditor : UserControl, IDisposable
    {
        private wf.RichTextBox _rtf;
        private bool _disposed;
        /// <summary>
        /// Name of the special icon font you use for glyph icons.
        /// Use the family name as it appears in FontFamily.Name.
        /// </summary>
        public string IconFontFamilyName { get; set; } = "LoTR JiME Icons";
        public string IconFontFamilyNameShort { get; set; } = "Icon";

        public int MinTextHeight
        {
            get => _rtf.MinimumSize.Height;
            set
            {
                _rtf.MinimumSize = new System.Drawing.Size(0, value);
            }
        }

        public int TextHeight
        {
            get => _rtf.Height;
            set
            {
                _rtf.Height = value;
            }
        }

        public int MaxTextHeight
        {
            get => _rtf.MaximumSize.Height;
            set
            {
                _rtf.MaximumSize = new System.Drawing.Size(0, value);
            }
        }

        public RichTextBoxRtfIconEditor()
        {
            InitializeComponent();
            InitializeWinFormsRichTextBox();
        }

        private void InitializeWinFormsRichTextBox()
        {
            _rtf = new wf.RichTextBox()
            {
                Dock = wf.DockStyle.Fill,
                Multiline = true,
                AcceptsTab = true,
                WordWrap = true,
                DetectUrls = false,
                ScrollBars = wf.RichTextBoxScrollBars.Both,
                // Optional: set a default font that matches your WPF look.
                Font = new sd.Font("Segoe UI", 12f, sd.FontStyle.Regular)
            };

            // Optionally: handle key events, e.g. Ctrl+B / Ctrl+I
            _rtf.KeyDown += OnRtfKeyDown;

            formsHost.Child = _rtf;
        }

        private void OnRtfKeyDown(object sender, wf.KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == wf.Keys.B)
            {
                ToggleBold();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == wf.Keys.I)
            {
                ToggleItalic();
                e.Handled = true;
            }
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            InsertIconGlyph(button.Content as string);
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBold();
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleItalic();
        }

        private void ClearFormattingButton_Click(object sender, RoutedEventArgs e)
        {
            ClearStyle();
        }

        #region Basic editing API

        /// <summary>
        /// Inserts the given glyph (string) using IconFontFamilyName at the current caret,
        /// or replaces the selected text. The glyph is inserted with the RTF control's current font size.
        /// </summary>
        public void InsertIconGlyph(string glyph)
        {
            if (string.IsNullOrEmpty(glyph)) return;

            int selStart = _rtf.SelectionStart;
            int selLength = _rtf.SelectionLength;

            float emSize = _rtf.SelectionFont?.Size ?? _rtf.Font.Size;

            var iconFont = new sd.Font(IconFontFamilyName, emSize, sd.FontStyle.Regular);

            // Replace selection or insert
            _rtf.SelectedText = glyph;
            // Apply font to the inserted range
            _rtf.SelectionStart = selStart;
            _rtf.SelectionLength = glyph.Length;
            _rtf.SelectionFont = iconFont;

            // Move caret after inserted glyph
            _rtf.SelectionStart = selStart + glyph.Length;
            _rtf.SelectionLength = 0;
            _rtf.Focus();
        }

        /// <summary>
        /// Toggles bold at the current selection (or caret position).
        /// </summary>
        public void ToggleBold()
        {
            ApplyToggleStyle(sd.FontStyle.Bold);
        }

        /// <summary>
        /// Toggles italic at the current selection (or caret position).
        /// </summary>
        public void ToggleItalic()
        {
            ApplyToggleStyle(sd.FontStyle.Italic);
        }

        private void ApplyToggleStyle(sd.FontStyle styleToToggle)
        {
            int selStart = _rtf.SelectionStart;
            int selLength = Math.Max(1, _rtf.SelectionLength); // at least apply to caret

            // If there is no selection we apply to the insertion point by creating a 0-length selection after setting SelectionFont.
            // However, WinForms' SelectionFont becomes null when selection spans multiple fonts/styles.
            // To handle mixed selections, we will apply style per-character grouping.
            // Save original selection
            int origStart = _rtf.SelectionStart;
            int origLen = _rtf.SelectionLength;

            // If selection length is 0, we create a 1-char temporary insertion font if needed.
            if (origLen == 0)
            {
                var curFont = _rtf.SelectionFont ?? _rtf.Font;
                bool hasStyle = (curFont.Style & styleToToggle) != 0;
                sd.Font newFont = new sd.Font(curFont, hasStyle ? curFont.Style & ~styleToToggle : curFont.Style | styleToToggle);
                _rtf.SelectionFont = newFont;
                // Restore caret (font remains for typing)
                _rtf.SelectionLength = 0;
                _rtf.SelectionStart = origStart;
                return;
            }

            // For non-empty selection - iterate runs and toggle style on contiguous ranges
            int idx = origStart;
            int end = origStart + origLen;
            while (idx < end)
            {
                _rtf.SelectionStart = idx;
                _rtf.SelectionLength = 1;
                var f = _rtf.SelectionFont ?? _rtf.Font;
                sd.FontStyle newStyle;
                if ((f.Style & styleToToggle) != 0)
                {
                    newStyle = f.Style & ~styleToToggle;
                }
                else
                {
                    newStyle = f.Style | styleToToggle;
                }

                sd.Font nf;
                try
                {
                    nf = new sd.Font(f.FontFamily, f.Size, newStyle);
                }
                catch
                {
                    // In rare cases font creation might fail, fallback to base font with toggled style
                    nf = new sd.Font(f, newStyle);
                }

                _rtf.SelectionFont = nf;
                idx++;
            }

            // Restore original selection
            _rtf.SelectionStart = origStart;
            _rtf.SelectionLength = origLen;
            _rtf.Focus();
        }

        /// <summary>
        /// Clears all style formatting (bold, italic, and any special font family) on the current selection
        /// or at the caret position if nothing is selected, resetting to the default control font.
        /// </summary>
        public void ClearStyle()
        {
            if (_rtf == null) return;

            // Save selection
            int origStart = _rtf.SelectionStart;
            int origLen = _rtf.SelectionLength;

            // Determine the range to clear:
            if (origLen == 0)
            {
                // No selection: apply at caret; clear future typing style
                // By selecting 1 character, we can reset its style, then move caret back
                if (_rtf.TextLength > 0 && origStart < _rtf.TextLength)
                {
                    _rtf.SelectionLength = 1;
                }
                else
                {
                    // Nothing to clear
                    return;
                }
            }

            // Default font to reset to
            var defaultFont = _rtf.Font;  // control’s default font (FontFamily, size, style = Regular)
            if (defaultFont == null)
                defaultFont = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Regular);

            int clearStart = _rtf.SelectionStart;
            int clearLen = _rtf.SelectionLength;

            // Loop character-by-character in range, resetting formatting
            for (int i = 0; i < clearLen; i++)
            {
                _rtf.Select(clearStart + i, 1);
                // Optionally preserve color or other properties, here we only reset font
                _rtf.SelectionFont = defaultFont;
            }

            // Restore selection or caret
            _rtf.SelectionStart = origStart;
            _rtf.SelectionLength = origLen;

            _rtf.Focus();
        }


        #endregion

        #region Simplified HTML export/import

        /// <summary>
        /// Export the whole document to a simplified HTML string supporting <b>, <i>, and <font name="..."> tags.
        /// </summary>
        public string ExportToSimplifiedHtml()
        {
            // Save selection to restore later
            int origStart = _rtf.SelectionStart;
            int origLen = _rtf.SelectionLength;

            int length = _rtf.TextLength;
            if (length == 0) return string.Empty;

            var sb = new StringBuilder();

            int idx = 0;
            // We'll read char-by-char and group runs with identical formatting
            while (idx < length)
            {
                _rtf.SelectionStart = idx;
                _rtf.SelectionLength = 1;

                var ch = _rtf.SelectedText;
                var font = _rtf.SelectionFont ?? _rtf.Font;
                bool bold = (font.Style & sd.FontStyle.Bold) != 0;
                bool italic = (font.Style & sd.FontStyle.Italic) != 0;
                string fam = font.FontFamily.Name;

                // Gather run
                int runStart = idx;
                int runLen = 1;
                idx++;
                while (idx < length)
                {
                    _rtf.SelectionStart = idx;
                    _rtf.SelectionLength = 1;
                    var f2 = _rtf.SelectionFont ?? _rtf.Font;
                    bool bold2 = (f2.Style & sd.FontStyle.Bold) != 0;
                    bool italic2 = (f2.Style & sd.FontStyle.Italic) != 0;
                    string fam2 = f2.FontFamily.Name;

                    if (bold2 == bold && italic2 == italic && fam2 == fam)
                    {
                        runLen++;
                        idx++;
                    }
                    else
                    {
                        break;
                    }
                }

                // Read text for run
                _rtf.SelectionStart = runStart;
                _rtf.SelectionLength = runLen;
                string runText = _rtf.SelectedText;
                // Escape basic HTML chars
                runText = EscapeForHtml(runText);

                // Wrap with tags: font -> b -> i order
                // But if icon font, ignore bold/italic and just use font tag
                string wrapped = runText;
                if (!string.IsNullOrEmpty(fam) && fam.Equals(IconFontFamilyName, StringComparison.OrdinalIgnoreCase))
                {
                    // treat icon font specially
                    wrapped = $"<font=\"{XmlEscape(IconFontFamilyNameShort)}\">{wrapped}</font>";
                }
                else
                {
                    if (italic) wrapped = $"<i>{wrapped}</i>";
                    if (bold) wrapped = $"<b>{wrapped}</b>";
                }

                sb.Append(wrapped);
            }

            // Restore selection
            _rtf.SelectionStart = origStart;
            _rtf.SelectionLength = origLen;
            return sb.ToString();
        }

        /// <summary>
        /// Import from the simplified HTML format produced by ExportToSimplifiedHtml.
        /// This does a very small parsing: recognizes <b>, <i>, <font name="..."> and plain text.
        /// </summary>
        public void ImportFromSimplifiedHtml(string simplifiedHtml)
        {
            if (simplifiedHtml == null) simplifiedHtml = string.Empty;

            // Clear existing content
            _rtf.Clear();

            // Very simple stack-based parser
            var idx = 0;
            var n = simplifiedHtml.Length;
            var boldStack = new Stack<bool>();
            var italicStack = new Stack<bool>();
            var fontStack = new Stack<string>();

            boldStack.Push(false);
            italicStack.Push(false);
            fontStack.Push(null);

            while (idx < n)
            {
                if (simplifiedHtml[idx] == '<')
                {
                    // find tag end
                    int gt = simplifiedHtml.IndexOf('>', idx);
                    if (gt == -1) break;
                    string tag = simplifiedHtml.Substring(idx + 1, gt - idx - 1).Trim();

                    // closing tags
                    if (tag.StartsWith("/"))
                    {
                        string closeTag = tag.Substring(1).Trim().ToLowerInvariant();
                        if (closeTag == "b" && boldStack.Count > 0) boldStack.Pop();
                        else if (closeTag == "i" && italicStack.Count > 0) italicStack.Pop();
                        else if (closeTag == "font" && fontStack.Count > 0) fontStack.Pop();
                    }
                    else
                    {
                        // opening tags
                        if (tag.Equals("b", StringComparison.OrdinalIgnoreCase))
                        {
                            boldStack.Push(true);
                        }
                        else if (tag.Equals("i", StringComparison.OrdinalIgnoreCase))
                        {
                            italicStack.Push(true);
                        }
                        else if (tag.StartsWith("font", StringComparison.OrdinalIgnoreCase))
                        {
                            // parse name attribute
                            var m = Regex.Match(tag, "(name)?\\s*=\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                fontStack.Push(XmlUnescape(m.Groups[2].Value));
                            }
                            else
                            {
                                fontStack.Push(null);
                            }
                        }
                        else
                        {
                            // unknown tag - ignore
                        }
                    }
                    idx = gt + 1;
                }
                else
                {
                    // text until next tag
                    int nextTag = simplifiedHtml.IndexOf('<', idx);
                    string text;
                    if (nextTag == -1)
                    {
                        text = simplifiedHtml.Substring(idx);
                        idx = n;
                    }
                    else
                    {
                        text = simplifiedHtml.Substring(idx, nextTag - idx);
                        idx = nextTag;
                    }
                    text = UnescapeHtml(text);
                    // Append text with current formatting
                    sd.Font baseFont = _rtf.Font;
                    sd.FontStyle style = sd.FontStyle.Regular;
                    if (boldStack.Count > 0 && boldStack.Peek()) style |= sd.FontStyle.Bold;
                    if (italicStack.Count > 0 && italicStack.Peek()) style |= sd.FontStyle.Italic;
                    string fam = fontStack.Count > 0 ? fontStack.Peek() : null;
                    sd.Font f;
                    try
                    {
                        if (!string.IsNullOrEmpty(fam) && (fam == IconFontFamilyNameShort || fam == IconFontFamilyName))
                        {
                            f = new sd.Font(IconFontFamilyName, baseFont.Size, style);
                        }
                        else
                        {
                            f = new sd.Font(baseFont, style);
                        }
                    }
                    catch
                    {
                        f = new sd.Font(baseFont, style);
                    }

                    int insertPos = _rtf.TextLength;
                    _rtf.SelectionStart = insertPos;
                    _rtf.SelectionLength = 0;
                    _rtf.SelectionFont = f;
                    _rtf.SelectedText = text;
                }
            }
        }

        private static string EscapeForHtml(string s)
        {
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static string UnescapeHtml(string s)
        {
            return s.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
        }

        private static string XmlEscape(string s)
        {
            return s.Replace("\"", "&quot;").Replace("&", "&amp;");
        }

        private static string XmlUnescape(string s)
        {
            return s.Replace("&quot;", "\"").Replace("&amp;", "&");
        }

        #endregion

        #region Helper & housekeeping

        public string GetRtf() => _rtf.Rtf;
        public void SetRtf(string rtf) => _rtf.Rtf = rtf;
        public string GetPlainText() => _rtf.Text;
        public void SetPlainText(string text)
        {
            _rtf.Rtf = string.Empty;
            _rtf.Text = text;
        }

        public void FocusEditor()
        {
            _rtf?.Focus();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            _rtf?.Focus();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_rtf != null)
            {
                _rtf.KeyDown -= OnRtfKeyDown;
                _rtf.Dispose();
                _rtf = null;
            }
            formsHost.Child = null;
        }

        ~RichTextBoxRtfIconEditor()
        {
            Dispose();
        }

        #endregion
    }
}
