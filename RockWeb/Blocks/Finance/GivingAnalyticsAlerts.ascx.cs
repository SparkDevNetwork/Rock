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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Lists of current alerts based on current filters.
    /// </summary>
    [DisplayName( "Giving Analytics Alerts" )]
    [Category( "Finance" )]
    [Description( "Lists of current alerts based on current filters." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Order = 0,
        Key = AttributeKey.TransactionPage )]

    #endregion  Block Attributes
    public partial class GivingAnalyticsAlerts : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string TransactionPage = "TransactionPage";
        }

        #endregion Attribute Keys
        #region Page Parameter Constants

        private static class PageParameterKey
        {
            /// <summary>
            /// The person identifier
            /// </summary>
            public const string PersonGuid = "PersonGuid";

            /// <summary>
            /// The campus identifier
            /// </summary>
            public const string CampusId = "CampusId";
        }

        #endregion

        #region Filter Keys

        /// <summary>
        /// Keys to use for Filters
        /// </summary>
        private static class FilterKey
        {
            public const string DateRange = "DateRange";
            public const string Person = "Person";
            public const string AlertType = "AlertType";
            public const string Campus = "Campus";
            public const string Transaction = "Transaction";
            public const string Note = "Note";
        }

        #endregion Filter Keys

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfAlertFilter.ApplyFilterClick += gfAlertFilter_ApplyFilterClick;
            gfAlertFilter.DisplayFilterValue += gfAlertFilter_DisplayFilterValue;
            gfAlertFilter.ClearFilterClick += gfAlertFilter_ClearFilterClick;

            gAlertList.DataKeyNames = new string[] { "Id" };
            gAlertList.GridRebind += gAlertList_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlAlerts );
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

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindFilter();
            BindGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfAlertFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs"/> instance containing the event data.</param>
        protected void gfAlertFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case FilterKey.DateRange:
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }

                case FilterKey.Person:
                    {
                        int? personId = e.Value.AsIntegerOrNull();
                        if ( personId != null && ppPerson.Visible )
                        {
                            var person = new PersonService( new RockContext() ).Get( personId.Value );
                            if ( person != null )
                            {
                                e.Value = person.ToString();
                            }
                            else
                            {
                                e.Value = string.Empty;
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case FilterKey.AlertType:
                    {
                        e.Value = GetAlertTypeNames( e.Value, cblAlertType );
                        break;
                    }

                case FilterKey.Campus:
                    {
                        var campusId = e.Value.AsIntegerOrNull();
                        if ( campusId.HasValue && cpCampus.Visible )
                        {
                            var campusCache = CampusCache.Get( campusId.Value );
                            if ( campusCache != null )
                            {
                                e.Value = campusCache.Name;
                            }
                            else
                            {
                                e.Value = string.Empty;
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case FilterKey.Transaction:
                    {
                        e.Value = NumberRangeEditor.FormatDelimitedValues( e.Value, "N2" );
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfAlertFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfAlertFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfAlertFilter.SaveUserPreference( FilterKey.AlertType, FilterKey.AlertType.SplitCase(), cblAlertType.SelectedValues.AsDelimited( ";" ) );
            gfAlertFilter.SaveUserPreference( FilterKey.DateRange, FilterKey.DateRange.SplitCase(), drpDateRange.DelimitedValues );
            gfAlertFilter.SaveUserPreference( FilterKey.Person, FilterKey.Person, ppPerson.SelectedValue.ToString() );
            gfAlertFilter.SaveUserPreference( FilterKey.Transaction, FilterKey.Transaction, nreTransaction.DelimitedValues );
            gfAlertFilter.SaveUserPreference( FilterKey.Campus, FilterKey.Campus, cpCampus.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfAlertFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfAlertFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfAlertFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAlertList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAlertList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAlertList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAlertList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var alert = e.Row.DataItem as FinancialTransactionAlert;

            if ( alert == null )
            {
                return;
            }

            var cssClass = alert.FinancialTransactionAlertType.AlertType == AlertType.Gratitude ? "success" : "warning";
            bool isExporting = false;
            if ( e is RockGridViewRowEventArgs )
            {
                isExporting = ( e as RockGridViewRowEventArgs ).IsExporting;
            }

            // Status icons
            var lStatusIcons = e.Row.FindControl( "lStatusIcons" ) as Literal;

            if ( lStatusIcons != null )
            {
                if ( isExporting )
                {
                    lStatusIcons.Text = alert.FinancialTransactionAlertType.AlertType.ToString();
                }
                else
                {
                    lStatusIcons.Text = string.Format(
                        "<span class='badge badge-{1} badge-circle' data-toggle='tooltip' data-original-title='{0}' style='inline-block'><span class='sr-only'></span></span>",
                        alert.FinancialTransactionAlertType.AlertType.ToString(),
                        cssClass );
                }
            }

            bool isAlertByAmountRaised = IsAlertByAmountRaised( alert );
            if ( alert.Amount.HasValue )
            {
                var lGiftAmount = e.Row.FindControl( "lGiftAmount" ) as Literal;
                if ( lGiftAmount != null )
                {
                    var transactionPageRoute = LinkedPageRoute( AttributeKey.TransactionPage );
                    if ( transactionPageRoute.IsNotNullOrWhiteSpace() && alert.TransactionId.HasValue && !isExporting )
                    {
                        var cell = e.Row.Cells.OfType<DataControlFieldCell>().Where( a => a == lGiftAmount.FirstParentControlOfType<DataControlFieldCell>() ).First();
                        cell.RemoveCssClass( "grid-select-cell" );
                        lGiftAmount.Text = string.Format( "<a href='{0}?TransactionId={1}'>{2}</a>", transactionPageRoute, alert.TransactionId, alert.Amount.FormatAsCurrency() );
                    }
                    else
                    {
                        lGiftAmount.Text = alert.Amount.FormatAsCurrency();
                    }
                }

                var lAmountMedian = e.Row.FindControl( "lAmountMedian" ) as Literal;
                if ( lAmountMedian != null && alert.AmountCurrentMedian.HasValue )
                {
                    if ( isAlertByAmountRaised && !isExporting )
                    {
                        lAmountMedian.Text = string.Format( "<span class='label label-{1}'>{0}</span>", ( alert.Amount - alert.AmountCurrentMedian ).FormatAsCurrency(), cssClass );
                    }
                    else
                    {
                        lAmountMedian.Text = string.Format( "{0}", ( alert.Amount - alert.AmountCurrentMedian ).FormatAsCurrency() );
                    }
                }
            }

            var lDaysMean = e.Row.FindControl( "lDaysMean" ) as Literal;
            if ( lDaysMean != null && alert.FrequencyDifferenceFromMean.HasValue )
            {
                if ( !isAlertByAmountRaised && !isExporting )
                {
                    lDaysMean.Text = string.Format( "<span class='label label-{1}'>{0}</span>", alert.FrequencyDifferenceFromMean, cssClass );
                }
                else
                {
                    lDaysMean.Text = string.Format( "{0}", alert.FrequencyDifferenceFromMean );
                }
            }

            var lAmtMeasures = e.Row.FindControl( "lAmtMeasures" ) as Literal;
            if ( lAmtMeasures != null )
            {
                lAmtMeasures.Text = string.Format( "{0}<span class='small text-muted'> {1} IQR</span>", alert.AmountCurrentMedian.FormatAsCurrency(), alert.AmountCurrentIqr );
            }

            var lFreqMeasures = e.Row.FindControl( "lFreqMeasures" ) as Literal;
            if ( lFreqMeasures != null )
            {
                lFreqMeasures.Text = string.Format( "{0}<span class='small text-muted'> {1} IQR</span>", alert.FrequencyCurrentMean, alert.FrequencyCurrentStandardDeviation );
            }
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Get the person through query list or context.
        /// </summary>
        private int? GetPerson( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var personGuid = PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();

            if ( personGuid.HasValue )
            {
                return new PersonService( rockContext ).GetId( personGuid.Value );
            }

            return null;
        }

        /// <summary>
        /// Get the campus through query list or context.
        /// </summary>
        private int? GetCampusFromQuery()
        {
            var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            if ( campusId.HasValue )
            {
                var campus = CampusCache.Get( campusId.Value );
                if ( campus != null )
                {
                    return campus.Id;
                }
            }

            return null;
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpDateRange.DelimitedValues = gfAlertFilter.GetUserPreference( FilterKey.DateRange );

            cblAlertType.BindToEnum<AlertType>();
            string alertTypeValue = gfAlertFilter.GetUserPreference( FilterKey.AlertType );
            if ( !string.IsNullOrWhiteSpace( alertTypeValue ) )
            {
                cblAlertType.SetValues( alertTypeValue.Split( ';' ).ToList() );
            }

            // don't show the person picker if the the current context is already a specific person
            if ( GetPerson() != null )
            {
                ppPerson.Visible = false;
            }
            else
            {
                ppPerson.Visible = true;
                var personId = gfAlertFilter.GetUserPreference( FilterKey.Person ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var person = new PersonService( new RockContext() ).Get( personId.Value );
                    ppPerson.SetValue( person );
                }
            }

            nreTransaction.DelimitedValues = gfAlertFilter.GetUserPreference( FilterKey.Transaction );

            var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            if ( GetCampusFromQuery() != null )
            {
                cpCampus.Visible = false;
            }
            else
            {
                cpCampus.ForceVisible = true;
                cpCampus.SelectedCampusId = gfAlertFilter.GetUserPreference( FilterKey.Campus ).AsIntegerOrNull();
            }
        }

        /// <summary>
        /// Get the alert type names.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string GetAlertTypeNames( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var financialTransactionAlertQry = new FinancialTransactionAlertService( rockContext ).Queryable();

            var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateRange.DelimitedValues );

            if ( dateRange.Start.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( se => se.AlertDateTime >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( se => se.AlertDateTime <= dateRange.End.Value );
            }

            // Filter by Group Member Status
            var alertTypes = new List<AlertType>();
            foreach ( string alertType in cblAlertType.SelectedValues )
            {
                if ( !string.IsNullOrWhiteSpace( alertType ) )
                {
                    alertTypes.Add( alertType.ConvertToEnum<AlertType>() );
                }
            }

            if ( alertTypes.Any() )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( m => alertTypes.Contains( m.FinancialTransactionAlertType.AlertType ) );
            }

            var personId = GetPerson( rockContext );
            if ( !personId.HasValue && ppPerson.Visible )
            {
                personId = ppPerson.SelectedValue;
            }

            if ( personId.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( a => a.PersonAlias.PersonId == personId.Value );
            }

            if ( nreTransaction.LowerValue.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( a => a.Amount >= nreTransaction.LowerValue.Value );
            }

            if ( nreTransaction.UpperValue.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( a => a.Amount <= nreTransaction.UpperValue.Value );
            }

            var campusId = GetCampusFromQuery();

            if ( campusId.HasValue )
            {
                var campusField = gAlertList.ColumnsOfType<RockBoundField>().Where( a => a.HeaderText == "Campus" ).FirstOrDefault();
                if ( campusField != null )
                {
                    campusField.Visible = false;
                }
            }

            if ( !campusId.HasValue && cpCampus.Visible )
            {
                campusId = cpCampus.SelectedCampusId;
            }

            if ( campusId.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry
                        .Where( a => a.FinancialTransactionAlertType.CampusId.HasValue || a.FinancialTransactionAlertType.CampusId == campusId.Value );
            }

            gAlertList.EntityTypeId = EntityTypeCache.Get<FinancialTransactionAlert>().Id;
            var sortProperty = gAlertList.SortProperty;
            gAlertList.DataSource = sortProperty != null ? financialTransactionAlertQry.Sort( sortProperty ).ToList() : financialTransactionAlertQry.OrderByDescending( p => p.AlertDateTime ).ToList();
            gAlertList.DataBind();
        }

        private bool IsAlertByAmountRaised( FinancialTransactionAlert alert )
        {
            if ( alert.Amount.HasValue &&
                ( alert.FinancialTransactionAlertType.MinimumGiftAmount.HasValue || alert.FinancialTransactionAlertType.MaximumGiftAmount.HasValue ) )
            {
                var minimumGiftAmount = alert.FinancialTransactionAlertType.MinimumGiftAmount ?? 0.00M;
                var maximumGiftAmount = alert.FinancialTransactionAlertType.MaximumGiftAmount ?? decimal.MaxValue;
                if ( alert.Amount >= minimumGiftAmount && alert.Amount <= maximumGiftAmount )
                {
                    return true;
                }
            }

            if ( alert.AmountCurrentMedian.HasValue &&
                ( alert.FinancialTransactionAlertType.MinimumMedianGiftAmount.HasValue || alert.FinancialTransactionAlertType.MaximumMedianGiftAmount.HasValue ) )
            {
                var minimumMedianGiftAmount = alert.FinancialTransactionAlertType.MinimumMedianGiftAmount ?? 0.00M;
                var maximumMedianGiftAmount = alert.FinancialTransactionAlertType.MaximumMedianGiftAmount ?? decimal.MaxValue;
                if ( alert.AmountCurrentMedian >= minimumMedianGiftAmount && alert.AmountCurrentMedian <= maximumMedianGiftAmount )
                {
                    return true;
                }
            }

            return false;
        }

        #endregion Internal Methods
    }
}