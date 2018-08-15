using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

using Rock.Model;
using Rock.Web.UI.Controls;
using System.Text.RegularExpressions;
using Rock.Data;
using Rock.Web.Cache;

namespace RockWeb.Plugins.org_newpointe.ExtraSearch
{

    /// <summary>
    /// Block to pick a person and get their URL encoded key.
    /// </summary>
    [DisplayName( "Page Search" )]
    [Category( "Extra Search" )]
    [Description( "Displays list of pages that match a given search term." )]

    public partial class PageSearch : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            BindGrid();
        }

        #endregion

        #region Events

        protected void gPeople_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToPage( ( Guid ) e.RowKeyValue, null );
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            string term = PageParameter( "SearchTerm" );

            if ( !string.IsNullOrWhiteSpace( term ) )
            {

                var pages = org.newpointe.ExtraSearch.PageTitle.SearchPages(term).Take(50).ToList();
    
                if ( pages.Count == 1 )
                {
                    NavigateToPage( pages[0].Guid, null );
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    gPages.DataKeyNames = new [] { "Guid" };
                    gPages.EntityTypeId = EntityTypeCache.GetId<Page>();
                    gPages.DataSource = pages.Select( p => new
                    {
                        p.Guid,
                        p.PageTitle,
                        Structure = ParentStructure( p ),
                        Site = p.Layout.Site.Name
                    } ).ToList();
                    gPages.DataBind();
                }
            }
        }

        private string ParentStructure( Page page, List<int> parentIds = null )
        {
            if ( page == null )
            {
                return string.Empty;
            }

            // Create or add this node to the history stack for this tree walk.
            if ( parentIds == null )
            {
                parentIds = new List<int>();
            }
            else
            {
                // If we have encountered this node before during this tree walk, we have found an infinite recursion in the tree.
                // Truncate the path with an error message and exit.
                if ( parentIds.Contains( page.Id ) )
                {
                    return "#Invalid-Parent-Reference#";
                }
            }

            parentIds.Add( page.Id );

            string prefix = ParentStructure( page.ParentPage, parentIds );

            if ( !string.IsNullOrWhiteSpace( prefix ) )
            {
                prefix += " <i class='fa fa-angle-right'></i> ";
            }

            string pageUrl = RockPage.ResolveUrl( "~/Page/" );

            return string.Format( "{0}<a href='{1}{2}'>{3}</a>", prefix, pageUrl, page.Id, page.PageTitle );
        }
        #endregion
    }
}