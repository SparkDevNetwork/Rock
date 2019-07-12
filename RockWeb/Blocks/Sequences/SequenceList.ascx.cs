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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

namespace RockWeb.Blocks.Sequences
{
    [DisplayName( "Sequence List" )]
    [Category( "Sequences" )]
    [Description( "Shows a list of all sequences." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKeys.DetailPage,
        Category = AttributeCategories.LinkedPages,
        Order = 1 )]

    #endregion Block Attributes

    public partial class SequenceList : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKeys
        {
            public const string DetailPage = "DetailPage";
        }

        /// <summary>
        /// Keys to use for Block Attribute Categories
        /// </summary>
        private static class AttributeCategories
        {
            public const string LinkedPages = "Linked Pages";
        }

        /// <summary>
        /// Keys to use for User Preferences
        /// </summary>
        private static class UserPreferenceKeys
        {
            public const string Active = "Active";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKeys
        {
            public const string SequenceId = "SequenceId";
        }

        #endregion Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Initialize Filter
            if ( !Page.IsPostBack )
            {
                BindFilter();
            }

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            // Initialize Grid
            gSequence.DataKeyNames = new string[] { "Id" };
            gSequence.Actions.AddClick += gSequence_Add;
            gSequence.GridRebind += gSequence_GridRebind;
            gSequence.RowItemText = "Sequence";

            // Initialize Grid: Secured actions
            var canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gSequence.Actions.ShowAdd = canAddEditDelete;
            gSequence.IsDeleteEnabled = canAddEditDelete;

            if ( canAddEditDelete )
            {
                gSequence.RowSelected += gSequence_Edit;
            }

            var securityField = gSequence.ColumnsOfType<SecurityField>().FirstOrDefault();
            securityField.EntityTypeId = EntityTypeCache.Get( typeof( Sequence ) ).Id;

            // Set up Block Settings change notification.
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upSequenceList );
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

        #endregion Base Control Methods

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion Control Events

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( UserPreferenceKeys.Active, ddlActiveFilter.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// ts the filter display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case UserPreferenceKeys.Active:
                    e.Value = ddlActiveFilter.SelectedValue;
                    break;
                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gSequence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSequence_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.DetailPage, PageParameterKeys.SequenceId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSequence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSequence_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.DetailPage, PageParameterKeys.SequenceId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gSequence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSequence_Delete( object sender, RowEventArgs e )
        {
            var rockContext = GetRockContext();
            var sequenceId = e.RowKeyId;
            var sequenceService = new SequenceService( rockContext );
            var sequence = sequenceService.Get( sequenceId );

            if ( sequence == null )
            {
                mdGridWarning.Show( "The sequence could not be found.", ModalAlertType.Information );
                return;
            }

            var errorMessage = string.Empty;
            if ( !sequenceService.CanDelete( sequence, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            sequenceService.Delete( sequence );
            rockContext.SaveChanges();
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSequence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gSequence_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion Grid Events

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlActiveFilter.SetValue( rFilter.GetUserPreference( UserPreferenceKeys.Active ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // Don't use the sequence cache here since the users will expect to see the instant changes to this
            // query when they add, edit, etc
            var sequenceQuery = GetSequenceService().Queryable().AsNoTracking();

            // Filter by: Active
            var activeFilter = rFilter.GetUserPreference( UserPreferenceKeys.Active ).ToLower();

            switch ( activeFilter )
            {
                case "active":
                    sequenceQuery = sequenceQuery.Where( s => s.IsActive );
                    break;
                case "inactive":
                    sequenceQuery = sequenceQuery.Where( a => !a.IsActive );
                    break;
            }

            // Create view models to display in the grid
            var viewModelQuery = sequenceQuery.Select( s => new SequenceViewModel
            {
                Id = s.Id,
                Name = s.Name,
                IsActive = s.IsActive,
                OccurrenceFrequency = s.OccurrenceFrequency,
                EnrollmentCount = s.SequenceEnrollments.Count(),
                StartDate = s.StartDate
            } );

            // Sort the view models
            var sortProperty = gSequence.SortProperty;
            if ( sortProperty != null )
            {
                viewModelQuery = viewModelQuery.Sort( sortProperty );
            }
            else
            {
                viewModelQuery = viewModelQuery.OrderBy( vm => vm.Id );
            }

            gSequence.SetLinqDataSource( viewModelQuery );
            gSequence.DataBind();
        }

        #endregion Internal Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the rock context
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            return _rockContext;
        }
        private RockContext _rockContext = null;

        /// <summary>
        /// Get the sequence service
        /// </summary>
        /// <returns></returns>
        private SequenceService GetSequenceService()
        {
            if ( _sequenceService == null )
            {
                var rockContext = GetRockContext();
                _sequenceService = new SequenceService( rockContext );
            }

            return _sequenceService;
        }
        private SequenceService _sequenceService = null;

        #endregion Data Interface Methods

        #region View Models

        /// <summary>
        /// Represents an entry in the list ofSequences shown on this page.
        /// </summary>
        private class SequenceViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
            public SequenceOccurrenceFrequency OccurrenceFrequency { get; set; }
            public int EnrollmentCount { get; set; }
            public DateTime StartDate { get; set; }
        }

        #endregion View Models
    }
}