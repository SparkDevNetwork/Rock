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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using church.ccv.Datamart.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Era
{
    /// <summary>
    /// This block lists eRA losses based on filters
    /// </summary>
    [DisplayName( "eRA Loss List" )]
    [Category( "CCV > eRA" )]
    [Description( "This block lists eRA losses based on filters." )]
    public partial class EraLossList : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.GridRebind += gList_GridRebind;
            gfList.ApplyFilterClick += gfList_ApplyFilterClick;
            gfList.ClearFilterClick += gfList_ClearFilterClick;
            gfList.DisplayFilterValue += gfList_DisplayFilterValue;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            var btnUpdate = new LinkButton();
            btnUpdate.ID = "ddlAction";
            btnUpdate.CssClass = "btn btn-primary pull-left";
            btnUpdate.CausesValidation = false;
            btnUpdate.Text = "Update";
            btnUpdate.Click += btnUpdate_Click;

            gList.Actions.AddCustomActionControl( btnUpdate );
        }

        /// <summary>
        /// Handles the Click event of the btnUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            BindGrid();
            var sfProcessed = gList.Columns.OfType<SelectField>().Where( a => a.DataSelectedField == "Processed" ).FirstOrDefault();
            var sfSendEmail = gList.Columns.OfType<SelectField>().Where( a => a.DataSelectedField == "SendEmail" ).FirstOrDefault();
            var rockContext = new RockContext();
            var datamartEraLossService = new DatamartEraLossService( rockContext );
            var dataSource = ( gList.DataSource as IEnumerable<object> ).ToList();
            foreach ( var row in gList.Rows.OfType<GridViewRow>() )
            {
                if ( row.RowType == DataControlRowType.DataRow )
                {
                    var rowData = dataSource[row.DataItemIndex];
                    if ( rowData != null )
                    {
                        var eraLossId = (int)rowData.GetPropertyValue( "Id" );
                        var processed = (bool)rowData.GetPropertyValue( "Processed" );
                        var sendEmail = (bool)rowData.GetPropertyValue( "SendEmail" );
                        var selectedProcessed = sfProcessed.SelectedKeys.OfType<int>().Contains( eraLossId );
                        var selectedSendEmail = sfSendEmail.SelectedKeys.OfType<int>().Contains( eraLossId );
                        
                        // only save ones that have changed
                        if ( selectedProcessed != processed || selectedSendEmail != sendEmail )
                        {
                            var datamartEraLoss = datamartEraLossService.Get( eraLossId );
                            if ( datamartEraLoss != null )
                            {
                                datamartEraLoss.Processed = selectedProcessed;
                                datamartEraLoss.SendEmail = selectedSendEmail;
                                rockContext.SaveChanges();
                            }
                        }
                    }
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Gfs the list_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfList_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "DateRange":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "PastorPersonId":
                    var personId = e.Value.AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( new RockContext() ).Get( personId.Value );
                        e.Value = person != null ? person.ToString() : e.Value;
                    }

                    break;
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpDates.DelimitedValues = gfList.GetUserPreference( "DateRange" );
            var personId = gfList.GetUserPreference( "PastorPersonId" ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                var person = new PersonService( new RockContext() ).Get( personId.Value );
                ppPastor.SetValue( person );
            }

            cbShowProcessed.Checked = gfList.GetUserPreference( "ShowProcessed" ).AsBooleanOrNull() ?? false;
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfList_ClearFilterClick( object sender, EventArgs e )
        {
            gfList.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfList_ApplyFilterClick( object sender, EventArgs e )
        {
            gfList.SaveUserPreference( "DateRange", "Date Range", drpDates.DelimitedValues );
            gfList.SaveUserPreference( "PastorPersonId", "Pastor", ppPastor.PersonId.ToString() );
            gfList.SaveUserPreference( "ShowProcessed", "Show Processed", cbShowProcessed.Checked.ToTrueFalse() );
            BindGrid();
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
                BindGrid();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var datamartEraLossQry = new DatamartEraLossService( rockContext ).Queryable();
            var datamartFamilyQry = new DatamartFamilyService( rockContext ).Queryable();
            var datamartNeighborhoodQry = new DatamartNeighborhoodService( rockContext ).Queryable();
            var datamartEraQry = new DatamartERAService( rockContext ).Queryable();
            var datamartPersonQry = new DatamartPersonService( rockContext ).Queryable();

            var joinEra = datamartEraLossQry.GroupJoin(
                datamartEraQry,
                l => l.FamilyId,
                e => e.FamilyId,
                ( l, e ) => new
                {
                    DatamartEraLoss = l,
                    DatamartEra = e.Where( x => x.WeekendDate == e.Max( xx => xx.WeekendDate ) ).FirstOrDefault()
                } );

            // do outer join in case they aren't in a neighboorhood
            var joinFamilyNeighboorHood =
                from f in datamartFamilyQry
                join n in datamartNeighborhoodQry on f.NeighborhoodId equals n.NeighborhoodId into fn
                from n in fn
                select new
                {
                    f.FamilyId,
                    f.HHPersonId,
                    f.HHFullName,
                    f.AdultNames,
                    f.ChildNames,
                    n.NeighborhoodPastorId,
                    n.NeighborhoodPastorName
                };

            joinFamilyNeighboorHood = joinFamilyNeighboorHood.Where( a => a.NeighborhoodPastorId.HasValue );

            var joinEraFamily = joinEra.Join(
                joinFamilyNeighboorHood,
                e => e.DatamartEraLoss.FamilyId,
                f => f.FamilyId,
                ( e, f ) => new
                {
                    Era = e,
                    Family = f
                } );

            var joinQry = joinEraFamily.Join(
                datamartPersonQry,
                ef => ef.Family.HHPersonId,
                p => p.PersonId,
                ( ef, p ) => new
                {
                    ef.Era.DatamartEraLoss.Id,
                    ef.Era.DatamartEraLoss.Processed,
                    ef.Era.DatamartEraLoss.SendEmail,
                    HHPerson = new
                    {
                        Id = ef.Family.HHPersonId,
                        FullName = ef.Family.HHFullName
                    },
                    ef.Family.AdultNames,
                    ef.Family.ChildNames,
                    ef.Era.DatamartEraLoss.LossDate,
                    ef.Era.DatamartEra.FirstAttended,
                    ef.Era.DatamartEra.LastAttended,
                    ef.Era.DatamartEra.LastGave,
                    ef.Era.DatamartEra.TimesGaveLastYear,
                    p.StartingPointDate,
                    p.BaptismDate,
                    NeighborhoodPastor = new
                    {
                        Id = ef.Family.NeighborhoodPastorId,
                        FullName = ef.Family.NeighborhoodPastorName
                    },
                    p.InNeighborhoodGroup
                } );

            /*Filter*/
            var drp = new DateRangePicker();
            drp.DelimitedValues = gfList.GetUserPreference( "DateRange" );
            if ( drp.LowerValue.HasValue )
            {
                joinQry = joinQry.Where( a => a.LossDate >= drp.LowerValue );
            }

            if ( drp.UpperValue.HasValue )
            {
                var upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                joinQry = joinQry.Where( a => a.LossDate < upperDate );
            }

            var pastorPersonId = gfList.GetUserPreference( "PastorPersonId" ).AsIntegerOrNull();
            if ( pastorPersonId.HasValue )
            {
                joinQry = joinQry.Where( a => a.NeighborhoodPastor.Id == pastorPersonId.Value );
            }

            if ( gfList.GetUserPreference( "ShowProcessed" ).AsBoolean() == false )
            {
                joinQry = joinQry.Where( a => a.Processed == false );
            }

            if ( gList.SortProperty != null )
            {
                joinQry = joinQry.Sort( gList.SortProperty );
            }
            else
            {
                joinQry = joinQry.OrderByDescending( o => o.LossDate ).ThenBy( o => o.HHPerson.FullName );
            }

            gList.SetLinqDataSource( joinQry.AsNoTracking() );
            gList.DataBind();
        }

        /// <summary>
        /// Handles the OnFormatDataValue event of the NeighborhoodPastor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CallbackField.CallbackEventArgs"/> instance containing the event data.</param>
        protected void NeighborhoodPastor_OnFormatDataValue( object sender, CallbackField.CallbackEventArgs e )
        {
            if ( e.DataValue != null )
            {
                string url = this.ResolveRockUrl( string.Format( "~/Person/{0}", e.DataValue.GetPropertyValue( "Id" ) ) );
                e.FormattedValue = string.Format( "<a href='{0}'>{1}</a>", url, e.DataValue.GetPropertyValue( "FullName" ) );
            }
        }

        /// <summary>
        /// Handles the OnFormatDataValue event of the HHPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CallbackField.CallbackEventArgs"/> instance containing the event data.</param>
        protected void HHPerson_OnFormatDataValue( object sender, CallbackField.CallbackEventArgs e )
        {
            if ( e.DataValue != null )
            {
                string url = this.ResolveRockUrl( string.Format( "~/Person/{0}", e.DataValue.GetPropertyValue( "Id" ) ) );
                e.FormattedValue = string.Format( "<a href='{0}'>{1}</a>", url, e.DataValue.GetPropertyValue( "FullName" ) );
            }
        }

        #endregion
    }
}