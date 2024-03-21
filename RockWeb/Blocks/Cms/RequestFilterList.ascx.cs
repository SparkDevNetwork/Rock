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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Request Filter List" )]
    [Category( "Cms" )]
    [Description( "Block that lists existing Request Filters." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "650E16B0-8B97-4336-9CE0-EAF8AAC20BDF" )]
    public partial class RequestFilterList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region UserPreference Keys

        private static class UserPreferenceKey
        {
            public const string RequestFilterName = "RequestFilterName";
            public const string IncludeInactive = "Include Inactive";
        }

        #endregion UserPreference Keys

        #region PageParameter Keys

        private static class PageParameterKey
        {
            public const string RequestFilterId = "RequestFilterId";
        }

        #endregion PageParameter Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.GridRebind += gList_GridRebind;
            gList.DataKeyNames = new string[] { "Id" };

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gList.Actions.ShowAdd = canAddEditDelete;
            gList.Actions.AddClick += gList_AddClick;

            gList.IsDeleteEnabled = canAddEditDelete;
            gFilter.DisplayFilterValue += gFilter_DisplayFilterValue;

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

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the DeleteClick event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_DeleteClick( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var requestFilterService = new RequestFilterService( rockContext );
            var requestFilter = requestFilterService.Get( e.RowKeyId );
            if ( requestFilter != null )
            {
                string errorMessage;
                if ( !requestFilterService.CanDelete( requestFilter, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                requestFilterService.Delete( requestFilter );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfList_ApplyFilterClick( object sender, EventArgs e )
        {
            gFilter.SetFilterPreference( UserPreferenceKey.RequestFilterName, tbNameFilter.Text );
            gFilter.SetFilterPreference( UserPreferenceKey.IncludeInactive, cbShowInactive.Checked ? cbShowInactive.Checked.ToString() : string.Empty );
            BindGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gFilter control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case UserPreferenceKey.IncludeInactive:
                    var includeFilterValue = e.Value.AsBooleanOrNull();
                    if ( includeFilterValue.HasValue && includeFilterValue.Value )
                    {
                        e.Value = includeFilterValue.Value.ToYesNo();
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfList_ClearFilterClick( object sender, EventArgs e )
        {
            gFilter.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the AddClick event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.RequestFilterId, 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.RequestFilterId, e.RowKeyId );
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbNameFilter.Text = gFilter.GetFilterPreference( UserPreferenceKey.RequestFilterName );
            cbShowInactive.Checked = gFilter.GetFilterPreference( UserPreferenceKey.IncludeInactive ).AsBoolean();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            RequestFilterService requestFilterService = new RequestFilterService( rockContext );

            var anonymousVisitorPersonGuid = Rock.SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid();

            var personAliasPersonalizationsQry = rockContext.Set<PersonAliasPersonalization>();

            var requestFilterQuery = requestFilterService.Queryable();

            var nameFilter = tbNameFilter.Text;
            if ( nameFilter.IsNotNullOrWhiteSpace() )
            {
                requestFilterQuery = requestFilterQuery.Where( x => x.Name.Contains( nameFilter ) );
            }

            if ( !cbShowInactive.Checked )
            {
                requestFilterQuery = requestFilterQuery.Where( s => s.IsActive == true );
            }

            var personalizationRequestFilterItemQuery = requestFilterQuery.Select( a => new PersonalizationRequestFilterItem
            {
                Id = a.Id,
                Name = a.Name,
                SiteName = a.Site.Name,
                IsActive = a.IsActive
            } );

            // Sort the query based on the column that was selected to be sorted. If no property is specified, sort by the name.
            var sortProperty = gList.SortProperty;
            if ( gList.AllowSorting && sortProperty != null )
            {
                personalizationRequestFilterItemQuery = personalizationRequestFilterItemQuery.Sort( sortProperty );
            }
            else
            {
                personalizationRequestFilterItemQuery = personalizationRequestFilterItemQuery.OrderBy( a => a.Name );
            }

            // Set the datasource as a query. This allows the grid to only fetch the records that need to be shown based on the grid page and page size.
            gList.SetLinqDataSource( personalizationRequestFilterItemQuery );
            gList.DataBind();
        }

        #endregion

        private class PersonalizationRequestFilterItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string SiteName { get; set; }
            public bool IsActive { get; set; }
        }
    }
}