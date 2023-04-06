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
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Displays Assessment History on the Person Profile's History tab. Allows a person to see and delete (if needed) pending
    /// assessment requests.
    /// </summary>
    [DisplayName( "Assessment History" )]
    [Category( "CRM" )]
    [Description( "Displays Assessment History on the Person Profile's History tab. Allows a person to see and delete (if needed) pending assessment requests." )]
    [Rock.SystemGuid.BlockTypeGuid( "E7EB1E42-FEA7-4735-83FE-A618BD2616BF" )]
    public partial class AssessmentHistory : RockBlock, ICustomGridColumns
    {
        #region Keys

        /// <summary>
        /// Keys for the filter preferences
        /// </summary>
        private static class FilterKey
        {
            /// <summary>
            /// The assessment type identifier
            /// </summary>
            public const string AssessmentTypeId = "AssessmentTypeId";
        }

        /// <summary>
        /// Keys of Page Params
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The person identifier
            /// </summary>
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
            gAssessments.DataKeyNames = new string[] { "AssessmentId" };
            gAssessments.GridRebind += gAssessments_GridRebind;

            var entityType = EntityTypeCache.Get<Assessment>();
            gAssessments.EntityTypeId = entityType.Id;
            gAssessments.RowItemText = entityType.FriendlyName;
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

        #endregion Control Methods

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            var assessmentTypeId = atpAssessmentType.SelectedValueAsInt();
            rFilter.SetFilterPreference( FilterKey.AssessmentTypeId, assessmentTypeId.HasValue ? assessmentTypeId.Value.ToString() : string.Empty );

            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case FilterKey.AssessmentTypeId:
                    var assessmentTypeId = e.Value.AsIntegerOrNull();

                    if ( !assessmentTypeId.HasValue )
                    {
                        break;
                    }

                    var assessmentTypeCache = AssessmentTypeCache.Get( assessmentTypeId.Value );

                    if ( assessmentTypeCache != null )
                    {
                        e.Value = assessmentTypeCache.Title;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            if ( !UserCanDelete() )
            {
                return;
            }

            // Get the id of the assessment from the command argument
            var button = sender as LinkButton;
            var assessmentId = button.CommandArgument.AsInteger();

            // Use a query to delete the record with a single DB trip. Safeguard the query to not delete complete assessments.
            var rockContext = new RockContext();
            var assessmentService = new AssessmentService( rockContext );
            var query = assessmentService.Queryable().Where( a => a.Id == assessmentId && a.Status == AssessmentRequestStatus.Pending );
            assessmentService.DeleteRange( query );

            rockContext.SaveChanges();
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAssessments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAssessments_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAssessments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAssessments_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var assessmentViewModel = e.Row.DataItem as AssessmentViewModel;
            var btnDelete = e.Row.FindControl( "btnDelete" ) as LinkButton;

            if ( assessmentViewModel.IsCompleted || !UserCanDelete() )
            {
                btnDelete.Visible = false;
            }
            else
            {
                btnDelete.Attributes["onclick"] = string.Format(
                    "javascript: return Rock.dialogs.confirmDelete(event, '{0}');",
                    Assessment.FriendlyTypeName );
            }
        }

        #endregion Grid Events

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var assessmentTypeId = rFilter.GetFilterPreference( FilterKey.AssessmentTypeId ).AsIntegerOrNull();
            atpAssessmentType.SetValue( assessmentTypeId );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var assessmentService = new AssessmentService( rockContext );
            var query = assessmentService.Queryable( "RequesterPersonAlias.Person, AssessmentType" );

            // Filter by the person id in the context
            var personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

            if ( personId.HasValue )
            {
                query = query.Where( a => a.PersonAlias.PersonId == personId.Value );
            }

            // Filter by assessment type if specified by the user
            var assessmentTypeId = rFilter.GetFilterPreference( FilterKey.AssessmentTypeId ).AsIntegerOrNull();

            if ( assessmentTypeId.HasValue )
            {
                query = query.Where( a => a.AssessmentTypeId == assessmentTypeId.Value );
            }

            // Transform to view models, which the sort options are based upon. Cannot use FullName here because it will throw a linq to entity
            // exception since linq cannot use the FullName property.
            var viewModelQuery = query.Select( a => new AssessmentViewModel
            {
                AssessmentId = a.Id,
                AssessmentTypeTitle = a.AssessmentType.Title,
                IsCompleted = a.Status == AssessmentRequestStatus.Complete,
                RequestedDateTime = a.RequestedDateTime,
                RequesterPersonFullName =
                    ( string.IsNullOrEmpty( a.RequesterPersonAlias.Person.NickName ) ?
                        a.RequesterPersonAlias.Person.FirstName :
                        a.RequesterPersonAlias.Person.NickName ) +
                    " " + a.RequesterPersonAlias.Person.LastName,
                Status = a.Status,
                StatusText = a.Status == AssessmentRequestStatus.Complete ? "Complete" : "Pending",
                CompletedDateTime = a.CompletedDateTime
            } );

            // Sort if specified by the user
            var sortProperty = gAssessments.SortProperty;

            if ( sortProperty != null )
            {
                viewModelQuery = viewModelQuery.Sort( sortProperty );
            }
            else
            {
                viewModelQuery = viewModelQuery.OrderByDescending( a => a.RequestedDateTime ).ThenBy( a => a.AssessmentTypeTitle );
            }

            gAssessments.SetLinqDataSource( viewModelQuery );
            gAssessments.DataBind();
        }

        #endregion Internal Methods

        #region Data Helpers

        /// <summary>
        /// Is the user allowed to delete a pending assessment?
        /// </summary>
        /// <returns></returns>
        private bool UserCanDelete()
        {
            return IsUserAuthorized( Authorization.EDIT );
        }

        #endregion Data Helpers

        #region View Model

        /// <summary>
        /// Assessment View Model for the grid
        /// </summary>
        protected class AssessmentViewModel
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int AssessmentId { get; set; }

            /// <summary>
            /// Gets or sets the assessment type title.
            /// </summary>
            /// <value>
            /// The assessment type title.
            /// </value>
            public string AssessmentTypeTitle { get; set; }

            /// <summary>
            /// Gets or sets the status.
            /// </summary>
            /// <value>
            /// The status.
            /// </value>
            public AssessmentRequestStatus Status { get; set; }

            /// <summary>
            /// Gets or sets the status text.
            /// </summary>
            /// <value>
            /// The status text.
            /// </value>
            public string StatusText { get; set; }

            /// <summary>
            /// Gets or sets the requested date time.
            /// </summary>
            /// <value>
            /// The requested date time.
            /// </value>
            public DateTime? RequestedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the full name of the requester person.
            /// </summary>
            /// <value>
            /// The full name of the requester person.
            /// </value>
            public string RequesterPersonFullName { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is completed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is completed; otherwise, <c>false</c>.
            /// </value>
            public bool IsCompleted { get; set; }

            /// <summary>
            /// Gets the completed date time.
            /// </summary>
            /// <value>
            /// The completed date time.
            /// </value>
            public DateTime? CompletedDateTime { get; internal set; }
        }

        #endregion View Model
    }
}