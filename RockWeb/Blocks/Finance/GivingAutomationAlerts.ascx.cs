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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// </summary>
    [DisplayName( "Giving Automation Alerts" )]
    [Category( "Finance" )]
    [Description( "Lists of current alerts based on current filters." )]

    #region Block Attributes

    [LinkedPage(
        "Transaction Detail Page",
        Description = "The transaction detail page",
        DefaultValue = Rock.SystemGuid.Page.TRANSACTION_DETAIL_TRANSACTIONS,
        Order = 0,
        Key = AttributeKey.TransactionPage )]

    [LinkedPage(
        "Automation Configuration Page",
        Description = "The page to configure what criteria should be used to generate alerts.",
        Order = 1,
        Key = AttributeKey.ConfigPage )]

    #endregion  Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "0A813EC3-EC36-499B-9EBD-C3388DC7F49D" )]
    public partial class GivingAutomationAlerts : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string TransactionPage = "TransactionPage";
            public const string ConfigPage = "ConfigPage";
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
            /// The person identifier
            /// </summary>
            public const string PersonId = "PersonId";

            /// <summary>
            /// The campus identifier
            /// </summary>
            public const string CampusId = "CampusId";

            /// <summary>
            /// The start date
            /// </summary>
            public const string StartDate = "StartDate";

            /// <summary>
            /// The end date
            /// </summary>
            public const string EndDate = "EndDate";

            /// <summary>
            /// The alert type id
            /// </summary>
            public const string AlertTypeId = "AlertTypeId";
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
            public const string AlertCategory = "AlertCategory";
            public const string AlertTypes = "AlertTypes";
            public const string Campus = "Campus";
            public const string TransactionAmount = "TransactionAmount";
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

            // Only show the config page if configured
            lbConfig.Visible = !GetAttributeValue( AttributeKey.ConfigPage ).IsNullOrWhiteSpace();
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
        /// Handles the Click event of the lbConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConfig_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.ConfigPage );
        }

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

                case FilterKey.AlertCategory:
                    {
                        e.Value = GetSelectedValues( e.Value, cblAlertCategory );
                        break;
                    }

                case FilterKey.AlertTypes:
                    {
                        e.Value = GetSelectedValues( e.Value, cblAlertTypes );
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

                case FilterKey.TransactionAmount:
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
            gfAlertFilter.SetFilterPreference( FilterKey.AlertTypes, FilterKey.AlertTypes.SplitCase(), cblAlertTypes.SelectedValues.AsDelimited( ";" ) );
            gfAlertFilter.SetFilterPreference( FilterKey.AlertCategory, FilterKey.AlertCategory.SplitCase(), cblAlertCategory.SelectedValues.AsDelimited( ";" ) );
            gfAlertFilter.SetFilterPreference( FilterKey.DateRange, FilterKey.DateRange.SplitCase(), drpDateRange.DelimitedValues );
            gfAlertFilter.SetFilterPreference( FilterKey.Person, FilterKey.Person, ppPerson.SelectedValue.ToString() );
            gfAlertFilter.SetFilterPreference( FilterKey.TransactionAmount, FilterKey.TransactionAmount, nreTransactionAmount.DelimitedValues );
            gfAlertFilter.SetFilterPreference( FilterKey.Campus, FilterKey.Campus, cpCampus.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfAlertFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfAlertFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfAlertFilter.DeleteFilterPreferences();
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

            // Determine if the alert was caused by amount, frequency, or both
            var reasons = alert.ReasonsKey.FromJsonOrNull<string[]>() ?? new string[0];
            var isFrequencyAlert = reasons.Contains( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ) );
            var isAmountAlert = reasons.Contains( nameof( FinancialTransactionAlertType.AmountSensitivityScale ) );

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
                    if ( isAmountAlert && !isExporting )
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
                if ( isFrequencyAlert && !isExporting )
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
                lAmtMeasures.Text = $"{alert.AmountCurrentMedian.FormatAsCurrency()}<bdi class='small text-muted'> {(alert.AmountCurrentIqr ?? 0m):0.0} IQR</bdi>";
            }

            var lFreqMeasures = e.Row.FindControl( "lFreqMeasures" ) as Literal;
            if ( lFreqMeasures != null )
            {

                lFreqMeasures.Text = $"{alert.FrequencyCurrentMean:0.0}<span class='small text-muted'> {alert.FrequencyCurrentStandardDeviation:0.0}d σ</span>";
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
            var personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

            if ( personId.HasValue )
            {
                return personId;
            }

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
            // Set the date range
            var startDate = PageParameter( PageParameterKey.StartDate ).AsDateTime();
            var endDate = PageParameter( PageParameterKey.EndDate ).AsDateTime();

            if ( startDate.HasValue || endDate.HasValue )
            {
                drpDateRange.Visible = false;
            }
            else
            {
                drpDateRange.DelimitedValues = gfAlertFilter.GetFilterPreference( FilterKey.DateRange );
            }

            // Bind alert types and categories if there is no query param
            var alertTypeId = PageParameter( PageParameterKey.AlertTypeId ).AsIntegerOrNull();

            if ( alertTypeId.HasValue )
            {
                cblAlertTypes.Visible = false;
                cblAlertCategory.Visible = false;
            }
            else
            {
                // Bind alert types: the names of the alert types
                using ( var rockContext = new RockContext() )
                {
                    var alertTypeService = new FinancialTransactionAlertTypeService( rockContext );

                    cblAlertTypes.DataTextField = "Value";
                    cblAlertTypes.DataValueField = "Key";
                    cblAlertTypes.DataSource = alertTypeService.Queryable()
                        .AsNoTracking()
                        .Select( at => new
                        {
                            Key = at.Id,
                            Value = at.Name
                        } )
                        .ToList();

                    cblAlertTypes.DataBind();
                }

                var alertTypesValue = gfAlertFilter.GetFilterPreference( FilterKey.AlertTypes );

                if ( !string.IsNullOrWhiteSpace( alertTypesValue ) )
                {
                    cblAlertTypes.SetValues( alertTypesValue.Split( ';' ).ToList() );
                }

                // Bind alert categories: gratitude and follow-up
                cblAlertCategory.BindToEnum<AlertType>();
                var alertCategoryValue = gfAlertFilter.GetFilterPreference( FilterKey.AlertCategory );

                if ( !string.IsNullOrWhiteSpace( alertCategoryValue ) )
                {
                    cblAlertCategory.SetValues( alertCategoryValue.Split( ';' ).ToList() );
                }
            }

            // Don't show the person picker if the current context is already a specific person.
            if ( GetPerson() != null )
            {
                ppPerson.Visible = false;
            }
            else
            {
                ppPerson.Visible = true;
                var personId = gfAlertFilter.GetFilterPreference( FilterKey.Person ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var person = new PersonService( new RockContext() ).Get( personId.Value );
                    ppPerson.SetValue( person );
                }
                else
                {
                    ppPerson.SetValue( null );
                }
            }

            // Set the transaction amount filter
            nreTransactionAmount.DelimitedValues = gfAlertFilter.GetFilterPreference( FilterKey.TransactionAmount );

            // Campus picker
            if ( GetCampusFromQuery() != null )
            {
                cpCampus.Visible = false;
            }
            else
            {
                cpCampus.SelectedCampusId = gfAlertFilter.GetFilterPreference( FilterKey.Campus ).AsIntegerOrNull();
            }
        }

        /// <summary>
        /// Get the alert type names.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string GetSelectedValues( string values, CheckBoxList checkBoxList )
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

            // Filter by date range
            var startDate = PageParameter( PageParameterKey.StartDate ).AsDateTime()?.Date;
            var endDate = PageParameter( PageParameterKey.EndDate ).AsDateTime()?.Date;

            if ( endDate.HasValue )
            {
                endDate = endDate.Value.AddDays( 1 ).AddTicks( -1 );
            }

            var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateRange.DelimitedValues );

            if ( startDate.HasValue || dateRange.Start.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( se => se.AlertDateTime >= ( startDate ?? dateRange.Start.Value ) );
            }

            if ( endDate.HasValue || dateRange.End.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( se => se.AlertDateTime <= ( endDate ?? dateRange.End.Value ) );
            }

            // Filter by alert type ids
            var alertTypeId = PageParameter( PageParameterKey.AlertTypeId ).AsIntegerOrNull();
            var alertTypeIds = cblAlertTypes.SelectedValues.AsIntegerList();

            if ( alertTypeId.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( a => a.AlertTypeId == alertTypeId );
            }
            else if ( alertTypeIds.Any() )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( a => alertTypeIds.Contains( a.AlertTypeId ) );
            }

            // Filter by alert category
            var alertCategories = new List<AlertType>();
            foreach ( var alertCategory in cblAlertCategory.SelectedValues )
            {
                if ( !string.IsNullOrWhiteSpace( alertCategory ) )
                {
                    alertCategories.Add( alertCategory.ConvertToEnum<AlertType>() );
                }
            }

            if ( alertCategories.Any() )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( m => alertCategories.Contains( m.FinancialTransactionAlertType.AlertType ) );
            }

            // Filter by person's giving id
            var personId = GetPerson( rockContext );
            if ( !personId.HasValue && ppPerson.Visible )
            {
                personId = ppPerson.SelectedValue;
            }
            else if ( personId.HasValue && !ppPerson.Visible )
            {
                var personField = gAlertList.ColumnsOfType<RockBoundField>().Where( a => a.HeaderText == "Name" ).FirstOrDefault();
                if ( personField != null )
                {
                    personField.Visible = false;
                }
            }

            if ( personId.HasValue )
            {
                var personGivingId = new PersonService( rockContext ).GetSelect( personId.Value, s => s.GivingId );
                if ( personGivingId.IsNotNullOrWhiteSpace() )
                {
                    financialTransactionAlertQry = financialTransactionAlertQry.Where( a => a.GivingId == personGivingId );
                }
            }

            // Filter by transaction amount
            if ( nreTransactionAmount.LowerValue.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( a => a.Amount >= nreTransactionAmount.LowerValue.Value );
            }

            if ( nreTransactionAmount.UpperValue.HasValue )
            {
                financialTransactionAlertQry = financialTransactionAlertQry.Where( a => a.Amount <= nreTransactionAmount.UpperValue.Value );
            }

            // Filter by campus id
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
            else if ( CampusCache.All().Count == 1 )
            {
                // Hide the campus field if there is only one campus in the system
                var campusField = gAlertList.ColumnsOfType<RockBoundField>().Where( a => a.HeaderText == "Campus" ).FirstOrDefault();

                if ( campusField != null )
                {
                    campusField.Visible = false;
                }
            }

            gAlertList.EntityTypeId = EntityTypeCache.Get<FinancialTransactionAlert>().Id;
            var sortProperty = gAlertList.SortProperty;
            gAlertList.DataSource = sortProperty != null ? financialTransactionAlertQry.Sort( sortProperty ).ToList() : financialTransactionAlertQry.OrderByDescending( p => p.AlertDateTime ).ToList();
            gAlertList.DataBind();
        }

        #endregion Internal Methods
    }
}