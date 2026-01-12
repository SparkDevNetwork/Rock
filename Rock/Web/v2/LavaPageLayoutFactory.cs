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
using AngleSharp.Html.Dom;

using Microsoft.Extensions.FileProviders;

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

        /// <summary>
        /// The instance that will help with resolving custom elements into
        /// valid HTML and Lava.
        /// </summary>
        private readonly CustomElementResolver _customElementResolver;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="LavaPageLayoutFactory"/>.
        /// </summary>
        /// <param name="fileProvider">The file provider that grants us access to the file system.</param>
        public LavaPageLayoutFactory( IFileProvider fileProvider )
        {
            _fileProvider = fileProvider;
            _customElementResolver = new CustomElementResolver( this );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the existing layout or creates a new one for the specified
        /// file.
        /// </summary>
        /// <param name="layoutPath">The full path and filename to the template file.</param>
        /// <param name="themeName">The name of the theme that the layout belongs to.</param>
        /// <param name="lavaEngine">The Lava engine that will be used to render the templates later.</param>
        /// <returns>An instance of <see cref="LavaPageLayout"/> that represents the pre-parsed template.</returns>
        public LavaPageLayout GetLayout( string layoutPath, string themeName, ILavaEngine lavaEngine )
        {
            return _layoutCache.GetOrAdd( layoutPath, ( p, a ) => CreateLayout( p, a.themeName, a.lavaEngine ), (themeName, lavaEngine) );
        }

        /// <summary>
        /// Creates a new layout for the specified file.
        /// </summary>
        /// <param name="layoutPath">The full path and filename to the template file.</param>
        /// <param name="themeName">The name of the theme that the layout belongs to.</param>
        /// <param name="lavaEngine">The Lava engine that will be used to render the templates later.</param>
        /// <returns>An instance of <see cref="LavaPageLayout"/> that represents the pre-parsed template.</returns>
        internal LavaPageLayout CreateLayout( string layoutPath, string themeName, ILavaEngine lavaEngine )
        {
            var context = new LavaPageLayoutContext( themeName );
            var nodes = ProcessLayout( layoutPath, context, 10 );
            string templateContent;

            if ( context.RootDocument == null )
            {
                templateContent = string.Empty;
            }
            else
            {
                if ( context.RootDocument.DocumentElement == null )
                {
                    var htmlElement = nodes.FirstOrDefault( n => n is IHtmlHtmlElement );

                    if ( htmlElement != null )
                    {
                        context.RootDocument.AppendChild( htmlElement );
                    }
                }

                templateContent = context.RootDocument.ToHtml();
            }

            var result = lavaEngine.ParseTemplate( templateContent );

            if ( result.Error != null )
            {
                throw result.Error;
            }

            return new LavaPageLayout( result.Template, templateContent, context.GetZones(), context.Dependencies );
        }

        /// <summary>
        /// Processes the specified layout file and returns a list of parsed
        /// nodes representing its structure.
        /// </summary>
        /// <param name="filePath">The full path to the layout file to process.</param>
        /// <param name="context">The context for the entire render operation.</param>
        /// <param name="maxDepth">The maximum depth allowed for recursion.</param>
        /// <returns>A list of nodes parsed from the layout file.</returns>
        internal List<INode> ProcessLayout( string filePath, LavaPageLayoutContext context, int maxDepth )
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

            _customElementResolver.ProcessNodes( document, document.DocumentElement, context, 0 );
            _customElementResolver.ProcessZoneNodes( document, document.DocumentElement, context );

            InjectTitleElement( document );
            InjectBodyClassAttribute( document );

            var headElement = document.QuerySelector( "head" );
            var bodyElement = document.QuerySelector( "body" );

            headElement?.Append( document.CreateTextNode( "{{ HeadEndContent }}" ) );
            bodyElement?.Append( document.CreateTextNode( "{{ BodyEndContent }}" ) );

            context.RootDocument = document;

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

            _customElementResolver.ProcessNodes( document, container, context, maxDepth );

            return container.ChildNodes.ToList();
        }

        /// <summary>
        /// Injects the standard head title Lava code into the &lt;title&gt;
        /// element. If the element does not exist it is created.
        /// </summary>
        /// <param name="document">The HTML document.</param>
        private void InjectTitleElement( IDocument document )
        {
            var headElement = document.QuerySelector( "head" );
            var titleElement = headElement.QuerySelector( "title" );

            if ( titleElement == null )
            {
                titleElement = document.CreateElement( "title" );
                headElement.AppendChild( titleElement );
            }

            titleElement.TextContent = "{% if BrowserTitle != null and BrowserTitle != empty %}{{ BrowserTitle | Escape }} | {% endif %}{{ SiteTitle | Escape }}";
        }

        /// <summary>
        /// Injects the Lava required to render custom CSS classes into the body
        /// element.
        /// </summary>
        /// <param name="document">The HTML document.</param>
        private void InjectBodyClassAttribute( IDocument document )
        {
            var bodyElement = document.QuerySelector( "body" );

            if ( bodyElement.HasAttribute( "class" ) )
            {
                bodyElement.SetAttribute( "class", bodyElement.GetAttribute( "class" ) + "{% if BodyCssClass != null and BodyCssClass != empty %} {{ BodyCssClass }}{% endif %}" );
            }
            else
            {
                bodyElement.SetAttribute( "class", "{{ BodyCssClass }}" );
            }
        }

        #endregion
    }
}
