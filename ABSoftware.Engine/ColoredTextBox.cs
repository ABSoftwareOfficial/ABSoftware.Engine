// Experimental Project
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Media;
//using System.Collections.Specialized;
//using System.Collections;

//namespace ABSoftware.Engine
//{
//    /// <summary>
//    /// A normal TextBox - but with the capability to have colored text!
//    /// </summary>
//    public class ColoredTextBox : TextBox
//    {
//        public new Brush Foreground = Brushes.Black;

//        private ColoredTextBoxAdorner _customLayer;
//        private string _lastText = "";

//        // Quickly store the width of a dot, for the current font.
//        internal bool canUseCacheDotWidth = false;
//        internal double cacheDotWidth = 0;

//        public ColoredTextBox()
//        {
//            // Make sure you can press RETURN and TAB.
//            AcceptsReturn = true;
//            AcceptsTab = true;

//            // Make sure the right scroll bar is always visible.
//            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
//            VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

//            // Set the foreground for the TextBox behind to Transparent - because, we want to draw the text ourselves!
//            base.Foreground = Brushes.Transparent;

//            // Add an event handler for to scroll - this will just redraw the text when you scroll.
//            AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChanged));

//            // Add the first item.
//            Sections.Add(new ABTextBoxSection(0, 0, Foreground));
//        }

//        //public OrderedDictionary CustomDefinedColors
//        //{
//        //    get { return _customDefinedColors; }
//        //    internal set
//        //    {
//        //        _customDefinedColors = value;
//        //        OrganizeColors();
//        //    }
//        //}

//        //public List<StringSpan> UncoloredSections {
//        //    get { return _uncoloredSections; }
//        //    set
//        //    {
//        //        _uncoloredSections = value;
//        //        _canUseCache = false;
//        //    }
//        //}

//        //public OrderedDictionary AllSections
//        //{
//        //    get
//        //    {
//        //        // If this has already been calculated, and we CAN use that already calculated one - just return that!
//        //        if (_canUseCache)
//        //            return _cache;

//        //        // Otherwise, we have to calculate it again.
//        //        else
//        //        {
//        //            // Combine the two dictionaries
//        //            var uncoloredSections = UncoloredSections.ToDictionary(pair => pair, v => Foreground);
//        //            var combined = CustomDefinedColors.Cast<KeyValuePair<StringSpan, Brush>>().Union(uncoloredSections.Cast<KeyValuePair<StringSpan, Brush>>());

//        //            // Order them, based on their span
//        //            var final = combined.OrderByStart();

//        //            // Set this as the cache, for future use.
//        //            _canUseCache = true;
//        //            _cache = final;

//        //            // Return the final calculated result.
//        //            return final;
//        //        }
//        //    }
//        //}

//        public List<ABTextBoxSection> Sections { get; private set; } = new List<ABTextBoxSection>();

//        public override void OnApplyTemplate()
//        {
//            base.OnApplyTemplate();

//            // Create our own layer
//            var layer = AdornerLayer.GetAdornerLayer(this);
//            _customLayer = new ColoredTextBoxAdorner(this);

//            // Add our own layer.
//            layer.Add(_customLayer);
//        }

//        protected override void OnTextChanged(TextChangedEventArgs e)
//        {
//            // We need recalculate the end of the color we are currently in (and change all the others as well)
//            // Find out how many characters were added/removed
//            //ModifyLength = Text.Length - _lastText.Length;

//            // Right, so the process is actually kind of complicated - don't forget that we're having to work off of just HOW MANY characters were removed/added together...
//            // So, this is how we figure out how many characters to add/remove and to what section.
//            // Essentially, we find the number of characters we have to work with (between the start of the change and the end of the group)
//            // 
//            // First of all, we find out WHICH section is the correct on to start on...

//            // Then (this is the clever part), we take away the removed characters from that number... 
//            // however - it can't go into minus numbers, and if it does we only take away the amount we can without doing that.

//            // After that, we just add all of the added characters to the first one (there is physically NO WAY of us knowing which section those were added to)

//            // And now, we go through each section after, and remove some characters if we weren't able to, as well as add some characters, while also adjusting the start and end based on the changes made to the ones before them.

//            // Store the total number of sections.
//            var count = Sections.Count;

//            // Now, we need to go through every change and change things accordingly...
//            for (int i = 0; i < e.Changes.Count; i++)
//            {
//                var item = e.Changes.ElementAt(i);

//                // The amount of characters changed in the found section
//                var amountChanged = item.AddedLength - item.RemovedLength;

//                // The index of the section that this change starts at.
//                var index = 0;

//                // The amount of space we have in this section...
//                var space = 0;

//                // The remaining amount of characters left.
//                var remainingRemoved = item.RemovedLength;

//                // Find which section the change starts in.
//                for (int j = 0; j < Sections.Count; j++)
//                {
//                    // Check if it is the right section...
//                    if (Sections[j].Start <= item.Offset && Sections[j].End + amountChanged >= item.Offset + amountChanged)
//                    {
//                        // Set the correct amount of space.
//                        space = Sections[j].End - item.Offset;

//                        // Set this as the correct index for the start section.
//                        index = j;

//                        // Add on the added characters
//                        Sections[j].End += item.AddedLength;

//                        // Change the end...
//                        if (remainingRemoved > space)
//                        {
//                            // If we don't have enough space (in this section) to remove all the remaining removed characters, just take away the amount we can.
//                            Sections[j].End -= space;
//                            remainingRemoved -= space;
//                        } else {
//                            // If we do have enough space, however, take it all away.
//                            Sections[j].End -= remainingRemoved;
//                            remainingRemoved = 0;
//                        }

//                        // Remove the section if needed.
//                        if (Sections[j].Start > Sections[j].End)
//                            Sections.RemoveAt(j);

//                        // Break out of the loop - since we've found what we needed
//                        break;
//                    }
//                    //remaining -= s.Key.End - (s.Key.End - amountChanged);
//                }

//                // We need to change all of the other elements, to align everything!
//                for (int j = index + 1; j < Sections.Count; j++)
//                {
//                    // Ignore elements with start/end of (0, 0)
//                    if (Sections[j].Start == 0 && Sections[j].End == 0)
//                        continue;

//                    // Set the correct amount of space.
//                    space = Sections[j].End - Sections[j].Start;

//                    // Make sure we adjust the start - this is for ALL sections, even ones that aren't in range of the changes.
//                    Sections[j].Start += item.AddedLength - (item.RemovedLength - remainingRemoved);
//                    Sections[j].End += item.AddedLength - (item.RemovedLength - remainingRemoved);

//                    // Change the end if any deleted characters are remaining...
//                    if (remainingRemoved > space)
//                    {
//                        // If we don't have enough space (in this section) to remove all the remaining removed characters, just take away the amount we can.
//                        Sections[j].End -= space;
//                        remainingRemoved -= space;
//                    }
//                    else
//                    {
//                        // If we do have enough space, however, take it all away.
//                        Sections[j].End -= remainingRemoved;
//                        remainingRemoved = 0;
//                    }

//                    // Remove the section if needed.
//                    if (Sections[j].Start >= Sections[j].End)
//                        Sections.RemoveAt(j);
//                }


//                //// We need to change all of the other elements, in case the text that was changed goes into some other colours!
//                //for (int j = index + 1; j < allSections.Count(); j++)
//                //{
//                //    // Get the current section.
//                //    var s = allSections.ElementAt(j);

//                //    // Don't bother if the number of changed characters doesn't reach this section.
//                //    if (item.Offset + amountChanged < s.Key.Start)
//                //        continue;

//                //    if (found)
//                //    {
//                //        // Update all the values of this section.
//                //        section.Key.Start += amountChanged;
//                //        section.Key.End += amountChanged;
//                //    }
//                //    else
//                //    {



//                //        // Update the end of the one we are in.
//                //        section.Key.End += amountChanged;

//                //        // Mark the element.
//                //        found = true;

//                //        // Remove the section entirely if the entire section's characters are gone.
//                //        if (section.Key.End - section.Key.Start <= 0)
//                //        {
//                //            _customDefinedColors.Remove(new StringSpan(section.Key.Start, section.Key.End - amountChanged));
//                //            _uncoloredSections.Remove(new StringSpan(section.Key.Start, section.Key.End - amountChanged));
//                //        }
//                //    }
//                //}
//            }

//            // Force it to redraw
//            _customLayer.InvalidateVisual();

//            // Set the "_lastText" property - to let the next TextChanged event detect how many characters were added/removed
//            //_lastText = Text;

//            base.OnTextChanged(e);
//        }

//        protected void OnScrollChanged(object sender, ScrollChangedEventArgs e)
//        {
//            _customLayer.InvalidateVisual();
//        }

//        public void ChangeSelectionBrush(Brush newBrush)
//        {
//            ColorRange(SelectionStart, SelectionStart + SelectionLength, newBrush);
//        }

//        public void ColorRange(int start, int end, Brush newBrush)
//        {
//            // We need to reverse the CustomDefinedColors.
//            //var reversed = CustomDefinedColors.Reverse();

//            // Make sure we clean up all the currently defined colors.
//            for (int i = Sections.Count - 1; i >= 0; i--)
//            {
//                // Get the current element.
//                var ele = Sections[i];

//                // We only want to act if this section is a custom-defined section.
//                //if (Sections[i].Color == Foreground)
//                //    continue;

//                // Used to check if any elements have been added.
//                var beforeCount = Sections.Count;

//                // Check if anywhere INSIDE our range, there is a custom color.
//                if (ele.Start >= start && ele.End <= end)
//                {
//                    // Remove it.
//                    Sections.RemoveAt(i);

//                    // Don't bother with the rest - since that's taken care of.
//                    continue;
//                }

//                // If it crosses after the end - add a new one.
//                if (end > ele.Start && ele.End > end)
//                    Sections.Add(new ABTextBoxSection(end, ele.End, ele.Color));

//                // If this is the first character, don't bother with splitting at the start
//                if (start == 0)
//                {
//                    // Remove the original one - if anything has changed.
//                    if (Sections.Count > beforeCount)
//                        Sections.RemoveAt(i);

//                    continue;
//                }

//                // If it crosses before the start (and we know it's not inside it) - add a new one.
//                if (start > ele.Start && ele.End > start)
//                    Sections.Add(new ABTextBoxSection(ele.Start, start, ele.Color));

//                // Remove the original one - if anything has changed.
//                if (Sections.Count > beforeCount)
//                    Sections.RemoveAt(i);
//            }

//            // Now that we've made sure everything is cleaned up, we can add the actual item.
//            Sections.Add(new ABTextBoxSection(start, end, newBrush));

//            // And since we are "Add"ing to it - the setter won't get called, so we'll make sure "OrganizeColors" gets dealt with.
//            OrganizeColors();

//            // Redraw
//            _customLayer.InvalidateVisual();
//        }

//        private void OrganizeColors()
//        {
//            // Put the colors in order.
//            Sections = Sections.OrderBy(t => t.Start).ToList();

//            // The last start/end of the last one.
//            int lastStart = 0;
//            int lastEnd = 0;

//            // Add all the uncolored sections within the text.
//            for (int i = 0; i < Sections.Count; i++)
//            {
//                // Get this section.
//                var ele = Sections[i];

//                // Remove this section if it's an uncolored one, and go to the next section.
//                //if (ele.Color == Foreground)
//                //{
//                //    Sections.RemoveAt(i);
//                //    continue;
//                //}

//                // Make sure we aren't at the very front, if we are, don't bother with adding uncolored sections before this.
//                if (ele.Start == 0)
//                {
//                    // Set the previous end.
//                    lastEnd = ele.End;

//                    // Go to the next section.
//                    continue;
//                }

//                // Add an uncolored section between this one and the previous one - if there is a gap.
//                if (ele.Start > lastEnd + 1)
//                    Sections.Add(new ABTextBoxSection(lastStart, lastEnd, Foreground));

//                // Set the previous end.
//                lastEnd = ele.End;
//            }

//            // Add an uncolored section right to the very end, based on the previous span - if it needs to.
//            if (lastEnd != Text.Length)
//                Sections.Add(new ABTextBoxSection(lastEnd, Text.Length, Foreground));
//        }
//    }

//    ///// <summary>
//    ///// Represents a span of text between two indexes.
//    ///// </summary>
//    //public class StringSpan
//    //{
//    //    public int Start;
//    //    public int End;

//    //    public StringSpan(int start, int end)
//    //    {
//    //        Start = start;
//    //        End = end;
//    //    }
//    //}

//    /// <summary>
//    /// Represents a section in a <see cref="ColoredTextBox"/>
//    /// </summary>
//    public class ABTextBoxSection
//    {
//        public int Start;
//        public int End;

//        public Brush Color;

//        public ABTextBoxSection(int start, int end, Brush clr)
//        {
//            Start = start;
//            End = end;

//            Color = clr;
//        }
//    }

//    /// <summary>
//    /// DO NOT USE THIS FOR ANYTHING ELSE OTHER THAN THE COLOREDTEXTBOX
//    /// </summary>
//    internal class ColoredTextBoxAdorner : Adorner
//    {
//        public ColoredTextBoxAdorner(UIElement adornedElement) : base(adornedElement)
//        {

//        }

//        protected override void OnRender(DrawingContext context)
//        {
//            // Get the actual ColoredTextBox we want to apply this to
//            var uiElement = ((ColoredTextBox)AdornedElement);

//            // A few variables to keep track of where things get drawn.
//            var CurrentLeft = (uiElement.Padding.Left + 2) - uiElement.HorizontalOffset;
//            var CurrentTop = (uiElement.Padding.Top + 2) - uiElement.VerticalOffset;

//            // Draw all of the sections.
//            for (int i = 0; i < uiElement.Sections.Count; i++)
//            {
//                // Get the key of this item.
//                var ele = uiElement.Sections[i];

//                // Don't draw it if it has an end of 0.
//                if (ele.End <= 0)
//                    continue;

//                // Get the text and its lines for this section.
//                var text = uiElement.Text.Substring(ele.Start, ele.End - ele.Start);
//                var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

//                // Draw a line, with the correct brush and location.
//                for (int j = 0; j < lines.Length; j++)
//                {
//                    // Create all of the format things for measuring the text.
//                    var measureFormattedText = FormatText(lines[j] + ".", uiElement);

//                    // Create all of the format things for DRAWING the text.
//                    var formattedText = FormatText(lines[j], uiElement);

//                    // Actually draw the text.
//                    Geometry textGeometry = formattedText.BuildGeometry(new Point(CurrentLeft, CurrentTop));
//                    context.DrawGeometry(ele.Color, new Pen(Brushes.Transparent, 0), textGeometry);

//                    // Update the locations... unless there are still more lines - in which case, we need to make sure we keep the "CurrentLine" reset.
//                    if (j < lines.Length - 1)
//                        CurrentLeft = (uiElement.Padding.Left + 2) - uiElement.HorizontalOffset;
//                    else
//                        CurrentLeft += measureFormattedText.Width - GetDotWidth(uiElement);

//                    // If there is another line, add on the height - otherwise, we'll want to keep it.
//                    if (j < lines.Length - 1)
//                        CurrentTop += measureFormattedText.Height;
//                }
//            }

//            // Cover up the text underneath with this
//            //context.DrawRoundedRectangle(uiElement.Background, null, new Rect(uiElement.Padding.Left + 7, uiElement.Padding.Top + 7, formattedText.Width - 5, formattedText.Height + 5), 5.0, 5.0);
            
//        }

//        public FormattedText FormatText(string text, ColoredTextBox uiElement)
//        {
//            // Return the formatted text.
//            return new FormattedText(
//                        text,
//                        CultureInfo.CurrentCulture,
//                        FlowDirection.LeftToRight,
//                        new Typeface(uiElement.FontFamily, uiElement.FontStyle, uiElement.FontWeight, uiElement.FontStretch),
//                        uiElement.FontSize,
//                        uiElement.Foreground);
//        }

//        public double GetDotWidth(ColoredTextBox uiElement)
//        {
//            if (uiElement.canUseCacheDotWidth)
//                return uiElement.cacheDotWidth;
//            else
//            {
//                uiElement.cacheDotWidth = FormatText(".", uiElement).Width;
//                uiElement.canUseCacheDotWidth = true;
//                return uiElement.cacheDotWidth;
//            }
//        }
//    }

//    //public static class OrderedDictionaryExtensions
//    //{
//    //    //Something that allows you to convert an array of DictionaryEntry to an OrderedDictionary
//    //    public static OrderedDictionary ToOrderedDictionary(this IOrderedEnumerable<DictionaryEntry> arr)
//    //    {
//    //        // Put this into an ordered dictionary.
//    //        var finalOrdered = new OrderedDictionary();

//    //        // Place the array items into the ordered dictionary
//    //        foreach (DictionaryEntry entry in arr)
//    //            finalOrdered.Add(entry.Key, entry.Value);

//    //        // Return it.
//    //        return finalOrdered;
//    //    }

//    //    public static OrderedDictionary OrderByStart(this IEnumerable<DictionaryEntry> dictEntry)
//    //    {
//    //        // The new array.
//    //        var result = new List<KeyValuePair<StringSpan, Brush>>();

//    //        // Go through each item and filter them out.
//    //        for (int i = 0; i < dictEntry.Count(); i++)
//    //            result.Add(new KeyValuePair<StringSpan, Brush>(dictEntry.ElementAt(i).Key as StringSpan, dictEntry.ElementAt(i).Value as Brush));

//    //        return result.OrderByStart();
//    //    }

//    //    public static OrderedDictionary OrderByStart(this IEnumerable<KeyValuePair<StringSpan, Brush>> dictEntry)
//    //    {
//    //        // How many numbers have been sorted so far...
//    //        var completed = 0;

//    //        // The final result that will be returned.
//    //        var result = new OrderedDictionary();

//    //        // Don't bother, if the array has nothing in it.
//    //        if (dictEntry.Count() == 0)
//    //            return result;

//    //        // Go through the whole array multiple times
//    //        for (int i = 0; i < dictEntry.Count(); i++)
//    //        {
//    //            // For each time, store which was the smallest one so far...
//    //            var smallest = -1;

//    //            // Make sure that for the smallest one, we also store what the key and value of it is... So that it can be added!
//    //            var smallestKey = new StringSpan(0, 0);
//    //            Brush smallestValue = null;

//    //            // Go through the whole array - starting at the numbers that haven't been organized.
//    //            for (int j = completed; j < dictEntry.Count(); j++)
//    //            {
//    //                // Get the data in this item.
//    //                var data = dictEntry.ElementAt(j);

//    //                // Get the start of the key in this item.
//    //                var value = (data.Key as StringSpan).Start;

//    //                // If this is smaller than the smallest one so far... Change the smallest one!
//    //                if (value > smallest)
//    //                {
//    //                    smallest = value;
//    //                    smallestKey = data.Key as StringSpan;
//    //                    smallestValue = data.Value as Brush;
//    //                }
//    //            }

//    //            // Now, add the smallest one it found to the ordereddictionary.
//    //            result.Add(smallestKey, smallestValue);
//    //        }

//    //        // Return the result.
//    //        return result;
//    //    }
//    //}
//}
