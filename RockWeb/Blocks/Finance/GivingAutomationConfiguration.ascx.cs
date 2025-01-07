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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Utility.Enums;
using Rock.Utility.Settings.Giving;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// </summary>
    [DisplayName( "Giving Automation Configuration" )]
    [Category( "Finance" )]
    [Description( "Block used to view and create new alert types for the giving automation system." )]

    [Rock.SystemGuid.BlockTypeGuid( "A91ACA78-68FD-41FC-B652-17A37789EA32" )]
    public partial class GivingAutomationConfiguration : Rock.Web.UI.RockBlock
    {
        #region Constants

        /// <summary>
        /// The account type: all tax deductible
        /// </summary>
        private const string AccountTypes_AllTaxDeductible = "AllTaxDeductible";

        /// <summary>
        /// The account type: custom
        /// </summary>
        private const string AccountTypes_Custom = "Custom";

        #endregion Constants

        #region Properties

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>ConnectionOpportunity
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAlerts.DataKeyNames = new string[] { "Id" };
            gAlerts.Actions.ShowAdd = true;
            gAlerts.Actions.AddClick += gAlerts_AddClick;
            gAlerts.GridRebind += gAlerts_GridRebind;
            gAlerts.GridReorder += gAlerts_GridReorder;

            mdAlertDetails.SaveClick += mdAlertDetails_SaveClick;
            mdAlertDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfFinancialTransactionAlertTypeId.ClientID );

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
                ShowDetail();
            }
            else
            {
                BindAlerts();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblAccountTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblAccountTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            divAccounts.Visible = rblAccountTypes.SelectedValue == AccountTypes_Custom;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlConnectionType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedConnectionTypeId = ddlConnectionType.SelectedValueAsId();
            var connectionOpportunityService = new ConnectionOpportunityService( new RockContext() );
            ddlConnectionOpportunity.Items.Clear();
            ddlConnectionOpportunity.Visible = selectedConnectionTypeId.HasValue;
            if ( selectedConnectionTypeId.HasValue )
            {
                var connectionOpportunities = connectionOpportunityService.Queryable().Where( a => a.ConnectionTypeId == selectedConnectionTypeId.Value );
                ddlConnectionOpportunity.Items.Add( new ListItem() );
                ddlConnectionOpportunity.Items.AddRange( connectionOpportunities.Select( x => new ListItem { Text = x.Name, Value = x.Id.ToString() } ).ToArray() );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAlerts_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAlerts_Delete( object sender, RowEventArgs e )
        {
            DeleteFinancialTransactionAlertType( e.RowKeyId );
            BindAlerts();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gAlerts_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindAlerts();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAlerts_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var alertType = e.Row.DataItem as FinancialTransactionAlertType;

            if ( alertType == null )
            {
                return;
            }

            // Status icons
            var lStatusIcons = e.Row.FindControl( "lStatusIcons" ) as Literal;

            if ( lStatusIcons != null )
            {
                var cssClass = alertType.AlertType == AlertType.Gratitude ? "success" : "warning";
                lStatusIcons.Text = string.Format(
                    "<span class='badge badge-{1} badge-circle' data-toggle='tooltip' data-original-title='{0}' style='inline-block'><span class='sr-only'></span></span>",
                    alertType.AlertType.ToString(),
                    cssClass );
            }

            // Actions Taken
            var lActionsTaken = e.Row.FindControl( "lActionsTaken" ) as Literal;

            if ( lActionsTaken != null )
            {
                string actionHtml = "<div class='status-list'>";
                actionHtml += string.Format( "<i class='margin-r-sm fa fa-cog' style='opacity: {0};' aria-hidden='true'></i>", alertType.WorkflowTypeId.HasValue ? 1 : 0 );
                actionHtml += string.Format( "<i class='margin-r-sm fa fa-comment' style='opacity: {0};' aria-hidden='true'></i>", alertType.SystemCommunicationId.HasValue ? 1 : 0 );
                actionHtml += string.Format( "<i class='margin-r-sm fa fa-plug' style='opacity: {0};' aria-hidden='true'></i>", alertType.ConnectionOpportunityId.HasValue ? 1 : 0 );
                actionHtml += string.Format( "<i class='margin-r-sm fa fa-bus' style='opacity: {0};' aria-hidden='true'></i>", alertType.SendBusEvent ? 1 : 0 );
                actionHtml += string.Format( "<i class='margin-r-sm fa fa-building-o' style='opacity: {0};' aria-hidden='true'></i>", alertType.AccountParticipantSystemCommunicationId.HasValue ? 1 : 0 );
                actionHtml += "</div>";
                lActionsTaken.Text = actionHtml;
            }
        }

        /// <summary>
        /// Handles the Add event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAlerts_AddClick( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the GridReorder event of the gAlerts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gAlerts_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var financialTransactionAlertTypes = GetFinancialTransactionAlertTypes( rockContext );
            if ( financialTransactionAlertTypes != null )
            {
                new FinancialTransactionAlertTypeService( rockContext ).Reorder( financialTransactionAlertTypes.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindAlerts();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdAlertDetails_SaveClick( object sender, EventArgs e )
        {
            int financialTransactionAlertTypeId = hfFinancialTransactionAlertTypeId.Value.AsIntegerOrNull() ?? 0;

            var rockContext = new RockContext();
            var financialTransactionAlertTypeService = new FinancialTransactionAlertTypeService( rockContext );
            FinancialTransactionAlertType financialTransactionAlertType = null;

            if ( financialTransactionAlertTypeId != 0 )
            {
                financialTransactionAlertType = financialTransactionAlertTypeService.Get( financialTransactionAlertTypeId );
            }

            if ( financialTransactionAlertType == null )
            {
                financialTransactionAlertType = new FinancialTransactionAlertType();
                financialTransactionAlertTypeService.Add( financialTransactionAlertType );
            }

            financialTransactionAlertType.Name = tbName.Text;
            financialTransactionAlertType.CampusId = cpCampus.SelectedCampusId;
            financialTransactionAlertType.FinancialAccountId = apAlertAccount.SelectedValueAsId();
            financialTransactionAlertType.IncludeChildFinancialAccounts = cbAlertIncludeChildAccounts.Checked;
            financialTransactionAlertType.AlertType = rblAlertType.SelectedValueAsEnum<AlertType>();
            financialTransactionAlertType.ContinueIfMatched = cbContinueIfMatched.Checked;
            financialTransactionAlertType.RepeatPreventionDuration = nbRepeatPreventionDuration.Text.AsIntegerOrNull();
            financialTransactionAlertType.AmountSensitivityScale = nbAmountSensitivityScale.Text.AsDecimalOrNull();
            financialTransactionAlertType.FrequencySensitivityScale = nbFrequencySensitivityScale.Text.AsDecimalOrNull();
            financialTransactionAlertType.MinimumGiftAmount = cbMinimumGiftAmount.Value;
            financialTransactionAlertType.MaximumGiftAmount = cbMaximumGiftAmount.Value;
            financialTransactionAlertType.MinimumMedianGiftAmount = cbMinimumMedianGiftAmount.Value;
            financialTransactionAlertType.MaximumMedianGiftAmount = cbMaximumMedianGiftAmount.Value;
            financialTransactionAlertType.MaximumDaysSinceLastGift = nbMaxDaysSinceLastGift.IntegerValue;
            financialTransactionAlertType.DataViewId = dvpPersonDataView.SelectedValueAsInt();
            financialTransactionAlertType.SendBusEvent = cbSendBusEvent.Checked;
            financialTransactionAlertType.ConnectionOpportunityId = ddlConnectionOpportunity.SelectedValueAsId();
            financialTransactionAlertType.SystemCommunicationId = ddlDonorSystemCommunication.SelectedValueAsId();
            financialTransactionAlertType.AccountParticipantSystemCommunicationId = ddlAccountParticipantSystemCommunication.SelectedValueAsId();
            financialTransactionAlertType.WorkflowTypeId = wtpLaunchWorkflow.SelectedValueAsId();
            financialTransactionAlertType.AlertSummaryNotificationGroupId = gpNotificationGroup.GroupId;
            financialTransactionAlertType.RunDays = dwpDaysToRunAlertType.SelectedDaysOfWeekAsFlags();
            rockContext.SaveChanges();

            hfFinancialTransactionAlertTypeId.Value = string.Empty;
            mdAlertDetails.Hide();

            BindAlerts();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var selectedGivingAutomationAccountIds = apGivingAutomationAccounts.SelectedIds;
            var selectedGivingAutomationAccountGuids = new List<Guid>();

            if ( selectedGivingAutomationAccountIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var accountService = new FinancialAccountService( rockContext );
                    selectedGivingAutomationAccountGuids = accountService.Queryable()
                        .AsNoTracking()
                        .Where( a => selectedGivingAutomationAccountIds.Contains( a.Id ) )
                        .Select( a => a.Guid )
                        .ToList();
                }
            }

            var isCustomAccounts = rblAccountTypes.SelectedValue == AccountTypes_Custom;

            var givingAutomationSettings = GivingAutomationSettings.LoadGivingAutomationSettings();

            givingAutomationSettings.FinancialAccountGuids = isCustomAccounts ? selectedGivingAutomationAccountGuids : null;
            givingAutomationSettings.AreChildAccountsIncluded = isCustomAccounts ? cbGivingAutomationIncludeChildAccounts.Checked : ( bool? ) null;
            givingAutomationSettings.TransactionTypeGuids = cblTransactionTypes.SelectedValues.AsGuidList();

            // Main Giving Automation Settings
            givingAutomationSettings.GivingAutomationJobSettings.IsEnabled = cbEnableGivingAutomation.Checked;
            givingAutomationSettings.GivingClassificationSettings.RunDays = dwpDaysToUpdateClassifications.SelectedDaysOfWeek?.ToArray();

            // Giving Journey Settings
            givingAutomationSettings.GivingJourneySettings.DaysToUpdateGivingJourneys = dwpDaysToUpdateGivingJourneys.SelectedDaysOfWeek?.ToArray();

            givingAutomationSettings.GivingJourneySettings.FormerGiverNoContributionInTheLastDays = nbFormerGiverNoContributionInTheLastDays.IntegerValue;
            givingAutomationSettings.GivingJourneySettings.FormerGiverMedianFrequencyLessThanDays = nbFormerGiverMedianFrequencyLessThanDays.IntegerValue;

            givingAutomationSettings.GivingJourneySettings.LapsedGiverNoContributionInTheLastDays = nbLapsedGiverNoContributionInTheLastDays.IntegerValue;
            givingAutomationSettings.GivingJourneySettings.LapsedGiverMedianFrequencyLessThanDays = nbLapsedGiverMedianFrequencyLessThanDays.IntegerValue;

            givingAutomationSettings.GivingJourneySettings.NewGiverContributionCountBetweenMinimum = ( int? ) nreNewGiverContributionCountBetween.LowerValue;
            givingAutomationSettings.GivingJourneySettings.NewGiverContributionCountBetweenMaximum = ( int? ) nreNewGiverContributionCountBetween.UpperValue;
            givingAutomationSettings.GivingJourneySettings.NewGiverFirstGiftInTheLastDays = nbNewGiverFirstGiftInLastDays.IntegerValue;

            givingAutomationSettings.GivingJourneySettings.OccasionalGiverMedianFrequencyDaysMinimum = ( int? ) nreOccasionalGiverMedianFrequencyDays.LowerValue;
            givingAutomationSettings.GivingJourneySettings.OccasionalGiverMedianFrequencyDaysMaximum = ( int? ) nreOccasionalGiverMedianFrequencyDays.UpperValue;

            givingAutomationSettings.GivingJourneySettings.ConsistentGiverMedianLessThanDays = nbConsistentGiverMedianLessThanDays.IntegerValue;

            // Alerting Settings
            givingAutomationSettings.GivingAlertingSettings.GlobalRepeatPreventionDurationDays = nbGlobalRepeatPreventionDuration.Text.AsIntegerOrNull();
            givingAutomationSettings.GivingAlertingSettings.GratitudeRepeatPreventionDurationDays = nbGratitudeRepeatPreventionDuration.Text.AsIntegerOrNull();
            givingAutomationSettings.GivingAlertingSettings.FollowupRepeatPreventionDurationDays = nbFollowupRepeatPreventionDuration.Text.AsIntegerOrNull();

            GivingAutomationSettings.SaveGivingAutomationSettings( givingAutomationSettings );

            this.NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            var rockContext = new RockContext();
            var transactionTypes = BindTransactionTypes();
            BindAccounts();

            // Load values from the system settings
            var givingAutomationSetting = GivingAutomationSettings.LoadGivingAutomationSettings();

            var savedTransactionTypeGuids = givingAutomationSetting.TransactionTypeGuids ?? new List<Guid> { Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() };

            var savedTransactionTypeGuidStrings = savedTransactionTypeGuids.Select( g => g.ToString() )
                .Intersect( transactionTypes.Select( dv => dv.Guid.ToString() ) );

            var savedAccountGuids = givingAutomationSetting.FinancialAccountGuids ?? new List<Guid>();
            var accounts = new List<FinancialAccount>();
            var areChildAccountsIncluded = givingAutomationSetting.AreChildAccountsIncluded ?? false;

            if ( savedAccountGuids.Any() )
            {
                var accountService = new FinancialAccountService( rockContext );
                accounts = accountService.Queryable()
                    .AsNoTracking()
                    .Where( a => savedAccountGuids.Contains( a.Guid ) )
                    .ToList();
            }

            // Sync the system setting values to the controls
            divAccounts.Visible = savedAccountGuids.Any();
            apGivingAutomationAccounts.SetValues( accounts );
            rblAccountTypes.SetValue( savedAccountGuids.Any() ? AccountTypes_Custom : AccountTypes_AllTaxDeductible );
            cbGivingAutomationIncludeChildAccounts.Checked = areChildAccountsIncluded;
            cblTransactionTypes.SetValues( savedTransactionTypeGuidStrings );

            // Main Giving Automation Settings
            cbEnableGivingAutomation.Checked = givingAutomationSetting.GivingAutomationJobSettings.IsEnabled;
            dwpDaysToUpdateClassifications.SelectedDaysOfWeek = givingAutomationSetting.GivingClassificationSettings.RunDays?.ToList();

            // Giving Journey Settings
            dwpDaysToUpdateGivingJourneys.SelectedDaysOfWeek = givingAutomationSetting.GivingJourneySettings.DaysToUpdateGivingJourneys?.ToList();

            nbFormerGiverNoContributionInTheLastDays.IntegerValue = givingAutomationSetting.GivingJourneySettings.FormerGiverNoContributionInTheLastDays;
            nbFormerGiverMedianFrequencyLessThanDays.IntegerValue = givingAutomationSetting.GivingJourneySettings.FormerGiverMedianFrequencyLessThanDays;

            nbLapsedGiverNoContributionInTheLastDays.IntegerValue = givingAutomationSetting.GivingJourneySettings.LapsedGiverNoContributionInTheLastDays;
            nbLapsedGiverMedianFrequencyLessThanDays.IntegerValue = givingAutomationSetting.GivingJourneySettings.LapsedGiverMedianFrequencyLessThanDays;

            nreNewGiverContributionCountBetween.LowerValue = givingAutomationSetting.GivingJourneySettings.NewGiverContributionCountBetweenMinimum;
            nreNewGiverContributionCountBetween.UpperValue = givingAutomationSetting.GivingJourneySettings.NewGiverContributionCountBetweenMaximum;
            nbNewGiverFirstGiftInLastDays.IntegerValue = givingAutomationSetting.GivingJourneySettings.NewGiverFirstGiftInTheLastDays;

            nreOccasionalGiverMedianFrequencyDays.LowerValue = givingAutomationSetting.GivingJourneySettings.OccasionalGiverMedianFrequencyDaysMinimum;
            nreOccasionalGiverMedianFrequencyDays.UpperValue = givingAutomationSetting.GivingJourneySettings.OccasionalGiverMedianFrequencyDaysMaximum;

            nbConsistentGiverMedianLessThanDays.IntegerValue = givingAutomationSetting.GivingJourneySettings.ConsistentGiverMedianLessThanDays;

            nbGlobalRepeatPreventionDuration.Text = givingAutomationSetting.GivingAlertingSettings.GlobalRepeatPreventionDurationDays.ToStringSafe();
            nbGratitudeRepeatPreventionDuration.Text = givingAutomationSetting.GivingAlertingSettings.GratitudeRepeatPreventionDurationDays.ToStringSafe();
            nbFollowupRepeatPreventionDuration.Text = givingAutomationSetting.GivingAlertingSettings.FollowupRepeatPreventionDurationDays.ToStringSafe();

            BindAlerts();
        }

        /// <summary>
        /// Binds the accounts.
        /// </summary>
        private void BindAccounts()
        {
            if ( rblAccountTypes.Items.Count > 0 )
            {
                return;
            }

            rblAccountTypes.DataTextField = "Text";
            rblAccountTypes.DataValueField = "Value";
            rblAccountTypes.DataSource = new[] {
                new {
                    Text = "All Tax Deductible Accounts",
                    Value = AccountTypes_AllTaxDeductible
                },
                new {
                    Text ="Custom",
                    Value = AccountTypes_Custom
                }
            };
            rblAccountTypes.DataBind();
        }

        /// <summary>
        /// Binds the transaction types.
        /// </summary>
        private List<DefinedValueCache> BindTransactionTypes()
        {
            var transactionTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE )
                .DefinedValues
                .Where( dv => dv.IsActive )
                .ToList();

            if ( cblTransactionTypes.Items.Count > 0 )
            {
                return transactionTypes;
            }

            cblTransactionTypes.DataTextField = "Text";
            cblTransactionTypes.DataValueField = "Value";
            cblTransactionTypes.DataSource = transactionTypes.Select( dv => new
            {
                Text = dv.Value,
                Value = dv.Guid.ToString()
            } );

            cblTransactionTypes.DataBind();
            return transactionTypes;
        }

        /// <summary>
        /// Binds the alerts.
        /// </summary>
        private void BindAlerts()
        {
            if ( CampusCache.All().Count == 1 )
            {
                // Hide the campus field if there is only one campus in the system
                var campusField = gAlerts.ColumnsOfType<RockBoundField>().Where( a => a.HeaderText == "Campus" ).FirstOrDefault();

                if ( campusField != null )
                {
                    campusField.Visible = false;
                }
            }

            gAlerts.DataSource = GetFinancialTransactionAlertTypes().ToList();
            gAlerts.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="financialTransactionAlertTypeId">The financial transaction alert type id.</param>
        private void ShowEdit( int financialTransactionAlertTypeId )
        {
            FinancialTransactionAlertType financialTransactionAlertType = null;
            if ( financialTransactionAlertTypeId > 0 )
            {
                financialTransactionAlertType = new FinancialTransactionAlertTypeService( new RockContext() ).Get( financialTransactionAlertTypeId );
            }

            if ( financialTransactionAlertType == null )
            {
                financialTransactionAlertType = new FinancialTransactionAlertType();
            }

            BindControl();
            tbName.Text = financialTransactionAlertType.Name;
            hfFinancialTransactionAlertTypeId.Value = financialTransactionAlertType.Id.ToString();
            cpCampus.SetValue( financialTransactionAlertType.CampusId );
            apAlertAccount.SetValue( financialTransactionAlertType.FinancialAccountId );
            cbAlertIncludeChildAccounts.Checked = financialTransactionAlertType.IncludeChildFinancialAccounts;
            rblAlertType.SetValue( ( int ) financialTransactionAlertType.AlertType );

            UpdateSensitivityDescriptions( financialTransactionAlertType.AlertType );

            cbContinueIfMatched.Checked = financialTransactionAlertType.ContinueIfMatched;
            nbRepeatPreventionDuration.Text = financialTransactionAlertType.RepeatPreventionDuration.ToStringSafe();
            nbAmountSensitivityScale.Text = financialTransactionAlertType.AmountSensitivityScale.ToStringSafe();
            nbFrequencySensitivityScale.Text = financialTransactionAlertType.FrequencySensitivityScale.ToStringSafe();
            cbMinimumGiftAmount.Value = financialTransactionAlertType.MinimumGiftAmount;
            cbMaximumGiftAmount.Value = financialTransactionAlertType.MaximumGiftAmount;
            cbMinimumMedianGiftAmount.Value = financialTransactionAlertType.MinimumMedianGiftAmount;
            cbMaximumMedianGiftAmount.Value = financialTransactionAlertType.MaximumMedianGiftAmount;
            nbMaxDaysSinceLastGift.IntegerValue = financialTransactionAlertType.MaximumDaysSinceLastGift;
            dvpPersonDataView.SetValue( financialTransactionAlertType.DataViewId );
            cbSendBusEvent.Checked = financialTransactionAlertType.SendBusEvent;
            wtpLaunchWorkflow.SetValue( financialTransactionAlertType.WorkflowTypeId );
            gpNotificationGroup.GroupId = financialTransactionAlertType.AlertSummaryNotificationGroupId;
            dwpDaysToRunAlertType.SelectedDaysOfWeek = ( financialTransactionAlertType.RunDays ?? DayOfWeekFlag.All ).AsDayOfWeekList();

            if ( financialTransactionAlertType.ConnectionOpportunity != null )
            {
                ddlConnectionType.SetValue( financialTransactionAlertType.ConnectionOpportunity.ConnectionTypeId );
                ddlConnectionType_SelectedIndexChanged( null, null );
                ddlConnectionOpportunity.SetValue( financialTransactionAlertType.ConnectionOpportunityId );
            }
            else
            {
                int? connectionTypeId = null;
                ddlConnectionType.SetValue( connectionTypeId );
                ddlConnectionType_SelectedIndexChanged( null, null );
            }

            ddlDonorSystemCommunication.SetValue( financialTransactionAlertType.SystemCommunicationId );
            ddlAccountParticipantSystemCommunication.SetValue( financialTransactionAlertType.AccountParticipantSystemCommunicationId );
            mdAlertDetails.Show();
        }

        /// <summary>
        /// Binds the data source to the selection control.
        /// </summary>
        private void BindControl()
        {
            rblAlertType.BindToEnum<AlertType>();

            var rockContext = new RockContext();
            var connectionTypeService = new ConnectionTypeService( rockContext );
            ddlConnectionType.Items.Clear();
            ddlConnectionType.Items.Add( new ListItem() );
            ddlConnectionType.Items.AddRange( connectionTypeService.Queryable().Select( x => new ListItem { Text = x.Name, Value = x.Id.ToString() } ).ToArray() );

            dvpPersonDataView.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

            var systemCommunications = new SystemCommunicationService( rockContext ).Queryable().OrderBy( e => e.Title );
            ddlDonorSystemCommunication.Items.Clear();
            ddlDonorSystemCommunication.Items.Add( new ListItem() );
            ddlAccountParticipantSystemCommunication.Items.Clear();
            ddlAccountParticipantSystemCommunication.Items.Add( new ListItem() );
            if ( systemCommunications.Any() )
            {
                ddlDonorSystemCommunication.Items.AddRange( systemCommunications.Select( x => new ListItem { Text = x.Title, Value = x.Id.ToString() } ).ToArray() );
                ddlAccountParticipantSystemCommunication.Items.AddRange( systemCommunications.Select( x => new ListItem { Text = x.Title, Value = x.Id.ToString() } ).ToArray() );
            }
        }

        /// <summary>
        /// Deletes the financial transaction alert type.
        /// </summary>
        /// <param name="financialTransactionAlertTypeId">The financial transaction alert type id.</param>
        private void DeleteFinancialTransactionAlertType( int financialTransactionAlertTypeId )
        {
            using ( var rockContext = new RockContext() )
            {
                var financialTransactionAlertTypeService = new FinancialTransactionAlertTypeService( rockContext );
                var financialTransactionAlertType = financialTransactionAlertTypeService.Get( financialTransactionAlertTypeId );
                if ( financialTransactionAlertType != null )
                {
                    string errorMessage;
                    if ( !financialTransactionAlertTypeService.CanDelete( financialTransactionAlertType, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        financialTransactionAlertTypeService.Delete( financialTransactionAlertType );
                        rockContext.SaveChanges();
                    } );
                }
            }
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="text">The text.</param>
        private void ShowError( string text )
        {
            nbEditModeMessage.Title = "Oops";
            nbEditModeMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbEditModeMessage.Text = text;
            nbEditModeMessage.Visible = true;
        }

        /// <summary>
        /// Get the Financial Transaction Alert Types.
        /// </summary>
        private IQueryable<FinancialTransactionAlertType> GetFinancialTransactionAlertTypes( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var financialTransactionAlertTypeService = new FinancialTransactionAlertTypeService( rockContext );
            return financialTransactionAlertTypeService.Queryable().OrderBy( s => s.Order ).ThenBy( s => s.Name );
        }

        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblAlertType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblAlertType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateSensitivityDescriptions( rblAlertType.SelectedValueAsEnum<AlertType>() );
        }

        /// <summary>
        /// Updates the sensitivity description.
        /// </summary>
        /// <param name="alertType">Type of the alert.</param>
        private void UpdateSensitivityDescriptions( AlertType alertType )
        {
            /* 11-19-2021 MDP

            AlertType drives the logic of how sensitivity values are used. (See notes and logic here https://github.com/SparkDevNetwork/Rock/blob/6dacabe84dcaf041450c3bc075164c7580151390/Rock/Jobs/GivingAutomation.cs#L1602)

            Follow-Up uses sensitivity to look for 'worse than usual':
                - Gifts with amounts that are significantly smaller than usual. For example, a $50 gift for somebody that usually gives $300 a week.
                - Gifts that are significantly late. For example: a Weekly giver than hasn't given for several weeks

            Gratitude uses sensitivity to look for 'better than usual':
                - Gifts with amounts that are significantly larger than usual. For example, a $1200 gift for somebody that usually gives $300 a week.
                - Gifts that are significantly early. For example: a monthly giver that gave 20 days earlier than usual.

            In both cases, a positive value for sensitivity should be used.
            A negative sensitivity could be entered if they really wanted to, but it'll do weird things such as express gratitude for a gift over $180 for a person that normally gives $200.

            */

            lFrequencySensitivityScaleHelp.Text = FinancialTransactionAlertType.GetFrequencySensitivityDescription( alertType );
            lAmountSensitivityScaleHelp.Text = FinancialTransactionAlertType.GetAmountSensitivityDescription( alertType );
        }
    }
}