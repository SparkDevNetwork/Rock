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
    [DisplayName( "Personalization Segment List" )]
    [Category( "Cms" )]
    [Description( "Block that lists existing Personalization Segments." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "06EC24B2-0B2A-47E0-9A1F-44587BC46099" )]
    public partial class PersonalizationSegmentList : RockBlock, ICustomGridColumns
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
            public const string SegmentName = "SegmentName";
            public const string IncludeInactive = "Include Inactive";
        }

        #endregion UserPreference Keys

        #region PageParameter Keys

        private static class PageParameterKey
        {
            public const string PersonalizationSegmentId = "PersonalizationSegmentId";
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
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
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
            var segmentService = new PersonalizationSegmentService( rockContext );
            var segment = segmentService.Get( e.RowKeyId );
            if ( segment != null )
            {
                string errorMessage;
                if ( !segmentService.CanDelete( segment, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                segmentService.Delete( segment );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfList_ApplyFilterClick( object sender, EventArgs e )
        {
            gFilter.SetFilterPreference( UserPreferenceKey.SegmentName, tbNameFilter.Text );
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
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.PersonalizationSegmentId, 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.PersonalizationSegmentId, e.RowKeyId );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbNameFilter.Text = gFilter.GetFilterPreference( UserPreferenceKey.SegmentName );
            cbShowInactive.Checked = gFilter.GetFilterPreference( UserPreferenceKey.IncludeInactive ).AsBoolean();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );

            var personAliasPersonalizationsSegmentsQry = personalizationSegmentService.GetPersonAliasPersonalizationSegmentQuery();

            var segmentQuery = personalizationSegmentService.Queryable();

            var nameFilter = tbNameFilter.Text;
            if ( nameFilter.IsNotNullOrWhiteSpace() )
            {
                segmentQuery = segmentQuery.Where( x => x.Name.Contains( nameFilter ) );
            }

            if ( !cbShowInactive.Checked )
            {
                segmentQuery = segmentQuery.Where( s => s.IsActive == true );
            }

            var anonymousVisitorPersonId = new PersonService( rockContext ).GetOrCreateAnonymousVisitorPersonId();

            var personalizationSegmentItemQuery = segmentQuery.Select( a => new PersonalizationSegmentItem
            {
                Id = a.Id,
                Name = a.Name,
                FilterDataViewName = a.FilterDataViewId.HasValue ? a.FilterDataView.Name : null,
                IsActive = a.IsActive,
                AnonymousIndividualsCount =
                     personAliasPersonalizationsSegmentsQry
                         .Where( p => p.PersonalizationEntityId == a.Id && p.PersonAlias.PersonId == anonymousVisitorPersonId ).Count(),
                KnownIndividualsCount =
                     personAliasPersonalizationsSegmentsQry
                         .Where( p => p.PersonalizationEntityId == a.Id && p.PersonAlias.PersonId != anonymousVisitorPersonId ).Count(),
                Guid = a.Guid,
                TimeToUpdateDurationMilliseconds = a.TimeToUpdateDurationMilliseconds
            } );

            // sort the query based on the column that was selected to be sorted
            var sortProperty = gList.SortProperty;
            if ( gList.AllowSorting && sortProperty != null )
            {
                personalizationSegmentItemQuery = personalizationSegmentItemQuery.Sort( sortProperty );
            }
            else
            {
                personalizationSegmentItemQuery = personalizationSegmentItemQuery.OrderBy( a => a.Name );
            }

            gList.SetLinqDataSource( personalizationSegmentItemQuery );
            gList.DataBind();
        }

        #endregion

        private class PersonalizationSegmentItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string FilterDataViewName { get; set; }
            public bool IsActive { get; set; }
            public int AnonymousIndividualsCount { get; set; }
            public int KnownIndividualsCount { get; set; }
            public Guid Guid { get; set; }
            public double? TimeToUpdateDurationMilliseconds { get; set; }
        }
    }
}