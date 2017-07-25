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
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_centralaz.Cms
{
    [DisplayName( "Page Configuration Report" )]
    [Category( "com_centralaz > CMS" )]
    [Description( "Displays a connection diagram of the selected page." )]
    [LinkedPage( "Root Page", "Select the root page to use as a starting point for the page confugration report.", true )]
    public partial class PageConfigurationReport : RockBlock
    {
        FieldTypeCache pageReferencefieldType = FieldTypeCache.Read( Rock.SystemGuid.FieldType.PAGE_REFERENCE.AsGuid() );
        StringBuilder nodeData = new StringBuilder();
        StringBuilder edgeData = new StringBuilder();
        bool _isDiagramEnabled = false;

        Dictionary<string, bool> _pageGuids = new Dictionary<string, bool>();
        Dictionary<string, string> _pageGuidsToName = new Dictionary<string, string>();
        Dictionary<string, bool> _referencedPages = new Dictionary<string, bool>();

        protected string NodeData()
        {
            return nodeData.ToString();
        }

        public string EdgeData()
        {
            return edgeData.ToString();
        }

        public string IsDiagramEnabled()
        {
            return _isDiagramEnabled.ToString().ToLower();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            //RockPage.AddCSSLink( ResolveRockUrl( "~/Plugins/com_centralaz/Utility/PageDiagram.css" ) );
            RockPage.AddScriptLink( "~/Plugins/com_centralaz/Cms/Scripts/cytoscape.min.js" );

            //PersonService personService = new PersonService( new RockContext() );
            //Guid guid = new Guid( "955BF3E9-D38A-4DCB-A6F2-EDF5EC2571C5" );
            //Person miller = personService.Get( guid,followMerges:true );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            //if (! Page.IsPostBack )
            //{
                BindPages();
            //}
        }

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindPages();
        }

        #endregion

        #region Methods
        protected void BindPages()
        {
            RockContext rockContext = new RockContext();
            PageService pageService = new PageService( rockContext );

            var allPages = pageService.Queryable( "PageContexts, PageRoutes" );

            // Get list of pages in root page's heirarchy
            PageCache rootPage = null;
            Guid pageGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "RootPage" ), out pageGuid ) )
            {
                rootPage = PageCache.Read( pageGuid );
            }
            else
            {
                return;
            }

            var descendantPages = new List<PageCache>();
            if ( rootPage != null )
            {
                descendantPages = rootPage.GetPages( rockContext ).ToList();
            }

            foreach ( var page in allPages )
            {
                PageCache.Read( page );
            }

            var sb = new StringBuilder();


            foreach ( var page in descendantPages.OrderBy( a => a.Order ).ThenBy( a => a.InternalName ) )
            {
                if ( page == null || page.Guid == null )
                {
                    continue;
                }
                _pageGuids.AddOrReplace( page.Guid.ToString().ToLower(), true );
                _pageGuidsToName.AddOrReplace( page.Guid.ToString().ToLower(), page.InternalName.ToStringSafe() );

                // Check each page's blocks to see which pages are unused.
                foreach ( var block in page.Blocks )
                {
                    foreach ( var attribute in block.Attributes.Values.Where( a => a.FieldType == pageReferencefieldType ).OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                    {
                        var refPageGuid = block.GetAttributeValue( attribute.Key ).ToStringSafe().ToLower();
                        if ( refPageGuid.Contains( "," ) )
                        {
                            refPageGuid = refPageGuid.SplitDelimitedValues()[0];
                        }

                        if ( !string.IsNullOrWhiteSpace( refPageGuid ) )
                        {
                            _referencedPages.AddOrReplace( refPageGuid, true );
                        }
                    }
                }
            }

            InteractionService interactionService = new InteractionService( rockContext );

            foreach ( var page in descendantPages.OrderBy( a => a.Order ).ThenBy( a => a.InternalName ) )
            {
                nodeData.Append( AddNodeData( page ) );
                sb.Append( AddPage( page, rockContext, interactionService ) );
            }

            lPages.Text = sb.ToString();

        }
        #endregion

        protected string AddNodeData( PageCache page )
        {
            if ( page == null )
            {
                return string.Empty;
            }

            // A page:
            //  { data: { id: '123', name: 'Admin' } },

            // A block:
            //  { data: { id: '456', name: 'Admin (CentralAZ)', parent: '123' }, classes: 'block' },

            // block settings:
            /*
                 { data: {
                        id: 'abc-987-654', name: 'Workflow Type : 090189-KHKH187-099898-09029-6737\n \
	Workflow Activity : none\n \
	Allow Manual Setup : True\n \
	Enable Location Sharing : False\n \
	Time to Cache Geo : 20\n \
	Enable Reverse Lookup : True\n \
	', parent: '456'
                    }, classes: 'setting'
                },
           */

            // Edge data
            // { data: { id: 'a1', source: 'adminblock', target: 'Welcome', label: 'Next Page' }, classes: 'autorotate' },

            var sb = new StringBuilder();

            // add the page node
            sb.AppendFormat( @"{{ data: {{ id: '{0}', name: '{1}' }} }},{2}", page.Guid, page.InternalName.Replace( "'", "''" ), Environment.NewLine );

            // add each block node
            foreach ( var block in page.Blocks )
            {
                sb.AppendFormat( @"{{ data: {{ id: '{0}', name: '{1}', parent: '{2}' }}, classes: 'block' }},{3}", block.Guid, block.Name.Replace( "'", "''" ), page.Guid, Environment.NewLine );

                // Page reference block attributes...
                foreach ( var attribute in block.Attributes.Values.Where( a => a.FieldType == pageReferencefieldType ).OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                {
                    var pageGuid = block.GetAttributeValue( attribute.Key ).ToStringSafe().ToLower();
                    if ( pageGuid.Contains( "," ) )
                    {
                        pageGuid = pageGuid.SplitDelimitedValues()[0];
                    }
                    edgeData.AppendFormat( "{{ data: {{ id: '{0}', source: '{1}', target: '{2}', label: '{3}' }}, classes: 'autorotate {4}' }},{5}", attribute.Guid, block.Guid, pageGuid, attribute.Key.SplitCase(), EdgeClass(attribute.Key), Environment.NewLine );
                }

                // Other block attributes
                var attributes = block.Attributes.Values.Where( a => a.FieldType != pageReferencefieldType ).OrderBy( a => a.Order ).ThenBy( a => a.Name );
                if ( attributes != null && attributes.Count() > 0 )
                {
                    sb.AppendFormat( @"{{ data: {{ id: '{0}', name: '", block.Id );

                    foreach ( var attribute in block.Attributes.Values.Where( a => a.FieldType != pageReferencefieldType ).OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                    {
                        if ( attribute != null && attribute.Key != null )
                        {
                            var value = block.GetAttributeValue( attribute.Key );
                            if ( value != null )
                            {
                                sb.AppendFormat( @"    {0} : {1} \n\{2} ", attribute.Key.SplitCase(), value.Replace( "'", "''" ), Environment.NewLine );
                            }
                        }
                    }
                    sb.AppendFormat( @"', parent: '{0}' }}, classes: 'setting' }},
", block.Guid );
                }
            }

            return sb.ToString();
        }

        protected string EdgeClass( string key )
        {
            if ( key.Contains( "Home" ) )
            {
                return "homePage";
            }
            else if ( key.Contains( "Next" ) )
            {
                return "nextPage";
            }
            else if ( key.Contains( "Previous" ) )
            {
                return "prevPage";
            }

            return "";
        }

        protected string AddPage( PageCache page, RockContext rockContext, InteractionService interactionService )
        {
            var sb = new StringBuilder();
            try
            {
                // Page
                string cssClass = _referencedPages.ContainsKey( page.Guid.ToString() ) ? "well" : "alert alert-danger";
                var routes = string.Join( ", ", page.PageRoutes.Select( o => o.Route ) );
                routes = string.IsNullOrWhiteSpace( routes ) ? string.Empty : "<small><b class='text-muted'>ROUTES:</b></small> " + routes;
                int pageEntityTypeId = EntityTypeCache.Read<Rock.Model.Page>().Id;
                var pageViews = interactionService.Queryable().Where( i => i.InteractionComponent.EntityId == page.Id && i.InteractionComponent.Channel.ComponentEntityTypeId == pageEntityTypeId ).AsNoTracking();
                var pageViewsCssClass = pageViews.Count() == 0 ? "text-warning" : "text-muted";

                sb.AppendFormat( @"
                <div id='{0}' class='{4}'><h2><a name='{2}'>{1}</a> <small style='font-size: 45%;'><sup>Id: {7}</sup> {3} {5}</small></h2>
                <small class='{8}'>page views: {9}</small><br/>
                <small>{6}</small>
                ", page.Guid, page.InternalName, page.Guid, CreatePageConfigIcon( page ), cssClass, routes, page.Description, page.Id, pageViewsCssClass, pageViews.Count() );

                foreach ( var block in page.Blocks )
                {
                    sb.AppendFormat( @"
                    <div id='{0}' class='alert alert-info'><h3>{1} <small>{2} {3}</small></h3> 
                        <table class='table table-condensed table-hover' style='background-color: whitesmoke;'>
                            <tr><th>Setting</th><th>Value</th></tr>", block.Guid, block.Name, CreateConfigIcon( block ), block.BlockType.Path );

                    // Page reference block attributes...
                    foreach ( var attribute in block.Attributes.Values.Where( a => a.FieldType == pageReferencefieldType ).OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                    {
                        var pageGuid = block.GetAttributeValue( attribute.Key ).ToStringSafe().ToLower();
                        if ( pageGuid.Contains( "," ) )
                        {
                            pageGuid = pageGuid.SplitDelimitedValues()[0];
                        }
                        sb.AppendFormat( "<tr><td>{0}</td><td><a href='#{2}'>{1}</a></td></tr>", attribute.Key.SplitCase(), _pageGuidsToName.ContainsKey( pageGuid ) ? _pageGuidsToName[pageGuid] : pageGuid, pageGuid );
                    }

                    // Other block attributes
                    foreach ( var attribute in block.Attributes.Values.Where( a => a.FieldType != pageReferencefieldType ).OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                    {
                        var value = block.GetAttributeValue( attribute.Key );
                        sb.AppendFormat( "<tr class='info'><td><small>{0}</small></td><td><small>{1}</small></td></tr>", attribute.Key.SplitCase(), value );
                    }

                    sb.AppendLine( "</table></div>" );
                }

                sb.AppendLine( "</div>" );
            }
            catch ( Exception ex )
            {
                nbMessage.Text = string.Format( "<pre>Error: {1} {2} {3}</pre>", ex.StackTrace, ex.ToString(), ex.InnerException.ToStringSafe() );
            }

            return sb.ToString();
        }
        
        /// <summary>
        /// Creates the block config icon.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns></returns>
        protected string CreateConfigIcon( BlockCache block )
        {
            var blockPropertyUrl = ResolveUrl( string.Format( "~/BlockProperties/{0}?t=Block Properties", block.Id ) );

            return string.Format(
                "<a class='rollover-item' href=\"javascript: Rock.controls.modal.show($(this), '{0}')\" title=\"Block Properties\"><i class=\"fa fa-cog\"></i>&nbsp;</a>",
                blockPropertyUrl );
        }

        protected string CreateSecurityIcon(int entityTypeId, int entityId, string title )
        {
            string url = this.Page.ResolveUrl( string.Format( "~/Secure/{0}/{1}?t={2}&pb=&sb=Done", entityTypeId, entityId, HttpUtility.JavaScriptStringEncode( title ) ) );
            return string.Format( "<span class='rollover-item' onclick=\"javascript: Rock.controls.modal.show($(this), '{0}'); event.stopImmediatePropagation();\" title=\"Security\"><i class=\"fa fa-lock\">&nbsp;</i></span>", url );
        }

        /// <summary>
        /// Creates the page configuration icon.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        protected string CreatePageConfigIcon( PageCache page )
        {
            var pagePropertyUrl = ResolveUrl( string.Format( "~/PageProperties/{0}?t=Page Properties", page.Id ) );

            return string.Format(
                "&nbsp;<span class='rollover-item' onclick=\"javascript: Rock.controls.modal.show($(this), '{0}'); event.stopImmediatePropagation();\" title=\"Page Properties\"><i class=\"fa fa-cog\"></i>&nbsp;</span>",
                pagePropertyUrl );
        }

        protected void lbToggleDiagram_Click( object sender, EventArgs e )
        {
            if ( _isDiagramEnabled )
            {
                _isDiagramEnabled = false;
                pnlReport.Visible = true;
                pnlDiagram.Visible = false;
            }
            else
            {
                //RockPage.AddScriptLink( "~/Plugins/com_centralaz/Cms/Scripts/cytoscape.min.js" );
                _isDiagramEnabled = true;
                pnlReport.Visible = false;
                pnlDiagram.Visible = true;
            }
        }
    }
}
 
 
 