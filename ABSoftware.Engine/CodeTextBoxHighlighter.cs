using ABParse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace ABSoftware.Engine
{
    public class CodeTextBoxHighlighter : ABParser
    {
        public CodeTextBox TextBox = null;
        public virtual char BlockStart { get; }
        public virtual char BlockEnd { get; }

        /// <summary>
        /// Represents whether this line is already highlighted or not.
        /// </summary>
        internal protected bool LineHighlighted;

        /// <summary>
        /// The characters that can a token can be surronded by for <see cref="SurroundedByWhiteSpaceOrDelimiter"/> to pass (what should be ran on a keyword).
        /// </summary>
        public char[] Delimiters;

        /// <summary>
        /// The offset that is used when highlighting - because, in order to keep the TextPointer at the right location - we have account for all the other runs.
        /// </summary>

        /// <summary>
        /// Gets the start of the last run - used for whitespace allowance/detection
        /// </summary>
        internal int RunStart;

        public CodeTextBoxHighlighter()
        {
            UseMoveForwardToChangePosition = true;

            PossibleTokenStartChanged = () =>
            {
                if (PossibleTokenStart != -1)
                // Get the TextPointer for this.
                    TextBox.StartPointer = TextBox.lastRun.ContentStart.GetPositionAtOffset(TextBox.currentPos);// - ((TextBox.currentPos > 0) ? 1 : 0));
            };
        }

        ///// <summary>
        ///// Highlights the text at the current location UP TO END OF THIS LINE.
        ///// </summary>
        ///// <param name="clr"></param>
        //public void Highlight(int start, Color clr)
        //{
        //    textbox.HighlightLine(clr, start);
        //}

        /// <summary>
        /// Highlights the text at the current location up to the specified length.
        /// </summary>
        /// <param name="clr"></param>
        public void Highlight(Color clr)
        {
            TextBox.Highlight(clr);
        }

        /// <summary>
        /// Highlights the rest of the line.
        /// </summary>
        /// <param name="clr"></param>
        public void HighlightLine(Color clr)
        {
            TextBox.HighlightLine(clr);

            // Mark this line (or run technically) as being highlighted.
            LineHighlighted = true;
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Now that the parser has started, we'll need to make sure that this line is no longer highlighted.
            LineHighlighted = false;
        }

        public bool SurroundedByWhiteSpaceOrDelimiter()
        {
            // We'll split it into two parts the "before" and "after", if both pass, we return true.
            var before = false;
            var after = false;

            // If this is at the start, the "before" passes.
            if (PossibleTokenStart == 0)
                before = true;

            // If the token is at the end, the "after" passes.
            if (PossibleTokenEnd == Text.Length - 1)
                after = true;

            // From now on, only focus on ones (before/after) that haven't passed.

            // If there's whitespace before, it passes the "before" part.
            if (!before)
                if (char.IsWhiteSpace(Text[PossibleTokenStart - 1]))
                    before = true;

            // If there's whitespace after, it passes the "after" part.
            if (!after)
                if (char.IsWhiteSpace(Text[CurrentLocation]))
                    after = true;

            // If this is the start of a token, the "before" will pass.
            if (!before)
                if (PossibleTokenStart == RunStart)
                    before = true;

            // If there's an empty character after (this can only be after), it passes the "after" part.
            if (!after)
                if (CurrentLocation - 1 == RunStart)
                    after = true;

            // If there's a delimiter before, the "before" passes.
            if (!before && Delimiters != null)
                for (int i = 0; i < Delimiters.Length; i++)
                    if (Text[PossibleTokenStart - 1] == Delimiters[i])
                    {
                        before = true;
                        break;
                    }


            // If there's a delimiter after, the "after" passes.
            if (!after && Delimiters != null)
                for (int i = 0; i < Delimiters.Length; i++)
                    if (Text[(CurrentLocation)] == Delimiters[i])
                    {
                        after = true;
                        break;
                    }

            // Now, if both "before" and "after" passed, we're done!
            return (before && after);
        }

        //protected override void BeforeTokenProcessed(ABParserToken token)
        //{
        //    base.BeforeTokenProcessed(token);

        //    if (PossibleTokenStart != -1)
        //    {
        //        if (TextBox.CurrentPointer.Parent is FlowDocument)
        //            TextBox.StartPointer = (TextBox.CurrentPointer.Parent as FlowDocument).ContentStart.GetPositionAtOffset(PossibleTokenStart);
        //        else if (TextBox.CurrentPointer.Parent is Run)
        //            TextBox.StartPointer = (TextBox.CurrentPointer.Parent as Run).ContentStart.GetPositionAtOffset(PossibleTokenStart - startOffset);
        //    }
        //}

        //protected override void OnTokenProcessed(TokenProcessedEventArgs e)
        //{
        //    base.OnTokenProcessed(e);

        //    TextBox.CurrentPointer.GetNextInsertionPosition(LogicalDirection.Backward);
        //    shiftedPointerBack = true;
        //}
    }
}
