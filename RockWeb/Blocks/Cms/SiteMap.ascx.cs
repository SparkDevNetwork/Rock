// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;
using System.Text;

using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using System.ComponentModel;
using Rock.Data;


namespace RockWeb.Blocks.Cms
{
    [DisplayName("Site Map")]
    [Category("CMS")]
    [Description("Displays a site map in a tree view.")]
    public partial class SiteMap : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            PageService pageService = new PageService( new RockContext() );

            var sb = new StringBuilder();

            sb.AppendLine( "<ul id=\"treeview\">" );
            var allPages = pageService.Queryable( "Pages, Blocks, Blocks.BlockType" ).ToList();
            foreach ( var page in allPages.Where( a => a.ParentPageId == null ).OrderBy( a => a.Order ).ThenBy( a => a.InternalName ) )
            {
                sb.Append( PageNode( page ) );
            }
            sb.AppendLine( "</ul>" );

            lPages.Text = sb.ToString();
        }

        /// <summary>
        /// Adds the page nodes.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        protected string PageNode( Page page )
        {
            var sb = new StringBuilder();

            sb.AppendFormat( "<li data-expanded='false' data-model='Page' data-id='p{0}'><span><i class=\"fa fa-file-o\">&nbsp;</i> <a href='{1}'>{2}</a></span>{3}", page.Id, new PageReference( page.Id ).BuildUrl(), page.InternalName, Environment.NewLine );

            if ( page.Pages.Any() || page.Blocks.Any() )
            {
                sb.AppendLine( "<ul>" );

                foreach ( var childPage in page.Pages.OrderBy( a => a.Order ).ThenBy( a => a.InternalName ) )
                {
                    sb.Append( PageNode( childPage ) );
                }

                foreach ( var block in page.Blocks.OrderBy( b => b.Order ) )
                {
                    sb.AppendFormat( "<li data-expanded='false' data-model='Block' data-id='b{0}'><span>{1}{2}:{3}</span></li>{4}", block.Id, CreateConfigIcon( block ), block.Name, block.BlockType.Name, Environment.NewLine );
                }

                sb.AppendLine( "</ul>" );
            }

            sb.AppendLine( "</li>" );

            return sb.ToString();
        }

        /// <summary>
        /// Creates the block config icon.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns></returns>
        protected string CreateConfigIcon( Block block )
        {
            var blockPropertyUrl = ResolveUrl( string.Format( "~/BlockProperties/{0}?t=Block Properties", block.Id ) );

            return string.Format( "<i class=\"fa fa-th-large\">&nbsp;</i> <a href=\"javascript: Rock.controls.modal.show($(this), '{0}')\" title=\"Block Properties\"><i class=\"fa fa-cog\"></i>&nbsp;</a>",
                blockPropertyUrl );
        }
    }
}