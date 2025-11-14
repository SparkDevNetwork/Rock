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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using AngleSharp;
using AngleSharp.Dom;

using Microsoft.Extensions.FileProviders;

using Rock.Configuration;
using Rock.Lava;

namespace Rock.Web.v2
{
    /// <summary>
    /// Provides functionality for loading, parsing and caching layouts from
    /// files on the filesystem.
    /// </summary>
    internal class LavaPageLayoutFactory
    {
        #region Fields

        /// <summary>
        /// The cached layouts for this factory.
        /// </summary>
        private readonly ConcurrentDictionary<string, LavaPageLayout> _layoutCache = new ConcurrentDictionary<string, LavaPageLayout>();

        /// <summary>
        /// The file provider that grants us access to the file system.
        /// </summary>
        private readonly IFileProvider _fileProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="LavaPageLayoutFactory"/>.
        /// </summary>
        /// <param name="fileProvider">The file provider that grants us access to the file system.</param>
        public LavaPageLayoutFactory( IFileProvider fileProvider )
        {
            _fileProvider = fileProvider;
        }

        #endregion

        /// <summary>
        /// Gets the existing layout or creates a new one for the specified
        /// file.
        /// </summary>
        /// <param name="layoutPath">The full path and filename to the template file.</param>
        /// <param name="lavaEngine">The Lava engine that will be used to render the templates later.</param>
        /// <returns>An instance of <see cref="LavaPageLayout"/> that represents the pre-parsed template.</returns>
        public LavaPageLayout GetLayout( string layoutPath, ILavaEngine lavaEngine )
        {
            return _layoutCache.GetOrAdd( layoutPath, CreateLayout, lavaEngine );
        }

        /// <summary>
        /// Creates a new layout for the specified file.
        /// </summary>
        /// <param name="layoutPath">The full path and filename to the template file.</param>
        /// <param name="lavaEngine">The Lava engine that will be used to render the templates later.</param>
        /// <returns>An instance of <see cref="LavaPageLayout"/> that represents the pre-parsed template.</returns>
        internal LavaPageLayout CreateLayout( string layoutPath, ILavaEngine lavaEngine )
        {
            var context = new LavaPageLayoutContext();
            var nodes = ProcessLayout( layoutPath, context, 10 );
            var templateContent = string.Join( string.Empty, nodes.Select( n => n.ToHtml() ) );

            var result = lavaEngine.ParseTemplate( templateContent );

            return new LavaPageLayout( result.Template, templateContent, context.Dependencies );
        }

        /// <summary>
        /// Processes the specified layout file and returns a list of parsed
        /// nodes representing its structure.
        /// </summary>
        /// <param name="filePath">The full path to the layout file to process.</param>
        /// <param name="context">The context for the entire render operation.</param>
        /// <param name="maxDepth">The maximum depth allowed for recursion.</param>
        /// <returns>A list of nodes parsed from the layout file.</returns>
        private List<INode> ProcessLayout( string filePath, LavaPageLayoutContext context, int maxDepth )
        {
            context.Dependencies.Add( filePath );
            context.EnterLayout();

            try
            {
                var fileInfo = _fileProvider.GetFileInfo( filePath );
                string fileContent;

                using ( var stream = fileInfo.CreateReadStream() )
                {
                    using ( var reader = new StreamReader( stream, detectEncodingFromByteOrderMarks: true ) )
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }

                var isRootLayout = fileContent.IndexOf( "<html", StringComparison.OrdinalIgnoreCase ) >= 0
                    || fileContent.IndexOf( "<head", StringComparison.OrdinalIgnoreCase ) >= 0
                    || fileContent.IndexOf( "<body", StringComparison.OrdinalIgnoreCase ) >= 0;

                return isRootLayout
                    ? ProcessRootLayout( fileContent, context )
                    : ProcessPartialLayout( fileContent, context, maxDepth );
            }
            finally
            {
                context.ExitLayout();
            }
        }

        /// <summary>
        /// Processes a root layout. A root layout has at least one
        /// &lt;html&gt;, &lt;head&gt; or &lt;body&gt; tags.
        /// </summary>
        /// <param name="content">The plain text content of the layout file.</param>
        /// <param name="context">The context for the entire render operation.</param>
        /// <returns>A list of nodes that represent this layout.</returns>
        private List<INode> ProcessRootLayout( string content, LavaPageLayoutContext context )
        {
            var document = context.Parser.ParseDocument( content );
            var bodyNode = document.QuerySelector( "body" );

            // For some reason, AngleSharp adds extra "\n\n" at the end of the
            // body tag. Try to clean that up.
            if ( bodyNode.ChildNodes.LastOrDefault() is IText textNode )
            {
                textNode.TextContent = Regex.Replace( textNode.TextContent, "\n\n+", "\n" );
            }

            ProcessNodes( document.DocumentElement, context, 0 );

            return new List<INode> { document.DocumentElement };
        }

        /// <summary>
        /// Processes a partial layout. A partial layout does not have any
        /// &lt;html&gt;, &lt;head&gt; or &lt;body&gt; tags.
        /// </summary>
        /// <param name="content">The plain text content of the layout file.</param>
        /// <param name="context">The context for the entire render operation.</param>
        /// <param name="maxDepth">The maximum depth allowed for recursion.</param>
        /// <returns>A list of nodes that represent this layout.</returns>
        private List<INode> ProcessPartialLayout( string content, LavaPageLayoutContext context, int maxDepth )
        {
            var document = context.Parser.ParseDocument( "<html></html>" );
            var nodes = context.Parser.ParseFragment( content, document.Body );

            var container = document.CreateElement( "div" );
            container.Append( nodes.ToArray() );

            ProcessNodes( container, context, maxDepth );

            return container.ChildNodes.ToList();
        }

        /// <summary>
        /// Process the custom nodes in the layout to build. This will modify
        /// the <paramref name="container"/> in place when making changes.
        /// </summary>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        /// <param name="maxDepth">The maximum depth allowed for recursion.</param>
        private void ProcessNodes( IElement container, LavaPageLayoutContext context, int maxDepth )
        {
            ProcessSectionNodes( container, context );
            ProcessRenderBodyNode( container, context );
            ProcessRenderSectionNodes( container, context );

            // If this is not a root layout, then process any parent layouts.
            if ( maxDepth > 0 )
            {
                ProcessParentLayoutNodes( container, context, maxDepth );
            }
        }

        /// <summary>
        /// <para>
        /// Processes any child 'Rock:Section' nodes. These define content that
        /// can be used by a parent layout. If the named section has already
        /// been defined by a child layout or previously in this layout then it
        /// will be replaced.
        /// </para>
        /// <para>
        /// Nesting sections can be achieved by doing something like the
        /// following, as RenderSection tags are processed before Section tags.
        /// <code>
        /// &lt;Rock:Section name="main"&gt;
        ///     &lt;div&gt;Additional content.&lt;/div&gt;
        ///     &lt;Rock:RenderSection name="main"&gt;&lt;/Rock:RenderSection&gt;
        /// &lt;/Rock:Section&gt;
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        private void ProcessSectionNodes( IElement container, LavaPageLayoutContext context )
        {
            var sectionElements = container.QuerySelectorAll( "Rock\\:Section" );

            foreach ( var sectionElement in sectionElements )
            {
                var sectionName = sectionElement.GetAttribute( "name" );

                if ( sectionName.IsNotNullOrWhiteSpace() )
                {
                    context.SetSection( sectionName, sectionElement.ChildNodes );
                }

                sectionElement.Remove();
            }
        }

        /// <summary>
        /// Renders the body content of the immediate child layout, that is any
        /// content that was inside the &lt;Rock:ParentLayout&gt; tag.
        /// </summary>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        private void ProcessRenderBodyNode( IElement container, LavaPageLayoutContext context )
        {
            var renderBodyElement = container.QuerySelectorAll( "Rock\\:RenderBody" )
                .FirstOrDefault();

            if ( renderBodyElement == null )
            {
                return;
            }

            var bodyNodes = context.GetChildBody();

            if ( bodyNodes != null )
            {
                renderBodyElement.InsertBefore( TrimNodes( bodyNodes ) );
            }
            else
            {
                renderBodyElement.InsertBefore( TrimNodes( renderBodyElement.ChildNodes ) );
            }

            renderBodyElement.Remove();
        }

        /// <summary>
        /// <para>
        /// Renders the named section into the layout. Sections can be defined
        /// in any child or descendant layout. They do not need to be defined
        /// in the immediate child.
        /// </para>
        /// <para>
        /// If the named section has not been defined then the inner content
        /// will be used as default content.
        /// </para>
        /// </summary>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        private void ProcessRenderSectionNodes( IElement container, LavaPageLayoutContext context )
        {
            var renderSectionElements = container.QuerySelectorAll( "Rock\\:RenderSection" );

            foreach ( var renderSectionElement in renderSectionElements )
            {
                var sectionName = renderSectionElement.GetAttribute( "name" );

                // If there is no section name, then just use the default
                // content.
                if ( sectionName.IsNullOrWhiteSpace() )
                {
                    renderSectionElement.InsertBefore( TrimNodes( renderSectionElement.ChildNodes ) );
                    renderSectionElement.Remove();

                    continue;
                }

                var elements = context.GetSection( sectionName );

                if ( elements != null )
                {
                    renderSectionElement.InsertBefore( TrimNodes( elements ) );
                }
                else
                {
                    renderSectionElement.InsertBefore( TrimNodes( renderSectionElement.ChildNodes ) );
                }

                renderSectionElement.Remove();
            }
        }

        /// <summary>
        /// Processes all &lt;Rock:ParentLayout&gt; nodes found in the layout
        /// and replaces them with the content of the parent layout.
        /// </summary>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        /// <param name="maxDepth">The maximum depth allowed for recursion.</param>
        private void ProcessParentLayoutNodes( IElement container, LavaPageLayoutContext context, int maxDepth )
        {
            var parentLayoutElements = container.QuerySelectorAll( "Rock\\:ParentLayout" );

            foreach ( var parentElement in parentLayoutElements )
            {
                var src = parentElement.GetAttribute( "src" );

                src = RockApp.Current.MapPath( src, context.ThemeName );

                if ( src.IsNullOrWhiteSpace() )
                {
                    parentElement.Remove();
                    continue;
                }

                context.SetBody( parentElement.ChildNodes.ToList() );

                var renderedElements = ProcessLayout( src, context, maxDepth - 1 );

                parentElement.InsertBefore( renderedElements.ToArray() );
                parentElement.Remove();
            }
        }

        /// <summary>
        /// Trim any whitespace off the start and end of the node list. This
        /// is used for embedding to keep the final node list and rendered
        /// HTML looking somewhat clean.
        /// </summary>
        /// <param name="nodes">The nodes to be trimmed</param>
        /// <returns>An array of nodes with whitespace trimmed from the start and end.</returns>
        private INode[] TrimNodes( IEnumerable<INode> nodes )
        {
            var trimmedNodes = nodes.ToList();

            // Trim whitespace from the start.
            while ( trimmedNodes.Count > 1 && trimmedNodes[0] is IText textNode && textNode.TextContent.Trim() == string.Empty )
            {
                trimmedNodes.RemoveAt( 0 );
            }

            while ( trimmedNodes.Count > 1 && trimmedNodes.Last() is IText textNode && textNode.TextContent.Trim() == string.Empty )
            {
                trimmedNodes.RemoveAt( trimmedNodes.Count - 1 );
            }

            return trimmedNodes.ToArray();
        }
    }

    internal class LavaPageLayout
    {
        public ILavaTemplate Template { get; }

        public string Source { get; }

        public IReadOnlyList<string> Dependencies { get; }

        public LavaPageLayout( ILavaTemplate template, string source, IReadOnlyList<string> dependencies )
        {
            Template = template;
            Source = source;
            Dependencies = dependencies;
        }
    }
}
