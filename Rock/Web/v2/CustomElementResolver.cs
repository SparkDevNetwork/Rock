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
using System.Collections.Generic;
using System.Linq;

using AngleSharp.Dom;

using Rock.Configuration;

namespace Rock.Web.v2
{
    /// <summary>
    /// Resolves and replaces custom elements with Lava and other content so
    /// that the template can be rendered as a simple Lava template.
    /// </summary>
    internal class CustomElementResolver
    {
        #region Fields

        /// <summary>
        /// The layout factory that owns this resolver. This is used to continue
        /// parsing nested layouts.
        /// </summary>
        private readonly LavaPageLayoutFactory _layoutFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="CustomElementResolver"/>.
        /// </summary>
        /// <param name="layoutFactory">The layout factory that owns this resolver.</param>
        public CustomElementResolver( LavaPageLayoutFactory layoutFactory )
        {
            _layoutFactory = layoutFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process the custom nodes in the layout to build. This will modify
        /// the <paramref name="container"/> in place when making changes.
        /// </summary>
        /// <param name="document">The document that should be used when creating new nodes.</param>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        /// <param name="maxDepth">The maximum depth allowed for recursion.</param>
        public void ProcessNodes( IDocument document, IElement container, LavaPageLayoutContext context, int maxDepth )
        {
            ProcessPageIconNodes( document, container, context );
            ProcessPageTitleNodes( document, container, context );
            ProcessPageBreadCrumbsNodes( document, container, context );
            ProcessPageDescriptionNodes( document, container, context );
            ProcessSectionNodes( container, context );
            ProcessRenderBodyNode( container, context );
            ProcessRenderSectionNodes( container, context );

            // If we have not reached max depth, check for parent layouts.
            if ( maxDepth > 0 )
            {
                ProcessParentLayoutNodes( container, context, maxDepth );
            }
        }

        /// <summary>
        /// Processes any child 'Rock:Zone' nodes. These define the zones that
        /// are available to render content into. This should only be called on
        /// the root layout.
        /// </summary>
        /// <param name="document">The document that should be used when creating new nodes.</param>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        public void ProcessZoneNodes( IDocument document, IElement container, LavaPageLayoutContext context )
        {
            var zoneElements = container.QuerySelectorAll( "Rock\\:Zone" );

            foreach ( var zoneElement in zoneElements )
            {
                var zoneName = zoneElement.GetAttribute( "name" );
                var zoneClasses = zoneElement.GetAttribute( "class" );

                if ( zoneName.IsNotNullOrWhiteSpace() )
                {
                    var zone = context.AddZone( zoneName, zoneClasses );

                    var textNode = document.CreateTextNode( $"{{{{ Zones.{zone.Key} }}}}" );
                    zoneElement.Before( textNode );
                }

                zoneElement.Remove();
            }
        }

        /// <summary>
        /// Processes any child 'Rock:PageIcon' nodes. These should output the
        /// icon defined for the page as an HTML element.
        /// </summary>
        /// <param name="document">The document that should be used when creating new nodes.</param>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        private void ProcessPageIconNodes( IDocument document, IElement container, LavaPageLayoutContext context )
        {
            var pageIconElements = container.QuerySelectorAll( "Rock\\:PageIcon" );

            foreach ( var pageIconElement in pageIconElements )
            {
                var lava = "{% if Page.PageDisplayIcon == true and PageIconCssClass != null and PageIconCssClass != empty %}<div class=\"page-icon\"><i class=\"{{ PageIconCssClass | Escape }}\"></i></div>{% endif %}";
                var nodes = context.Parser.ParseFragment( lava, document.Body ).ToArray();

                pageIconElement.InsertBefore( nodes );
                pageIconElement.Remove();
            }
        }

        /// <summary>
        /// Processes any child 'Rock:PageTitle' nodes. These should output the
        /// title defined for the page as an escaped HTML string.
        /// </summary>
        /// <param name="document">The document that should be used when creating new nodes.</param>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        private void ProcessPageTitleNodes( IDocument document, IElement container, LavaPageLayoutContext context )
        {
            var pageTitleElements = container.QuerySelectorAll( "Rock\\:PageTitle" );

            foreach ( var pageTitleElement in pageTitleElements )
            {
                var lava = "{% if Page.PageDisplayTitle and PageTitle != null and PageTitle != empty %}{{ PageTitle | Escape }}{% endif %}";
                var nodes = context.Parser.ParseFragment( lava, document.Body ).ToArray();

                pageTitleElement.InsertBefore( nodes );
                pageTitleElement.Remove();
            }
        }

        /// <summary>
        /// Processes any child 'Rock:PageBreadCrumbs' nodes. These should output the
        /// breadcrumbs as HTML elements.
        /// </summary>
        /// <param name="document">The document that should be used when creating new nodes.</param>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        private void ProcessPageBreadCrumbsNodes( IDocument document, IElement container, LavaPageLayoutContext context )
        {
            var pageBreadCrumbsElements = container.QuerySelectorAll( "Rock\\:PageBreadCrumbs" );

            foreach ( var pageBreadCrumbsElement in pageBreadCrumbsElements )
            {
                var lava = "<ol class=\"breadcrumb\">{% for crumb in BreadCrumbs %}<li class=\"breadcrumb-item\">{% if crumb.Active == false %}<a href=\"{{ crumb.Url }}\" rel=\"rocknofollow\">{{ crumb.Name }}</a>{% else %}{{ crumb.Name }}{% endif %}</li>{% endfor %}</ol>";
                var nodes = context.Parser.ParseFragment( lava, document.Body ).ToArray();

                pageBreadCrumbsElement.InsertBefore( nodes );
                pageBreadCrumbsElement.Remove();
            }
        }

        /// <summary>
        /// Processes any child 'Rock:PageDescription' nodes. These should output the
        /// description defined for the page as an escaped HTML element.
        /// </summary>
        /// <param name="document">The document that should be used when creating new nodes.</param>
        /// <param name="container">The container element that represents the current layout.</param>
        /// <param name="context">The context for the entire render operation.</param>
        private void ProcessPageDescriptionNodes( IDocument document, IElement container, LavaPageLayoutContext context )
        {
            var pageDescriptionElements = container.QuerySelectorAll( "Rock\\:PageDescription" );

            foreach ( var pageDescriptionElement in pageDescriptionElements )
            {
                var lava = "{% if Page.PageDisplayDescription and Page.Description != null and Page.Description != empty %}<div class=\"pageoverview-description\">{{ Page.Description | Escape }}</div>{% endif %}";
                var nodes = context.Parser.ParseFragment( lava, document.Body ).ToArray();

                pageDescriptionElement.InsertBefore( nodes );
                pageDescriptionElement.Remove();
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

                var renderedElements = _layoutFactory.ProcessLayout( src, context, maxDepth - 1 );

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

        #endregion
    }
}
