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
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Site List")]
    [Category("CMS")]
    [Description("Lists sites defined in the system.")]
    [LinkedPage("Detail Page")]

    //TODO Work with Mike to determine Pattern for EnumType Parameter
    [EnumsField( "Site Type", "Includes Items with the following Type.", typeof( SiteType ), true, "0", order: 1, key: AttributeKey.SiteType)]
    //[EnumsField( "Site Type",
    //    Key = AttributeKey.SiteType,
    //    Description = "Includes Items with the following Type.",
    //    IsRequired = true,
    //    DefaultValue = "0",
    //    Category = "",
    //    EnumSourceType = typeof( SiteType ),
    //    Order = 1 )]
    public partial class SiteList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys
        protected static class AttributeKey
        {
            public const string SiteType = "SiteType";
        }
        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gSites.DataKeyNames = new string[] { "Id" };
            gSites.Actions.AddClick += gSites_Add;
            gSites.GridRebind += gSites_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEdit = IsUserAuthorized( Authorization.EDIT );
            gSites.Actions.ShowAdd = canAddEdit;

            var securityField = gSites.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Site ) ).Id;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSites_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "siteId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSites_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "siteId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSites_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            SiteService siteService = new SiteService( new RockContext() );
            SortProperty sortProperty = gSites.SortProperty;
            var qry = siteService.Queryable();

            var siteType = GetAttributeValue( AttributeKey.SiteType).SplitDelimitedValues().Select( a => a.ConvertToEnumOrNull<SiteType>() ).ToList();
            //Default show inactive to false if no filter (user preference) applied. 
            bool showInactiveSites = rFilterSite.GetUserPreference( "Show Inactive" ).AsBoolean();

            // filter by block setting Site type
            qry = qry.Where( s => siteType.Contains( s.SiteType ) );

            if ( !showInactiveSites )
            {
                qry = qry.Where( s => s.IsActive == true );
            }

            if ( sortProperty != null )
            {
                gSites.DataSource = qry.Sort(sortProperty).ToList();
            }
            else
            {
                gSites.DataSource = qry.OrderBy( s => s.Name ).ToList();
            }

            gSites.EntityTypeId = EntityTypeCache.Get<Site>().Id; 
            gSites.DataBind();
        }

        protected string GetDomains( int siteID )
        {
            return new SiteDomainService( new RockContext() ).Queryable()
                .Where( d => d.SiteId == siteID )
                .OrderBy( d => d.Domain )
                .Select( d => d.Domain )
                .ToList()
                .AsDelimited( ", " );
        }

        #endregion

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilterSite control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilterSite_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilterSite.SaveUserPreference( "Show Inactive", cbShowInactive.Checked.ToString() );
            BindGrid();
        }
    }
}