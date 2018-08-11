using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
        /// If, when highlighting, we've decided on a start for a highlight.
        /// </summary>
        internal bool decidedStart = false;

        /// <summary>
        /// All of the locations that are highlighted.
        /// </summary>
        internal List<CodeTextBoxHighlight> highlights = new List<CodeTextBoxHighlight>();

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
        #endregion

        #region Managed Methods
        public CodeTextBox()
        {
            InitializeComponent();

            LineNumbersScroll.CanContentScroll = true;
        }

        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Set the line numbers now.
            // The total number of lines.
            var lineCount = LineCount;

            // We only want to change the line numbers if the totaline number has changed.
            if (lineCount != lastLineCount)
            {

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

            // If we are already waiting a second for the FinishTextChanged event.
            if (alreadyWaiting)
                return;
            else
            {
                // Make it so we're waiting.
                alreadyWaiting = true;

                // Wait a second - before executing the FinishTextChanged event.
                await Task.Delay(1000);

                // We aren't waiting anymore
                alreadyWaiting = false;

                // Call to the finish text changed event - which happens every second of typing.
                TextBox_FinishTextChanged(sender, e);
            }
        }

        private void TextBox_FinishTextChanged(object sender, TextChangedEventArgs e)
        {
            // The process is fairly simple for the line numbers.
            // Just add a massive TextBlock, with all the line numbers.
            // And then sync up its scroll with the scroll of the TextBox.

            // Don't bother if the document is null.
            if (txtCode.Document == null)
                return;

            // Make sure we remove the text changed event, so that we don't cause an endless loop!
            txtCode.TextChanged -= TextBox_TextChanged;

            // Make sure that we work in just plaintext - we'll color it ourselves.
            new TextRange(txtCode.Document.ContentStart, txtCode.Document.ContentEnd).ClearAllProperties();

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

        public void HighlightText()
        {
            // We'll want to make sure we keep track of the index of the run we're on.
            var rIndex = 0;

            // Clear out the old highlights
            highlights.Clear();

            // Create a pointer to the start - which we will increment.
            CurrentPointer = txtCode.Document.ContentStart;

            // Initilize the parser.
            TextHighlighter.Init();

            // Set the textbox within the parser to this textbox.
            TextHighlighter.TextBox = this;

            // Set the text for the highlighter, the parser deals with it as a character array - so we have to make sure we convert the string to that.
            TextHighlighter.Text = ABParse.ABParser.ToCharArray(Text);

            // Move forward through all the symbols, and act where all the text is...
            while (CurrentPointer != null && CurrentPointer.CompareTo(txtCode.Document.ContentEnd) < 0)
            {
                // We're going to use ABParser, which is another ABSoftware product capable of parsing a test JSON string (55 characters) within 0.0232ms!
                // However, we can't use it the way it's intended (with a string) because this RichTextBox has a BUNCH of other symbols added.
                // So, instead... We're going to manually call to method that processes each character... When we're actually at a piece of text!

                // Get the current context - this is essentially what this symbol means.
                var context = CurrentPointer.GetPointerContext(LogicalDirection.Backward);

                // If this is a piece of text - we can process it.
                if (context == TextPointerContext.ElementStart && CurrentPointer.Parent is Run)
                {
                    // We'll store the current run so that we can detect when we've gone onto another run.
                    var lastPos = CurrentPointer;

                    // We need to move the pointer backward based on the index of the current run.
                    //for (int i = 0; i < rIndex; i++)
                    //    CurrentPointer = CurrentPointer.GetNextInsertionPosition(LogicalDirection.Backward);

                    // Store the length in a variable to add to the offset at the end of this run.
                    //var length = (CurrentPointer.Parent as Run).Text.Length;

                    // Process each character while we're still in a run.
                    while (CurrentPointer != null && CurrentPointer.Parent is Run)
                    {
                        // Process this character.
                        var result = TextHighlighter.ProcessChar(false);

                        // If it's decided on a token start (it isn't its "unset" value)... Make sure we set THIS offset as the start offset.
                        if (TextHighlighter.PossibleTokenStart != -1 && !decidedStart)
                        {
                            // In order to stop the start being set every character, we'll say that we've decided on a start.
                            decidedStart = true;

                            // Make sure we set the start of the token.
                            TextHighlighter.TokenStart = TextHighlighter.PossibleTokenStart;
                        }

                        // If, however, it is unset again, and we had already decided on a start... Then that start isn't valid anymore
                        else if (TextHighlighter.PossibleTokenStart == -1 && decidedStart)
                            decidedStart = false;

                        // The events require the pointer to be one character backwards - if it can be.
                        //if (TextHighlighter.CurrentLocation - offset - 1 >= 0)
                        //CurrentPointer = (CurrentPointer.Parent as Run).ContentStart.GetPositionAtOffset(TextHighlighter.CurrentLocation - offset - 1);

                        // Also, we passed false to the process character method because we wanted the events to execute AFTER getting
                        // the start of the token, so, NOW we'll check the events - if we should.
                        if (!result)
                            TextHighlighter.ProcessBuiltUpTokens(true);

                        // If the pointer was shifted back in that - shift it forward again.
                        //if (TextHighlighter.shiftedPointerBack)
                        //{
                        //    CurrentPointer = CurrentPointer.GetNextInsertionPosition(LogicalDirection.Forward);
                        //    TextHighlighter.shiftedPointerBack = false;
                        //}

                        // Make sure we move on the next character in the parser.
                        TextHighlighter.MoveForward();

                        // Just in case the CurrentPointer becomes null, we'll store a copy of it before.
                        var oldPointer = CurrentPointer;

                        // We also have to make sure the our pointer moves forward, as that's used for the start/end position and has to be accurate.
                        CurrentPointer = CurrentPointer.GetNextInsertionPosition(LogicalDirection.Forward);

                        // If the above caused CurrentPointer to become null, then we'll reset it back to what it was before and exit the loop.
                        if (CurrentPointer == null)
                        {
                            CurrentPointer = oldPointer;
                            break;
                        }

                        // Get the gap of symbols between the current position and the last.
                        var offset = CurrentPointer.GetOffsetToPosition(lastPos);

                        // If we've moved ahead more than a "symbol", make sure we shift the highlighter ahead the correct amount.
                        if (Math.Abs(CurrentPointer.GetOffsetToPosition(lastPos)) > 1)
                        {
                            // Move the parser forward by the correct amount, in order to make up for any skipped characters.
                            for (int i = 0; i < offset / 2; i++)
                                CurrentPointer = CurrentPointer.GetNextInsertionPosition(LogicalDirection.Backward);
                        }

                        // Set the "old" position as the current one ready for the next character.
                        lastPos = CurrentPointer;
                    }

                    // Shift the current index forward.
                    rIndex++;

                    // Increment the offset, based on the length
                    //IncrementOffset(length);
                }

                //// However, if this is the end of an empty paragraph - we want to make sure we shift the offset forward a bit.
                //else if (CurrentPointer.Parent is Paragraph && (CurrentPointer.Parent as Paragraph).Inlines.Count == 0 && context == TextPointerContext.ElementStart)

                //    // Increment the offset forward by two.
                //    TextHighlighter.offset += 2;

                // Go to the next insertion point if it isn't at a valid one.
                CurrentPointer = CurrentPointer.GetNextInsertionPosition(LogicalDirection.Forward);
            }

            // In order to be ready for the next highlighting process, we need to make sure that it hasn't decided the start anymore.
            decidedStart = false;

            // Reset the offset.
            TextHighlighter.offset = 0;

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

        private void IncrementOffset(int length)
        {
            // Increment the offset by the correct amount - we also have to add two to it, because 
            // the runs actually ignore the \r\n at the end, and so it's all wrong without that counting that.
            // But, only do that if this run has something in it.
            if (length > 0) TextHighlighter.offset += length + 2;
        }

        private void ApplyHighlights()
        {
            // Go through each highlight, applying each one.
            for (int i = 0; i < highlights.Count; i++)
            {
                // Now, actually apply the color!
                highlights[i].Range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(highlights[i].Color));
            }
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
            // Don't bother if the end is null.
            if (CurrentPointer == null)
                return;

            // Create a highlight with the correct range and add it to the array - the TextRange represents what part of the RichTextBox to highlight.
            highlights.Add(new CodeTextBoxHighlight(new TextRange(StartPointer, CurrentPointer), clr));

            // Now that we're finished with that highlight we don't want the main highlighter to
            // think that it has the correct start for the next one as well.
            decidedStart = false;
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
