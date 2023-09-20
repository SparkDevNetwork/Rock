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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.bemaservices.MinistrySafe.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemaservices.MinistrySafe
{
    [DisplayName( "MinistrySafe Training List" )]
    [Category( "BEMA Services > MinistrySafe" )]
    [Description( "Lists all the MinistrySafe Trainings." )]

    [LinkedPage( "Workflow Detail Page", "The page to view details about the MinistrySafe workflow" )]
    public partial class MinistrySafeTrainingList : RockBlock, ICustomGridColumns
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fUser.ApplyFilterClick += fUser_ApplyFilterClick;
            fUser.DisplayFilterValue += fUser_DisplayFilterValue;

            gUser.DataKeyNames = new string[] { "Id" };
            gUser.Actions.ShowAdd = false;
            gUser.IsDeleteEnabled = false;
            gUser.GridRebind += gUser_GridRebind;
            gUser.RowDataBound += gUser_RowDataBound;
            gUser.RowSelected += gUser_RowSelected;
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
        /// Handles the ApplyFilterClick event of the fUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fUser_ApplyFilterClick( object sender, EventArgs e )
        {
            fUser.SaveUserPreference( "First Name", tbFirstName.Text );
            fUser.SaveUserPreference( "Last Name", tbLastName.Text );
            fUser.SaveUserPreference( "Request Date Range", drpRequestDates.DelimitedValues );
            fUser.SaveUserPreference( "Completed Date Range", drpResponseDates.DelimitedValues );

            BindGrid();
        }

        /// <summary>
        /// Displays the text of the current filters
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fUser_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Request Date Range":
                case "Completed Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gUser_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gUser_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                MinistrySafeUserRow request = e.Row.DataItem as MinistrySafeUserRow;

                if ( !request.HasWorkflow )
                {
                    foreach ( var lb in e.Row.Cells[4].ControlsOfTypeRecursive<LinkButton>() )
                    {
                        lb.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gUser_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var msu = new MinistrySafeUserService( rockContext ).Get( e.RowKeyId );
                if ( msu != null && msu.PersonAlias != null )
                {
                    int personId = e.RowKeyId;
                    Response.Redirect( string.Format( "~/Person/{0}", msu.PersonAlias.PersonId ), false );
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
        }

        /// <summary>
        /// Handles the ViewWorkflow event of the gUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gUser_ViewWorkflow( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var msu = new MinistrySafeUserService( rockContext ).Get( e.RowKeyId );
                if ( msu != null )
                {
                    var qryParms = new Dictionary<string, string> { { "WorkflowId", msu.WorkflowId.Value.ToString() } };
                    NavigateToLinkedPage( "WorkflowDetailPage", qryParms );
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbFirstName.Text = fUser.GetUserPreference( "First Name" );
            tbLastName.Text = fUser.GetUserPreference( "Last Name" );
            drpRequestDates.DelimitedValues = fUser.GetUserPreference( "Request Date Range" );
            drpResponseDates.DelimitedValues = fUser.GetUserPreference( "Completed Date Range" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var qry = new MinistrySafeUserService( rockContext )
                    .Queryable( "PersonAlias.Person" ).AsNoTracking()
                    .Where( g =>
                        g.PersonAlias != null &&
                        g.PersonAlias.Person != null )
                    .Where( g =>
                        g.ForeignId == 2 || g.ForeignId == 3 || g.ForeignId == 4 );

                // FirstName
                string firstName = fUser.GetUserPreference( "First Name" );
                if ( !string.IsNullOrWhiteSpace( firstName ) )
                {
                    qry = qry.Where( t =>
                        t.PersonAlias.Person.FirstName.StartsWith( firstName ) ||
                        t.PersonAlias.Person.NickName.StartsWith( firstName ) );
                }

                // LastName
                string lastName = fUser.GetUserPreference( "Last Name" );
                if ( !string.IsNullOrWhiteSpace( lastName ) )
                {
                    qry = qry.Where( t =>
                        t.PersonAlias.Person.LastName.StartsWith( lastName ) );
                }

                // Request Date Range
                var drpRequestDates = new DateRangePicker();
                drpRequestDates.DelimitedValues = fUser.GetUserPreference( "Request Date Range" );
                if ( drpRequestDates.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.RequestDate >= drpRequestDates.LowerValue.Value );
                }

                if ( drpRequestDates.UpperValue.HasValue )
                {
                    DateTime upperDate = drpRequestDates.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.RequestDate < upperDate );
                }

                // Response Date Range
                var drpResponseDates = new DateRangePicker();
                drpResponseDates.DelimitedValues = fUser.GetUserPreference( "Completed Date Range" );
                if ( drpResponseDates.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.CompletedDateTime >= drpResponseDates.LowerValue.Value );
                }

                if ( drpResponseDates.UpperValue.HasValue )
                {
                    DateTime upperDate = drpResponseDates.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.CompletedDateTime < upperDate );
                }

                List<com.bemaservices.MinistrySafe.Model.MinistrySafeUser> items = null;
                SortProperty sortProperty = gUser.SortProperty;
                if ( sortProperty != null )
                {
                    if ( sortProperty.Property == "Name" )
                    {
                        if ( sortProperty.Direction == SortDirection.Descending )
                        {
                            items = qry.OrderByDescending( q => q.PersonAlias.Person.LastName ).ThenBy( q => q.PersonAlias.Person.FirstName ).ToList();
                        }
                        else
                        {
                            items = qry.OrderBy( q => q.PersonAlias.Person.LastName ).ThenBy( q => q.PersonAlias.Person.FirstName ).ToList();
                        }
                    }
                    else
                    {
                        items = qry.Sort( sortProperty ).ToList();
                    }
                }
                else
                {
                    items = qry.OrderByDescending( d => d.RequestDate ).ToList();
                }

                gUser.DataSource = items.Select( b => new MinistrySafeUserRow
                {
                    Name = b.PersonAlias.Person.LastName + ", " + b.PersonAlias.Person.NickName,
                    Id = b.Id,
                    PersonId = b.PersonAlias.PersonId,
                    HasWorkflow = b.WorkflowId.HasValue,
                    RequestDate = b.RequestDate,
                    CompletedDateTime = b.CompletedDateTime,
                    Score = b.Score
                } ).ToList();

                gUser.DataBind();
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public class MinistrySafeUserRow
        {
            public string Name { get; set; }

            public int Id { get; set; }

            public int PersonId { get; set; }

            public bool HasWorkflow { get; set; }

            public DateTime RequestDate { get; set; }

            public DateTime? CompletedDateTime { get; set; }

            public int? Score { get; set; }
        }
    }
}