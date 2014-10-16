using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            this.Source = source.Replace('\r', ' ').Replace('\n', ' ');
        }

        /// <summary>
        /// Selector method for selecting elements
        /// </summary>
        /// <param name="selector">CSS selector (i.e. #id, .class, element, [property]value)</param>
        /// <returns>Array of Query containing the selected elements</returns>
        public Query[] Select(string selector)
        {
            char qualifier = selector[0];
            int[] indices = null;

            // Depending on the leading character we use a different string to search the dom
            switch (qualifier)
            {
                case '#': // Element ID
                    { 
                        this.Select("[id]" + selector.Substring(1));
                        break;
                    }
                case '.': // Element Class
                    {
                        indices = this.GetSplitIndices(selector.Substring(1));
                        break;
                    }
                case '[': // Custom Element Property
                    {
                        indices = this.GetSplitIndices(selector.Substring(1, selector.IndexOf(']') - 1) + "=\"" + selector.Substring(selector.IndexOf(']') + 1) + "\"");
                        break;
                    }
                default: // Element Type
                    {
                        indices = this.GetSplitIndices("<" + selector);
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

                temp.Add(
                    // Get the source for the complete tag and wrap with new Query
                    new Query(this.Source.Substring(start, end))
                );
            }

            return temp.ToArray();
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
            string start = this.Source.Substring(this.Source.IndexOf(name) + name.Length);
            return new Query(start.Remove(start.IndexOf("\"")));
        }

        /// <summary>
        /// Gets the inner html of this.Query
        /// </summary>
        /// <returns>The HTML between the start/end tags</returns>
        public Query Inner()
        {
            if (this.Source[0] != '<')
                throw new InvalidOperationException("Method may only be used on a Query containing an element");

            int start = this.Source.IndexOf('>') + 1;
            return new Query(this.Source.Substring(start, this.Source.Length - start - this.FindTagName(0).Length - 3));
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
        private int[] GetSplitIndices(string splitter)
        {
            if (!this.Source.Contains(splitter))
                throw new ArgumentException("No instances of " + splitter + " found");

            string[] split = this.Source.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries);

            int length = split[0].Length;
            int[] indices = new int[split.Length - 1];
            
            for (int i = 1; i < split.Length; i++)
            {
                // Add the start index of the current split
                indices[i - 1] = length + 1;

                // Increment start index
                length += split[i].Length + splitter.Length;
            }
            
            return indices;
        }

        /// <summary>
        /// Given a position in a string, get the nearest opening tag
        /// </summary>
        /// <param name="start">The position to search from</param>
        /// <returns>The position index of the start tag</returns>
        private int FindStartTag(int start)
        {
            string str = this.Source.Substring(0, start);
            return str.LastIndexOf('<');
        }

        /// <summary>
        /// Given the position index of the start tag, find the matching end tag
        /// </summary>
        /// <param name="start">The position of the start tag</param>
        /// <returns>The position index of the final character in this tag</returns>
        private int FindEndTag(int start)
        {
            string workingSource = this.Source.Substring(start);

            if (workingSource[0] != '<')
                throw new ArgumentException("The source must begin with a tag");

            string tag = this.FindTagName(start);
            string startTag = "<" + tag;
            string endTag = "</" + tag + ">";
            
            // Build a regex and split the string on the start and end tags
            // Regex is used because regular string.split removed the original split string
            List<string> delimiters = new List<string>
			{
				startTag,
				endTag
			};
            string pattern = "(" + string.Join("|", ( from d in delimiters select Regex.Escape(d)).ToArray<string>()) + ")";
            string[] parts = Regex.Split(workingSource, pattern);
            
            
            int index = parts[0].Length;
            int opens = 0;

            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i].StartsWith(startTag))
                    opens++; // If we find an opening tag, increment
                else if (parts[i].StartsWith(endTag))
                    opens--; // If we have an end tag, decrement 

                index += parts[i].Length;
                
                if (opens == 0) // If we have found as many close tags as we have open tags, we have the matching end tag
                    return index;
            }

            if (opens > 0)
                throw new NullReferenceException("No matching end tag");

            throw new InvalidOperationException("No tag found");
        }

        /// <summary>
        /// Given an index in the source, get the name of the closes tag
        /// </summary>
        /// <param name="start">The position index to start from</param>
        /// <returns>The tag name</returns>
        private string FindTagName(int start)
        {
            string clean = this.Source.Substring(start).Delete().TrimStart(new char[0]);
            return clean.Substring(0, clean.IndexOfAny(new char[]
			{
				' ',
				'>'
			}));
        }
    }
}
