using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace ABSoftware.Engine
{
    public class CodeTextBoxHighlight
    {
        public TextRange Range;
        public Color Color;

        public CodeTextBoxHighlight(TextRange range, Color color) { Range = range; Color = color; }
    }

    /// <summary>
    /// Interaction logic for CodeTextBox.xaml
    /// </summary>
    public partial class CodeTextBox : UserControl
    {
        #region Private Properties

        /// <summary>
        /// All of the locations that are highlighted.
        /// </summary>
        internal List<CodeTextBoxHighlight> highlights = new List<CodeTextBoxHighlight>();

        /// <summary>
        /// The start of where we need to highlight.
        /// </summary>
        internal TextPointer HighlightStart;

        /// <summary>
        /// The end of where we need to highlight.
        /// </summary>
        internal TextPointer HighlightEnd;

        /// <summary>
        /// Whether the start/end for highlighting has been decided yet.
        /// </summary>
        internal bool chosenStartEnd;

        private int lastLineCount;

        #endregion

        #region Public Properties
        /// <summary>
        /// The text within this CodeTextBox.
        /// </summary>
        public string Text
        {
            get
            {
                return new TextRange(txtCode.Document.ContentStart, txtCode.Document.ContentEnd).Text;
            }
        }

        /// <summary>
        /// The text within this CodeTextBox - with no line breaks!
        /// </summary>
        public string TextNoLineBreaks
        {
            get
            {
                return new TextRange(txtCode.Document.ContentStart, txtCode.Document.ContentEnd).Text.Replace("\r\n", "");
            }
        }

        /// <summary>
        /// Gets the number of lines - based on <see cref="Environment.NewLine"/>
        /// </summary>
        public int LineCount
        {
            get
            {
                return Text.Split('\n').Length - 1;
            }
        }

        public int TextLength
        {
            get
            {
                return new TextRange(txtCode.Document.ContentStart, txtCode.Document.ContentEnd).Text.Length;
            }
        }

        public CodeTextBoxHighlighter TextHighlighter { get; set; }

        /// <summary>
        /// The current location we're at when highlighting.
        /// </summary>
        public TextPointer CurrentPointer;

        /// <summary>
        /// The start of a certain color.
        /// </summary>
        public TextPointer StartPointer;

        /// <summary>
        /// Whether we're already waiting a second for the finishtextchanged event.
        /// </summary>
        private bool alreadyWaiting;

        private DateTime lastDTime;
        #endregion

        #region Managed Methods
        public CodeTextBox()
        {
            InitializeComponent();

            // Allow the line numbers to scroll.
            LineNumbersScroll.CanContentScroll = true;
        }

        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Don't bother, If there were no changes.
            if (e.Changes.Count == 0)
                return;

            // Set the line numbers now.
            // The total number of lines.
            var lineCount = LineCount;

            // We only want to change the line numbers if the total number of lines has changed.
            if (lineCount != lastLineCount)
            {
                // The process is fairly simple for the line numbers.
                // Just add a massive TextBlock, with all the line numbers.
                // And then sync up its scroll with the scroll of the TextBox.

                // Update the last total number of lines we have.
                lastLineCount = lineCount;

                // Set the height of the line numbers of the TextBlock.
                //LineNumbers.Height = txtCode.ExtentHeight;

                // Reset the text inside the TextBlock
                LineNumbers.Text = "";

                // Make sure the FontSize is the same as the one from the main code
                LineNumbers.FontSize = txtCode.FontSize;

                // Now, set the text inside the line numbers
                for (int i = 0; i < lastLineCount; i++)
                    LineNumbers.Text += (i + 1) + Environment.NewLine;
            }

            // Don't continue if the document is null.
            if (txtCode.Document == null)
                return;

            // Now, go through each change and decide the lowest and highest blocks.
            foreach (TextChange change in e.Changes)
            {
                // Get this block's start.
                var start = LastIndexOf(txtCode.Document.ContentStart.GetPositionAtOffset(change.Offset), TextHighlighter.BlockStart) ?? txtCode.Document.ContentStart;

                // Get this block's end.
                var end = IndexOf(txtCode.Document.ContentStart.GetPositionAtOffset(change.Offset + change.AddedLength), TextHighlighter.BlockEnd) ?? txtCode.Document.ContentEnd;

                // Keep track of the position this was done, so that we only need to process a section in the FinishTextChanged.
                // If we haven't decided on anything yet, make the start/end the start/end.
                if (!chosenStartEnd)
                {
                    // Set the start to the last "ObjectStart" - at the start of this "block".
                    HighlightStart = start;

                    // Set the end to the end of where the text was changed - at the end of the line.
                    HighlightEnd = end;

                    // We've now decided on a basic start/end - which can be changed.
                    chosenStartEnd = true;
                }
                else
                {
                    // If this has the lowest offset so far, this needs to be the start since it's before the current start - set it to the start of the line.
                    if (start.CompareTo(HighlightStart) < 0)
                        HighlightStart = start;

                    var rng = new TextRange(start, end);
                    // If this has the highest offset so far, tihs needs to be the end since it's after the current end.
                    if (end.CompareTo(HighlightEnd) > 0)
                        HighlightEnd = end;
                }
            }

            // This is used to decide if there have been any other calls to this event in those 400ms - if so, don't do anything on this one.
            var lastDateTime = DateTime.Now;
            lastDTime = lastDateTime;

            // The rest will happen after 400ms.
            await Task.Delay(400);

            // Now, call to the FinishTextChanged if needed - this is where all the highlighting is done.
            if (lastDateTime == lastDTime)
                TextBox_FinishTextChanged();

            //await Task.Delay(400);

            //var watch = Stopwatch.StartNew();

            //// It will call the FinishTextChanged - which will run after 400ms, and will process the code - with highlighting, collapsing etc.
            //TextBox_FinishTextChanged();

            //watch.Stop();
            //Console.WriteLine(watch.ElapsedMilliseconds);
        }

        private TextPointer IndexOf(TextPointer pointerArg, char ch)
        {
            // Store whether we found the character.
            bool found = false;

            // Store the character it's on in the run it's in, so that we can get an exact TextPointer position.
            var i = 0;

            // The current position we're at.
            var pointer = pointerArg;

            // Keep going if we haven't found it - or we've reached the end.
            while (!found && pointer.CompareTo(txtCode.Document.ContentEnd) < 0)
            {
                // Go to the next symbol.
                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);

                // Get the current context - this is essentially what this symbol means.
                var context = pointer.GetPointerContext(LogicalDirection.Backward);

                // If this is a run - that's where the text is so we can check it!
                if (context == TextPointerContext.ElementStart && pointer.Parent is Run run)
                    for (i = 0; i < run.Text.Length; i++)
                        if (run.Text[i] == ch)
                            found = true;
            }

            // Return whether we found the character.
            return (found) ? pointer.GetPositionAtOffset(i) : null;
        }

        private TextPointer LastIndexOf(TextPointer pointerArg, char ch)
        {
            // Store whether we found the character.
            bool found = false;

            // Store the character it's on in the run it's in, so that we can get an exact TextPointer position.
            var i = 0;

            // The current position we're at.
            var pointer = pointerArg;

            // Keep going if we haven't found it - or we've reached the end.
            while (!found && pointer.CompareTo(txtCode.Document.ContentStart) > 0)
            {
                // Go to the next symbol.
                pointer = pointer.GetNextContextPosition(LogicalDirection.Backward);

                // Get the current context - this is essentially what this symbol means.
                var context = pointer.GetPointerContext(LogicalDirection.Backward);

                // If this is a run - that's where the text is so we can check it!
                if (context == TextPointerContext.ElementStart && pointer.Parent is Run run)
                    for (i = run.Text.Length - 1; i >= 0; i--)
                        if (run.Text[i] == ch)
                            found = true;
            }

            // Return whether we found the character.
            return (found) ? pointer.GetPositionAtOffset(i) : null;
        }

        private void TextBox_FinishTextChanged()
        {
            // Don't bother if the document is null.
            if (txtCode.Document == null)
                return;

            // We want to make sure we only act upon the section where it was changed.
            //HighlightStart = txtCode.Document.ContentStart.GetPositionAtOffset(e.Changes.FirstOrDefault().Offset);
            //HighlightEnd = txtCode.Document.ContentStart.GetPositionAtOffset(e.Changes.Last().Offset);

            // Make sure we remove the text changed event, so that we don't cause an endless loop!
            txtCode.TextChanged -= TextBox_TextChanged;

            // Make sure that we remove all styling to where we want to highlight - we'll color it ourselves.
            new TextRange(HighlightStart, HighlightEnd).ClearAllProperties();

            // Highlight the current text.
            HighlightText();

            // Add the event back, since we're finished.
            txtCode.TextChanged += TextBox_TextChanged;
        }

        ///// <summary>
        ///// Measure the height of the code
        ///// </summary>
        ///// <returns>The height of the code</returns>
        //private double MeasureCodeHeight()
        //{
        //    var formattedText = new FormattedText(
        //        txtCode.Text,
        //        CultureInfo.CurrentCulture,
        //        FlowDirection.LeftToRight,
        //        new Typeface(txtCode.FontFamily, txtCode.FontStyle, txtCode.FontWeight, txtCode.FontStretch),
        //        txtCode.FontSize,
        //        Brushes.Black,
        //        new NumberSubstitution());

        //    return formattedText.Height + txtCode.FontSize * 2;
        //}

        /// <summary>
        /// The current position of the highlighter in its run.
        /// </summary>
        internal int currentPos;

        /// <summary>
        /// The last run that the highlighter was on.
        /// </summary>
        internal Run lastRun;

        public void HighlightText()
        {
            // Clear out the old highlights within the correct block.
            highlights.RemoveAll(i => HighlightStart.CompareTo(i.Range.Start) >= 0 && HighlightEnd.CompareTo(i.Range.Start) < 0);

            // Create a pointer to the start - which we will increment.
            CurrentPointer = HighlightStart;

            // Initilize the parser.
            TextHighlighter.Init();

            // Set the textbox within the parser to this textbox.
            TextHighlighter.TextBox = this;

            // Reset text for parser instead of setting, b/c we'll dynamically set it as we come across each run,
            // so that the whitespace detection (which allows anything at the end) will allow tokens at the end of a run!
            //TextHighlighter.Text = ABParse.ABParser.ToCharArray(TextNoLineBreaks);
            TextHighlighter.Text = new char[TextNoLineBreaks.Length];

            // Move forward through all the symbols, and act where all the text is...
            while (CurrentPointer.CompareTo(HighlightEnd) < 0)
            {
                // We're going to use ABParser, which is another ABSoftware product capable of parsing a test JSON string (55 characters) within 0.0232ms!
                // However, we can't use it the way it's intended (with a string) because this RichTextBox has a BUNCH of other symbols added.
                // So, instead... We're going to manually call to method that processes each character... When we're actually at a piece of text!

                // Get the current context - this is essentially what this symbol means.
                var context = CurrentPointer.GetPointerContext(LogicalDirection.Backward);

                // If this is a run - that's where the text is so we can process it!
                if (context == TextPointerContext.ElementStart && CurrentPointer.Parent is Run run)
                {
                    // Just in case the "Highlight" methods execute too late, we'll store this run in a variable.
                    lastRun = run;

                    // Add this run as text to the parser - the parser works with character arrays so we need to make sure we convert it.
                    ABParse.ABParser.ToCharArray(run.Text).CopyTo(TextHighlighter.Text, TextHighlighter.CurrentLocation);

                    // Go through all the text in this run - this is where the heart of the highlighter is.
                    for (currentPos = 0; currentPos < run.Text.Length; currentPos++)
                    {
                        // Process this character.
                        var result = TextHighlighter.ProcessChar(true);

                        // Also, we passed false to the process character method because we wanted the events to execute AFTER getting
                        // the start of the token, so, NOW we'll check the events - if we should.
                        //if (!result)
                            //TextHighlighter.ProcessBuiltUpTokens(false);

                        // Make sure we move on the next character in the parser - if this isn't the last one.
                        if (TextHighlighter.CurrentLocation + 1 < TextHighlighter.Text.Length)
                            TextHighlighter.MoveForward();

                    }

                    // We need to store the end of this run - so that the whitespace detection can allow tokens at start/ends of runs.
                    TextHighlighter.RunStart = TextHighlighter.CurrentLocation;

                    // Now that we've finished with this run - which represent lines in our case, make sure we aren't highlighting the line anymore.
                    TextHighlighter.LineHighlighted = false;

                    // The currentPos is now one character too far ahead - so, if this is the end of the string, shift it back one.
                    //if (TextHighlighter.CurrentLocation + 1 == TextHighlighter.Text.Length)
                    //    currentPos--;
                }

                // Go to the next insertion point if it isn't at a valid one.
                CurrentPointer = CurrentPointer.GetNextContextPosition(LogicalDirection.Forward);
            }

            // Now that we're completely finished, we haven't chosen a start/emd yet.
            chosenStartEnd = false;

            // Make sure we clean it up afterwards
            TextHighlighter.ProcessBuiltUpTokens(false);

            // We also want to reset all the variables ready for the next go.
            TextHighlighter.Stop();

            // Now, apply all the highlights that were created - they had to be put into an array, because,
            // otherwise, the runs start to change while we're highlighting... which isn't what we want.
            ApplyHighlights();

            //// Create a navigator to go through the text.
            //var navigator = txtCode.Document.ContentStart;

            //// Go through all the text.
            //while (navigator.CompareTo(txtCode.Document.ContentEnd) < 0)
            //{
            //    // Get a context of where the navigator is.
            //    var context = navigator.GetPointerContext(LogicalDirection.Backward);

            //    // If we're at the start of a run, start highlighting.
            //    if (context == TextPointerContext.ElementStart && navigator.Parent is Run)
            //        HighlightRun(navigator.Parent as Run);

            //    // Go to the next character.
            //    navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
            //}
        }

        private void ApplyHighlights()
        {
            // Go through each highlight, applying each one within the range we changed.
            for (int i = 0; i < highlights.Count; i++)
                if (highlights[i].Range.Start.CompareTo(HighlightStart) > 0 && highlights[i].Range.End.CompareTo(HighlightEnd) < 0)

                    // Now, actually apply the color!
                    highlights[i].Range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(highlights[i].Color));
        }

        //internal void HighlightLine(Color clr, int startIndex)
        //{
        //    // Find where the next newline will be located.
        //    var nextNewLine = Text.IndexOf('\n', startIndex);

        //    // Find how many character is will take to get there.
        //    var length = nextNewLine - startIndex;

        //    // If it the length is zero - it must have failed to find a new line - so we'll work to the end of the string.
        //    if (length == 0)
        //        length = Text.Length - startIndex;

        //    // Now, actually highlight the text.
        //    Highlight(clr);
        //}

        internal void Highlight(Color clr)
        {
            // Create a highlight with the correct range and add it to the array - the TextRange represents what part of the RichTextBox to highlight.
            highlights.Add(new CodeTextBoxHighlight(new TextRange(StartPointer, lastRun.ContentStart.GetPositionAtOffset(currentPos)), clr));
        }

        internal void HighlightLine(Color clr)
        {
            // Get the where the next line start - this will be null if there is no next line, in which case it will use the document's end.
            var nextStart = StartPointer.GetLineStartPosition(1);

            // Create a highlight up from the start to the end of the line - the TextRange represents where the RichTextBox gets highlighted.
            highlights.Add(new CodeTextBoxHighlight(new TextRange(StartPointer, (nextStart ?? txtCode.Document.ContentEnd).GetInsertionPosition(LogicalDirection.Backward)), clr));
        }

        private void txtCode_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Make sure it is scrolled to the right point
            LineNumbersScroll.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void txtCode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // If we pressed the TAB key - insert four spaces in the selection spot.
            if (e.Key == System.Windows.Input.Key.Tab)

                // Get a TextPointer to the caret's position and insert the text into that.
                txtCode.CaretPosition.GetInsertionPosition(LogicalDirection.Forward).InsertTextInRun("    ");

        }
        #endregion

        //private int GetTextBoxTextHeight(TextBox tBox)
        //{
        //    return TextRenderer.MeasureText(tBox.Text, tBox.Font, tBox.ClientSize,
        //             TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
        //}
    }
}
