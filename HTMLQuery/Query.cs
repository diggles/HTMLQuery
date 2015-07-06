using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace HTMLQuery
{
    /// <summary>
    /// Class containing the dom and manipulation methods
    /// </summary>
    public class Query
    {
        /// <summary>
        /// The HTML Source
        /// </summary>
        public string Source
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The HTML Source</param>
        public Query(string source)
        {
            this.Source = source.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
            
            // Strip doctype
            if(source.ToLower().Contains("html"))
                this.Source = this.Source.Remove(0, this.Source.IndexOf("<html>", System.StringComparison.OrdinalIgnoreCase)).Trim();
        }

        /// <summary>
        /// Selector method for selecting elements
        /// </summary>
        /// <param name="selector">CSS selector (i.e. #id, .class, element, [property]value)</param>
        /// <param name="searchChildren">Whether or not to traverse child elements</param>
        /// <returns>Array of Query containing the selected elements</returns>
        public Query[] Select(string selector, bool searchChildren)
        {
            char qualifier = selector[0];
            int[] indices = null;

            // Depending on the leading character we use a different string to search the dom
            switch (qualifier)
            {
                case '#': // Element ID
                    {
                        return this.Select("[id]" + selector.Substring(1), searchChildren);
                    }
                case '.': // Element Class
                    {
                        return this.Select("[class]" + selector.Substring(1), searchChildren);
                    }
                case '[': // Custom Element Property
                {
                    string property = selector.Substring(1, selector.IndexOf(']') - 1);
                    string value = selector.Substring(selector.IndexOf(']') + 1);
                    string reg = string.Format("({0}=(?:\"|')[a-zA-Z0-9 ]*{1}[a-zA-Z0-9 ]*(?:\"|'))", property, value);
                    indices = this.GetSplitIndices(reg, searchChildren);
                    break;
                    }
                default: // Element Type
                    {
                        indices = this.GetSplitIndices("(<" + selector + ")", searchChildren);
                        break;
                    }
            }

            List<Query> temp = new List<Query>();

            for (int j = 0; j < indices.Length; j++)
            {
                // Get the starting index of this tag
                int start = this.FindStartTag(indices[j]);

                // Get the end tag
                int end = this.FindEndTag(start);

                //

                temp.Add(
                    // Get the source for the complete tag and wrap with new Query
                    new Query(this.Source.Substring(start, end))
                );
            }

            return temp.ToArray();
        }

        // Override for the most common case
        public Query[] Select(string selector)
        {
            return this.Select(selector, true);
        }

        /// <summary>
        /// Gets the value of a perperty in an element
        /// Only queries the first element in this.Source
        /// </summary>
        /// <param name="property">The property name</param>
        /// <returns>The value of the property</returns>
        public Query Value(string property)
        {
            if (this.Source[0] != '<')
                throw new InvalidOperationException("Method may only be used on a Query containing an element");

            string name = property + "=\"";
            string start = this.Source.Substring(this.Source.IndexOf(name, System.StringComparison.Ordinal) + name.Length);
            return new Query(start.Remove(start.IndexOf("\"", System.StringComparison.Ordinal)));
        }

        /// <summary>
        /// Gets the inner html of this.Query
        /// </summary>
        /// <returns>The HTML between the start/end tags</returns>
        public Query InnerHtml()
        {
            return new Query(StripOuterTags(this.Source));
        }

        private string StripOuterTags(string input)
        {
            if (this.Source[0] != '<')
                throw new InvalidOperationException("Method may only be used on a Query containing an element");

            int start = this.Source.IndexOf('>') + 1;
            int end = this.Source.Length - start - this.FindTagName(0).Length - 3;
            if(end<0)
                return string.Empty;

            return input.Substring(start, end).Trim();
        }

        /// <summary>
        /// Gets the inner text of this.Query
        /// </summary>
        /// <returns>The text with any child elements removed</returns>
        public string InnerText()
        {
            if (this.Source[0] != '<')
                throw new InvalidOperationException("Method may only be used on a Query containing an element");

            return Regex.Replace(this.InnerHtml().ToString(), "<.*?>", string.Empty);
        }

        /// <summary>
        /// Removes all HTML tags from this.Source
        /// </summary>
        /// <returns>Plain text</returns>
        public string StripHtml()
        {
            return WebUtility.HtmlDecode(Regex.Replace(this.Source, "<[^>]*>", string.Empty));
        }

        /// <summary>
        /// Override ToString to return this.Source so we dont have to call Query.String explicitly
        /// </summary>
        /// <returns>The HTML contained in this query</returns>
        public override string ToString()
        {
            return this.Source;
        }

        /// <summary>
        /// Searches the source for all matching strings and returns the start indices
        /// </summary>
        /// <param name="splitter">The search string</param>
        /// <returns>An array of start indices</returns>
        private int[] GetSplitIndices(string splitter, bool searchChildren)
        {
            Regex matcher = new Regex(splitter);

            if (!matcher.IsMatch(this.Source))
                return new int[] {};
                //throw new ArgumentException("No instances of " + splitter + " found");

            string workingSource = searchChildren ? this.Source : this.Flatten().Source;

            string[] split = workingSource.InclusiveSplit(new [] { splitter });

            int length = 0;
            List<int> indices = new List<int>();
            
            foreach (string element in split)
            {
                // Add the start index of the current split
                if (matcher.IsMatch(element))
                    indices.Add(length + 1);    
                
                // Increment start index
                // TODO: Implement this
                //if (searchChildren)
                
                length += element.Length;
            }
            
            return indices.ToArray();
        }

        /// <summary>
        /// Removes all child elements
        /// If there are multiple top-level elements only the first one is returned
        /// </summary>
        /// <returns>A new Query with only top level elements remaining</returns>
        public Query Flatten()
        {
            if(!this.Source.Contains('<'))
                return new Query(this.Source);

            StringBuilder sb = new StringBuilder();

            string workingSource = this.Source;

            int start = workingSource.IndexOf('<');

            // Scope the source to just the top tag
            string workingTag = workingSource.Substring(start, FindEndTag(start));

            string startTag = workingTag.Substring(0, workingTag.IndexOf('>') + 1);
            string endTag = workingTag.Substring(workingTag.LastIndexOf('<'));

            // Strip the outertags and re-scope to the content
            // we need to strip the child tags from this markup
            workingTag = this.StripOuterTags(workingTag).Clone().ToString();

            // While there are remaining child tags
            while (workingTag.Contains('<'))
            {
                // Save any text 
                sb.Append(TakeClean(ref workingTag));
                
                // And strip the next tag
                workingTag = StripNext(workingTag);
            }

            sb.Append(workingTag);

            return new Query(startTag + sb.ToString() + endTag);
        }

        private string StripNext(string input)
        {
            int end = FindEndTag(input, input.IndexOf('<'));
            return input.Remove(0, end);
        }

        private string TakeClean(ref string input)
        {
            string output = input.Substring(0, input.IndexOf('<'));
            input = input.Substring(output.Length);
            return output;
        }

        /// <summary>
        /// Given a position in a string, get the nearest opening tag
        /// </summary>
        /// <param name="start">The position to search from</param>
        /// <returns>The position index of the start tag</returns>
        private int FindStartTag(int start)
        {
            return this.FindStartTag(this.Source, start);
        }

        private int FindStartTag(string input, int start)
        {
            string str = input.Substring(0, start);
            return str.LastIndexOf('<');
        }

        /// <summary>
        /// Given the position index of the start tag, find the matching end tag
        /// </summary>
        /// <param name="start">The position of the start tag</param>
        /// <returns>The position index of the final character in this tag</returns>
        private int FindEndTag(int start)
        {
            return this.FindEndTag(this.Source, start);
        }

        private int FindEndTag(string input, int start)
        {
            string workingSource = input.Substring(start);

            if (workingSource[0] != '<')
                throw new ArgumentException("The source must begin with a tag");

            string tag = this.FindTagName(input, start);
            string startTag = "<" + tag + "";
            string endTag = "</" + tag + ">";

            // Build a regex and split the string on the start and end tags
            // Regex is used because regular string.split removed the original split string
            string[] parts = workingSource.InclusiveSplit(new []
			{
				"(" + startTag + ")",
				"(" + endTag + ")"
			});

            int index = parts[0].Length;
            int opens = 0;
            int firstOpen = 0;

            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i].StartsWith(startTag))
                {
                    if (firstOpen == 0) firstOpen = index;
                    opens++; // If we find an opening tag, increment
                }
                else if (parts[i].StartsWith(endTag))
                    opens--; // If we have an end tag, decrement 

                index += parts[i].Length;

                if (opens == 0) // If we have found as many close tags as we have open tags, we have the matching end tag
                    return index;
            }

            // Assume the tag isn't closed
            if (opens > 0)
                return firstOpen;

            throw new InvalidOperationException("No tag found");
        }

        /// <summary>
        /// Given an index in the source, get the name of the closes tag
        /// </summary>
        /// <param name="start">The position index to start from</param>
        /// <returns>The tag name</returns>
        private string FindTagName(int start)
        {
            return this.FindTagName(this.Source, start);
        }

        private string FindTagName(string input, int start)
        {
            string clean = input.Substring(start).Delete().TrimStart(new char[0]);
            return clean.Substring(0, clean.IndexOfAny(new char[]
			{
				' ',
				'>'
			}));
        }
    }
}
