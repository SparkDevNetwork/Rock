// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace Rock.Web.v2
{
    /// <summary>
    /// Provides contextual information and state management for rendering
    /// operations by <see cref="LavaPageLayoutFactory"/>.
    /// </summary>
    class LavaPageLayoutContext
    {
        #region Fields

        /// <summary>
        /// Represents the various levels of nesting layout files.
        /// </summary>
        private readonly List<ContextLevel> _levels = new List<ContextLevel>();

        #endregion

        #region Properties

        /// <summary>
        /// The full paths to files that are involved in the final rendering
        /// of the layout. This is used for change tracking later.
        /// </summary>
        public List<string> Dependencies { get; } = new List<string>();

        /// <summary>
        /// The name of the theme this layout belongs to. This is used when
        /// resolving <c>~~/</c> paths.
        /// </summary>
        public string ThemeName { get; }

        /// <summary>
        /// Gets the HTML parser used to parse HTML content within the current
        /// context.
        /// </summary>
        public HtmlParser Parser { get; } = new HtmlParser();

        #endregion

        #region Methods

        /// <summary>
        /// Begins a new layout level for this context.
        /// </summary>
        public void EnterLayout()
        {
            _levels.Add( new ContextLevel() );
        }

        /// <summary>
        /// Exits the current layout level for this context.
        /// </summary>
        public void ExitLayout()
        {
            _levels.RemoveAt( _levels.Count - 1 );
        }

        /// <summary>
        /// Sets the content for a named section. If the content for the
        /// section has already been set on the layout it is overwritten.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="nodes">The content of the section.</param>
        public void SetSection( string name, INodeList nodes )
        {
            _levels[_levels.Count - 1].Sections[name] = nodes;
        }

        /// <summary>
        /// Gets the nodes that make up the content of a named section.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <returns>An enumerable that contains the nodes or <c>null</c> if the section was not defined.</returns>
        public INodeList GetSection( string name )
        {
            for ( int i = _levels.Count - 1; i >= 0; i-- )
            {
                if ( _levels[i].Sections.TryGetValue( name, out var nodes ) )
                {
                    return nodes;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the body content of the current layout. This should be called
        /// when processing the &lt;Rock:ParentLayout&gt; tag, before beginning
        /// processing of the parent layout.
        /// </summary>
        /// <param name="nodes">The nodes that make up the body content.</param>
        public void SetBody( List<INode> nodes )
        {
            _levels[_levels.Count - 1].Body = nodes;
        }

        /// <summary>
        /// Gets the content that makes up the body of the
        /// &lt;Rock:ParentLayout&gt; tag of the child layout that requested
        /// this layout be included.
        /// </summary>
        /// <returns>The content of the body or <c>null</c> if it was not defined.</returns>
        public List<INode> GetChildBody()
        {
            if ( _levels.Count < 2 )
            {
                return null;
            }

            return _levels[_levels.Count - 2].Body;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// A single layout level in the hiearchy of layouts when building
        /// a parsed layout.
        /// </summary>
        private class ContextLevel
        {
            /// <summary>
            /// The section content that has been defined at this level.
            /// </summary>
            public Dictionary<string, INodeList> Sections { get; } = new Dictionary<string, INodeList>();

            /// <summary>
            /// The body content defined at this level.
            /// </summary>
            public List<INode> Body { get; set; }
        }

        #endregion
    }
}
