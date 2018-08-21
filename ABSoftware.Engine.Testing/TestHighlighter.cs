using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using ABParse;

namespace ABSoftware.Engine.Testing
{
    public class TestHighlighter : CodeTextBoxHighlighter
    {
        public override char BlockStart => '{';
        public override char BlockEnd => '}';

        public TestHighlighter()
        {
            // Set all the tokens for C#
            Tokens = new System.Collections.ObjectModel.ObservableCollection<ABParserToken>()
            {
                // Keywords
                new ABParserToken("as", 'a', 's'),
                new ABParserToken("do", 'd', 'o'),
                new ABParserToken("if", 'i', 'f'),
                new ABParserToken("in", 'i', 'n'),
                new ABParserToken("is", 'i', 's'),
                new ABParserToken("for", 'f', 'o', 'r'),
                new ABParserToken("int", 'i', 'n', 't'),
                new ABParserToken("new", 'n', 'e', 'w'),
                new ABParserToken("out", 'o', 'u', 't'),
                new ABParserToken("ref", 'r', 'e', 'f'),
                new ABParserToken("try", 't', 'r', 'y'),
                new ABParserToken("var", 'v', 'a', 'r'),
                new ABParserToken("base", 'b', 'a', 's', 'e'),
                new ABParserToken("bool", 'b', 'o', 'o', 'l'),
                new ABParserToken("byte", 'b', 'y', 't', 'e'),
                new ABParserToken("case", 'c', 'a', 's', 'e'),
                new ABParserToken("char", 'c', 'h', 'a', 'r'),
                new ABParserToken("else", 'e', 'l', 's', 'e'),
                new ABParserToken("enum", 'e', 'n', 'u', 'm'),
                new ABParserToken("goto", 'g', 'o', 't', 'o'),
                new ABParserToken("lock", 'l', 'o', 'c', 'k'),
                new ABParserToken("long", 'l', 'o', 'n', 'g'),
                new ABParserToken("null", 'n', 'u', 'l', 'l'),
                new ABParserToken("this", 't', 'h', 'i', 's'),
                new ABParserToken("true", 't', 'r', 'u', 'e'),
                new ABParserToken("uint", 'u', 'i', 'n', 't'),
                new ABParserToken("void", 'v', 'o', 'i', 'd'),
                new ABParserToken("async", 'a', 's', 'y', 'n', 'c'),
                new ABParserToken("await", 'a', 'w', 'a', 'i', 't'),
                new ABParserToken("break", 'b', 'r', 'e', 'a', 'k'),
                new ABParserToken("catch", 'c', 'a', 't', 'c', 'h'),
                new ABParserToken("class", 'c', 'l', 'a', 's', 's'),
                new ABParserToken("const", 'c', 'o', 'n', 's', 't'),
                new ABParserToken("event", 'e', 'v', 'e', 'n', 't'),
                new ABParserToken("false", 'f', 'a', 'l', 's', 'e'),
                new ABParserToken("fixed", 'f', 'i', 'x', 'e', 'd'),
                new ABParserToken("float", 'f', 'l', 'o', 'a', 't'),
                new ABParserToken("sbyte", 's', 'b', 'y', 't', 'e'),
                new ABParserToken("short", 's', 'h', 'o', 'r', 't'),
                new ABParserToken("throw", 't', 'h', 'r', 'o', 'w'),
                new ABParserToken("ulong", 'u', 'l', 'o', 'n', 'g'),
                new ABParserToken("using", 'u', 's', 'i', 'n', 'g'),
                new ABParserToken("while", 'w', 'h', 'i', 'l', 'e'),
                new ABParserToken("double", 'd', 'o', 'u', 'b', 'l', 'e'),
                new ABParserToken("extern", 'e', 'x', 't', 'e', 'r', 'n'),
                new ABParserToken("object", 'o', 'b', 'j', 'e', 'c', 't'),
                new ABParserToken("params", 'p', 'a', 'r', 'a', 'm', 's'),
                new ABParserToken("public", 'p', 'u', 'b', 'l', 'i', 'c'),
                new ABParserToken("return", 'r', 'e', 't', 'u', 'r', 'n'),
                new ABParserToken("sealed", 's', 'e', 'a', 'l', 'e', 'd'),
                new ABParserToken("sizeof", 's', 'i', 'z', 'e', 'o', 'f'),
                new ABParserToken("static", 's', 't', 'a', 't', 'i', 'c'),
                new ABParserToken("string", 's', 't', 'r', 'i', 'n', 'g'),
                new ABParserToken("struct", 's', 't', 'r', 'u', 'c', 't'),
                new ABParserToken("switch", 's', 'w', 'i', 't', 'c', 'h'),
                new ABParserToken("typeof", 't', 'y', 'p', 'e', 'o', 'f'),
                new ABParserToken("unsafe", 'u', 'n', 's', 'a', 'f', 'e'),
                new ABParserToken("ushort", 'u', 's', 'h', 'o', 'r', 't'),
                new ABParserToken("checked", 'c', 'h', 'e', 'c', 'k', 'e', 'd'),
                new ABParserToken("decimal", 'd', 'e', 'c', 'i', 'm', 'a', 'l'),
                new ABParserToken("default", 'd', 'e', 'f', 'a', 'u', 'l', 't'),
                new ABParserToken("finally", 'f', 'i', 'n', 'a', 'l', 'l', 'y'),
                new ABParserToken("foreach", 'f', 'o', 'r', 'e', 'a', 'c', 'h'),
                new ABParserToken("private", 'p', 'r', 'i', 'v', 'a', 't', 'e'),
                new ABParserToken("virtual", 'v', 'i', 'r', 't', 'u', 'a', 'l'),
                new ABParserToken("abstract", 'a', 'b', 's', 't', 'r', 'a', 'c', 't'),
                new ABParserToken("continue", 'c', 'o', 'n', 't', 'i', 'n', 'u', 'e'),
                new ABParserToken("delegate", 'd', 'e', 'l', 'e', 'g', 'a', 't', 'e'),
                new ABParserToken("explicit", 'e', 'x', 'p', 'l', 'i', 'c', 'i', 't'),
                new ABParserToken("implicit", 'i', 'm', 'p', 'l', 'i', 'c', 'i', 't'),
                new ABParserToken("internal", 'i', 'n', 't', 'e', 'r', 'm', 'a', 'l'),
                new ABParserToken("operator", 'o', 'p', 'e', 'r', 'a', 't', 'o', 'r'),
                new ABParserToken("override", 'o', 'v', 'e', 'r', 'r', 'i', 'd', 'e'),
                new ABParserToken("readonly", 'r', 'e', 'a', 'd', 'o', 'n', 'l', 'y'),
                new ABParserToken("volatile", 'v', 'o', 'l', 'a', 't', 'i', 'l', 'e'),
                new ABParserToken("__arglist", '_', '_', 'a', 'r', 'g', 'l', 'i', 's', 't'),
                new ABParserToken("__makeref", '_', '_', 'm', 'a', 'k', 'e', 'r', 'e', 'f'),
                new ABParserToken("__reftype", '_', '_', 'r', 'e', 'f', 't', 'y', 'p', 'e'),
                new ABParserToken("interface", 'i', 'n', 't', 'e', 'r', 'f', 'a', 'c', 'e'),
                new ABParserToken("namespace", 'n', 'a', 'm', 'e', 's', 'p', 'a', 'c', 'e'),
                new ABParserToken("protected", 'p', 'r', 'o', 't', 'e', 'c', 't', 'e', 'd'),
                new ABParserToken("unchecked", 'u', 'n', 'c', 'h', 'e', 'c', 'k', 'e', 'd'),
                new ABParserToken("__refvalue", '_', '_', 'r', 'e', 'f', 'v', 'a', 'l', 'u', 'e'),
                new ABParserToken("stackalloc", 's', 't', 'a', 'c', 'k', 'a', 'l', 'l', 'o', 'c'),

                // Syntax
                new ABParserToken("SingleQuoteString", '\''),
                new ABParserToken("DoubleQuoteString", '"'),

                // Numbers
                new ABParserToken("Numerical0", '0'),
                new ABParserToken("Numerical1", '1'),
                new ABParserToken("Numerical2", '2'),
                new ABParserToken("Numerical3", '3'),
                new ABParserToken("Numerical4", '4'),
                new ABParserToken("Numerical5", '5'),
                new ABParserToken("Numerical6", '6'),
                new ABParserToken("Numerical7", '7'),
                new ABParserToken("Numerical8", '8'),
                new ABParserToken("Numerical9", '9'),

                // Comments
                new ABParserToken("SingleLineComment", '/', '/'),
                new ABParserToken("MultiLineCommentStart", '/', '*'),
                new ABParserToken("MultiLineCommentEnd", '*', '/')
                //new ABParserToken("public", 'p', 'u', 'b', 'l', 'i', 'c'),
                //new ABParserToken("static", 's', 't', 'a', 't', 'i', 'c'),
                //new ABParserToken("void", 'v', 'o', 'i', 'd'),
            };

            // Set all the delimiters for C# - to allow keyword detection to work better.
            Delimiters = new char[] { ';', ',', '.', '+', '-', '*', '/', '=', '%', '\'', '"', '[', ']', '{', '}', '(', ')' };
        }

        public override bool NotifyCharacterProcessed => true;

        public bool InString = false;
        public bool InComment = false;
        public TextPointer ActualStart;

        protected override void OnStart()
        {
            InString = false;
            InComment = false;
        }

        //protected override void OnCharacterProcessed(char ch)
        //{
        //    // If it's a number - color it!
        //    if (char.IsNumber(ch))
        //        Highlight(Colors.YellowGreen);
        //}

        protected override void BeforeTokenProcessed(ABParserToken token)
        {
            base.BeforeTokenProcessed(token);

            // If this line is already highlighted (usually comments), don't bother with highlighting it.
            if (LineHighlighted)
                return;

            // If we're in a multi-line comment, then color it and exit it - since this token can only be a multi-line comment ending, because of the limit.
            if (InComment)
            {
                // Set the start to where the comment actually started - but, go back one if needed
                TextBox.StartPointer = ActualStart;

                // Color it in the green colour.
                Highlight(Color.FromArgb(255, 87, 166, 74));

                // Exit the comment
                InComment = false;
            }

            // If we're in a string, then color it and exit it - since this token can only be a string because of the limit.
            else if (InString)
            {
                // Set the start to where the string actually started.
                TextBox.StartPointer = ActualStart;

                // Color it in red now.
                Highlight(Colors.OrangeRed);

                // Exit the string
                InString = false;
            }

            // If we aren't in a string.
            else
            {
                if (token.Name.StartsWith("Numerical")) // Number

                    // Highlight the number YellowGreen.
                    Highlight(Colors.YellowGreen);
                else if (token.Name.EndsWith("String")) // String/Char
                {

                    // Set a limit so that only the string token matters and make sure that limit expires at the next string token.
                    TokenLimit = new System.Collections.ObjectModel.ObservableCollection<char[]>() { token.Token };
                    LimitAffectsNextTrailing = false;

                    // Make sure that we keep hold of the current StartPointer - so that we can use it as the start of the string.
                    ActualStart = TextBox.StartPointer;

                    // Mark us as being in a string, so that the next time it can apply it!
                    InString = true;
                }

                else if (token.Name == "SingleLineComment") // Single Line Comment
                    HighlightLine(Color.FromArgb(255, 87, 166, 74));

                else if (token.Name == "MultiLineCommentStart") // Multi-Line Comment
                {
                    // Set a limit so that only the multi-line comment ending token matters.
                    TokenLimit = new System.Collections.ObjectModel.ObservableCollection<char[]>() { new char[]{ '*', '/' } };
                    LimitAffectsNextTrailing = false;

                    // Make sure that we keep hole the current StartPointer - so that we can use it as the start of the multi-line comment.
                    ActualStart = TextBox.StartPointer;

                    // Mark us as being in a multi-line comment, so that the next time it will finish the comment!
                    InComment = true;
                }

                // Highlight it blue - but only if there's whitespace around it, or
                // a null character after, meaning it's the end of a run (this stops things like "int in = 0")
                else if (SurroundedByWhiteSpaceOrDelimiter())

                    // Color it blue!
                    Highlight(Colors.Blue);
            }
        }
    }
}
