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
        private int _start;

        /// <summary>
        /// The offset that is used when highlighting - because, in order to keep the TextPointer at the right location - we have account for all the other runs.
        /// </summary>
        internal int offset;

        /// <summary>
        /// The start that has been currently decided.
        /// </summary>
        internal int TokenStart
        {
            get { return _start; }
            set
            {
                _start = value;
                TextBox.StartPointer = TextBox.CurrentPointer;
            }
        }

        public CodeTextBoxHighlighter()
        {
            UseMoveForwardToChangePosition = true;
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
