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
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility.Settings.DataAutomation;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Data Automation Settings - Block used to set values specific to data automation (updating person status, family campus, etc.).
    /// </summary>
    [DisplayName( "Data Automation Settings" )]
    [Category( "Administration" )]
    [Description( "Block used to set values specific to data automation (updating person status, family campus, etc.)." )]
    [Rock.SystemGuid.BlockTypeGuid( "E34C45E9-97CA-4902-803B-1EFAC9174083" )]
    public partial class DataAutomationSettings : RockBlock
    {
        #region private variables

        private List<IgnoreCampusChangeRow> _ignoreCampusChangeRows { get; set; }

        private RockContext _rockContext = new RockContext();

        private Dictionary<string, string> _generalSettings = new Dictionary<string, string>();

        private ReactivatePeople _reactivateSettings = new ReactivatePeople();

        private InactivatePeople _inactivateSettings = new InactivatePeople();

        private UpdateFamilyCampus _campusSettings = new UpdateFamilyCampus();

        private MoveAdultChildren _adultChildrenSettings = new MoveAdultChildren();

        private UpdatePersonConnectionStatus _updatePersonConnectionStatus = new UpdatePersonConnectionStatus();

        private UpdateFamilyStatus _updateFamilyStatus = new UpdateFamilyStatus();

        private List<InteractionItem> _interactionChannelTypes = new List<InteractionItem>();

        private List<CampusCache> _campuses = new List<CampusCache>();

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            _campuses = CampusCache.All();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindControls();
                GetSettings();
                SetPanels();
            }
            else
            {
                GetIgnoreCampusChangesRepeaterData();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the SystemConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles saving all the data set by the user to the web.config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void bbtnSaveConfig_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            SaveSettings();
        }

        /// <summary>
        /// Handles the Click event of the lbAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAdd_Click( object sender, EventArgs e )
        {
            int newId = _ignoreCampusChangeRows.Max( a => a.Id ) + 1;
            _ignoreCampusChangeRows.Add( new IgnoreCampusChangeRow { Id = newId } );

            rptIgnoreCampusChanges.DataSource = _ignoreCampusChangeRows;
            rptIgnoreCampusChanges.DataBind();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptIgnoreCampusChanges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptIgnoreCampusChanges_ItemCommand( object sender, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "delete" )
            {
                int rowId = e.CommandArgument.ToString().AsInteger();
                var repeaterRow = _ignoreCampusChangeRows.SingleOrDefault( a => a.Id == rowId );
                if ( repeaterRow != null )
                {
                    _ignoreCampusChangeRows.Remove( repeaterRow );
                }

                rptIgnoreCampusChanges.DataSource = _ignoreCampusChangeRows;
                rptIgnoreCampusChanges.DataBind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptIgnoreCampusChanges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptIgnoreCampusChanges_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var ignoreCampusChangeRow = e.Item.DataItem as IgnoreCampusChangeRow;
            CampusPicker fromCampus = e.Item.FindControl( "cpFromCampus" ) as CampusPicker;
            CampusPicker toCampus = e.Item.FindControl( "cpToCampus" ) as CampusPicker;
            RockDropDownList ddlCampusAttendanceOrGiving = e.Item.FindControl( "ddlAttendanceOrGiving" ) as RockDropDownList;

            if ( ignoreCampusChangeRow != null && fromCampus != null && toCampus != null && ddlCampusAttendanceOrGiving != null )
            {
                fromCampus.Campuses = _campuses;
                fromCampus.SelectedCampusId = ignoreCampusChangeRow.FromCampusId;

                toCampus.Campuses = _campuses;
                toCampus.SelectedCampusId = ignoreCampusChangeRow.ToCampusId;

                if ( ignoreCampusChangeRow.CampusCriteria.HasValue )
                {
                    ddlCampusAttendanceOrGiving.SetValue( ignoreCampusChangeRow.CampusCriteria.ConvertToInt() );
                }
                else
                {
                    ddlCampusAttendanceOrGiving.SetValue( string.Empty );
                }
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event when enabling/disabling a data automation option.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void cbDataAutomationEnabled_CheckedChanged( object sender, EventArgs e )
        {
            SetPanels();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the controls.
        /// </summary>
        private void BindControls()
        {
            var groupTypes = new GroupTypeService( _rockContext )
                .Queryable().AsNoTracking()
                .Where( t => t.TakesAttendance == true )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name )
                .Select( t => new { value = t.Id, text = t.Name } )
                .ToList();

            rlbAttendanceInGroupType.DataSource = groupTypes;
            rlbAttendanceInGroupType.DataBind();

            rlbNoAttendanceInGroupType.DataSource = groupTypes;
            rlbNoAttendanceInGroupType.DataBind();

            var personAttributes = new AttributeService( _rockContext )
                .GetByEntityTypeId( new Person().TypeId, false )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name )
                .Select( t => new { value = t.Id, text = t.Name } )
                .ToList();

            rlbPersonAttributes.DataSource = personAttributes;
            rlbPersonAttributes.DataBind();

            rlbNoPersonAttributes.DataSource = personAttributes;
            rlbNoPersonAttributes.DataBind();

            ddlAttendanceOrGiving.BindToEnum<CampusCriteria>();

            var knownRelGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            if ( knownRelGroupType != null )
            {
                rpParentRelationship.GroupTypeId = knownRelGroupType.Id;
                rpSiblingRelationship.GroupTypeId = knownRelGroupType.Id;
            }
        }

        /// <summary>
        /// Enables the data automation panels and sets the titles.
        /// </summary>
        private void SetPanels()
        {
            if ( CampusCache.All().Count == 1 )
            {
                pwUpdateCampus.Visible = false;
            }
            else
            {
                SetPanel( pwUpdateCampus, pnlCampusUpdate, "Update Family Campus", cbCampusUpdate.Checked );
            }

            SetPanel( pwReactivatePeople, pnlReactivatePeople, "Reactivate People", cbReactivatePeople.Checked );
            SetPanel( pwInactivatePeople, pnlInactivatePeople, "Inactivate People", cbInactivatePeople.Checked );
            SetPanel( pwAdultChildren, pnlAdultChildren, "Move Adult Children", cbAdultChildren.Checked );
            SetPanel( pwUpdatePersonConnectionStatus, pnlUpdatePersonConnectionStatus, "Update Connection Status", cbUpdatePersonConnectionStatus.Checked );
            SetPanel( pwUpdateFamilyStatus, pnlUpdateFamilyStatus, "Update Family Status", cbUpdateFamilyStatus.Checked );
        }

        /// <summary>
        /// Enables a data automation panel and sets the title.
        /// </summary>
        /// <param name="panelWidget">The panel widget.</param>
        /// <param name="panel">The panel.</param>
        /// <param name="title">The title.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        private void SetPanel( PanelWidget panelWidget, Panel panel, string title, bool enabled )
        {
            panel.Enabled = enabled;
            var enabledLabel = string.Empty;
            if ( enabled )
            {
                enabledLabel = "<span class='label label-success'>Enabled</span>";
            }
            else
            {
                enabledLabel = "<span class='label label-warning'>Disabled</span>";
            }

            panelWidget.Title = string.Format( "<h3 class='panel-title pull-left margin-r-sm'>{0}</h3> <div class='pull-right'>{1}</div>", title, enabledLabel );
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        private void GetSettings()
        {
            // Get General Settings
            nbGenderAutoFill.Text = Rock.Web.SystemSettings.GetValue( SystemSetting.GENDER_AUTO_FILL_CONFIDENCE );

            // Get Data Automation Settings
            _reactivateSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_REACTIVATE_PEOPLE ).FromJsonOrNull<ReactivatePeople>() ?? new ReactivatePeople();
            _inactivateSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_INACTIVATE_PEOPLE ).FromJsonOrNull<InactivatePeople>() ?? new InactivatePeople();
            _campusSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_CAMPUS_UPDATE ).FromJsonOrNull<UpdateFamilyCampus>() ?? new UpdateFamilyCampus();
            _adultChildrenSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_ADULT_CHILDREN ).FromJsonOrNull<MoveAdultChildren>() ?? new MoveAdultChildren();
            _updatePersonConnectionStatus = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_UPDATE_PERSON_CONNECTION_STATUS ).FromJsonOrNull<UpdatePersonConnectionStatus>() ?? new UpdatePersonConnectionStatus();
            _updateFamilyStatus = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_UPDATE_FAMILY_STATUS ).FromJsonOrNull<UpdateFamilyStatus>() ?? new UpdateFamilyStatus();

            // Set Data Automation Controls

            // Reactivate
            cbReactivatePeople.Checked = _reactivateSettings.IsEnabled;
            pnlReactivatePeople.Enabled = _reactivateSettings.IsEnabled;
            cbLastContribution.Checked = _reactivateSettings.IsLastContributionEnabled;
            nbLastContribution.Text = _reactivateSettings.LastContributionPeriod.ToStringSafe();
            cbAttendanceInServiceGroup.Checked = _reactivateSettings.IsAttendanceInServiceGroupEnabled;
            nbAttendanceInServiceGroup.Text = _reactivateSettings.AttendanceInServiceGroupPeriod.ToStringSafe();
            cbRegisteredInAnyEvent.Checked = _reactivateSettings.IsRegisteredInAnyEventEnabled;
            nbRegisteredInAnyEvent.Text = _reactivateSettings.RegisteredInAnyEventPeriod.ToStringSafe();
            cbAttendanceInGroupType.Checked = _reactivateSettings.IsAttendanceInGroupTypeEnabled;
            nbAttendanceInGroupType.Text = _reactivateSettings.AttendanceInGroupTypeDays.ToStringSafe();
            rlbAttendanceInGroupType.SetValues( _reactivateSettings.AttendanceInGroupType ?? new List<int>() );
            cbSiteLogin.Checked = _reactivateSettings.IsSiteLoginEnabled;
            nbSiteLogin.Text = _reactivateSettings.SiteLoginPeriod.ToStringSafe();
            cbPrayerRequest.Checked = _reactivateSettings.IsPrayerRequestEnabled;
            nbPrayerRequest.Text = _reactivateSettings.PrayerRequestPeriod.ToStringSafe();
            cbPersonAttributes.Checked = _reactivateSettings.IsPersonAttributesEnabled;
            nbPersonAttributes.Text = _reactivateSettings.PersonAttributesDays.ToStringSafe();
            rlbPersonAttributes.SetValues( _reactivateSettings.PersonAttributes ?? new List<int>() );
            cbIncludeDataView.Checked = _reactivateSettings.IsIncludeDataViewEnabled;
            dvIncludeDataView.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;
            dvIncludeDataView.SetValue( _reactivateSettings.IncludeDataView );
            cbExcludeDataView.Checked = _reactivateSettings.IsExcludeDataViewEnabled;
            dvExcludeDataView.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;
            dvExcludeDataView.SetValue( _reactivateSettings.ExcludeDataView );
            cbInteractions.Checked = _reactivateSettings.IsInteractionsEnabled;

            var interactionChannels = new InteractionChannelService( _rockContext )
                .Queryable().AsNoTracking()
                .Select( a => new { a.Guid, a.Name } )
                .ToList();

            var reactivateChannelTypes = interactionChannels.Select( c => new InteractionItem( c.Guid, c.Name ) ).ToList();
            if ( _reactivateSettings.Interactions != null )
            {
                bool noneSelected = !_reactivateSettings.Interactions.Any( i => i.IsInteractionTypeEnabled );
                foreach ( var settingInteractionItem in _reactivateSettings.Interactions )
                {
                    var interactionChannelType = reactivateChannelTypes.SingleOrDefault( a => a.Guid == settingInteractionItem.Guid );
                    if ( interactionChannelType != null )
                    {
                        interactionChannelType.IsInteractionTypeEnabled = noneSelected || settingInteractionItem.IsInteractionTypeEnabled;
                        interactionChannelType.LastInteractionDays = settingInteractionItem.LastInteractionDays;
                    }
                }

                // Now UNCHECK all channels that were NOT *previously* saved
                var remainingChannels = reactivateChannelTypes.Where( c => !_reactivateSettings.Interactions.Any( x => x.Guid == c.Guid ) );
                foreach ( var nonSavedInteractionItem in remainingChannels )
                {
                    nonSavedInteractionItem.IsInteractionTypeEnabled = false;
                }
            }

            rInteractions.DataSource = reactivateChannelTypes;
            rInteractions.DataBind();

            // Inactivate
            cbInactivatePeople.Checked = _inactivateSettings.IsEnabled;
            pnlInactivatePeople.Enabled = _inactivateSettings.IsEnabled;
            cbNoLastContribution.Checked = _inactivateSettings.IsNoLastContributionEnabled;
            nbNoLastContribution.Text = _inactivateSettings.NoLastContributionPeriod.ToStringSafe();
            cbNoAttendanceInGroupType.Checked = _inactivateSettings.IsNoAttendanceInGroupTypeEnabled;
            nbNoAttendanceInGroupType.Text = _inactivateSettings.NoAttendanceInGroupTypeDays.ToStringSafe();
            rlbNoAttendanceInGroupType.SetValues( _inactivateSettings.AttendanceInGroupType ?? new List<int>() );
            cbNoRegistrationInAnyEvent.Checked = _inactivateSettings.IsNotRegisteredInAnyEventEnabled;
            nbNoRegistrationInAnyEvent.Text = _inactivateSettings.NotRegisteredInAnyEventDays.ToStringSafe();
            cbNoSiteLogin.Checked = _inactivateSettings.IsNoSiteLoginEnabled;
            nbNoSiteLogin.Text = _inactivateSettings.NoSiteLoginPeriod.ToStringSafe();
            cbNoPrayerRequest.Checked = _inactivateSettings.IsNoPrayerRequestEnabled;
            nbNoPrayerRequest.Text = _inactivateSettings.NoPrayerRequestPeriod.ToStringSafe();
            cbNoPersonAttributes.Checked = _inactivateSettings.IsNoPersonAttributesEnabled;
            nbNoPersonAttributes.Text = _inactivateSettings.NoPersonAttributesDays.ToStringSafe();
            rlbNoPersonAttributes.SetValues( _inactivateSettings.PersonAttributes ?? new List<int>() );
            cbNotInDataView.Checked = _inactivateSettings.IsNotInDataviewEnabled;
            dvNotInDataView.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;
            dvNotInDataView.SetValue( _inactivateSettings.NotInDataview );
            cbNoInteractions.Checked = _inactivateSettings.IsNoInteractionsEnabled;
            nbRecordsOlderThan.Text = _inactivateSettings.RecordsOlderThan.ToStringSafe();

            var inactivateChannelTypes = interactionChannels.Select( c => new InteractionItem( c.Guid, c.Name ) ).ToList();
            if ( _inactivateSettings.NoInteractions != null )
            {
                bool noneSelected = !_inactivateSettings.NoInteractions.Any( i => i.IsInteractionTypeEnabled );
                foreach ( var settingInteractionItem in _inactivateSettings.NoInteractions )
                {
                    var interactionChannelType = inactivateChannelTypes.SingleOrDefault( a => a.Guid == settingInteractionItem.Guid );
                    if ( interactionChannelType != null )
                    {
                        interactionChannelType.IsInteractionTypeEnabled = noneSelected || settingInteractionItem.IsInteractionTypeEnabled;
                        interactionChannelType.LastInteractionDays = settingInteractionItem.LastInteractionDays;
                    }
                }

                // Now UNCHECK all channels that were NOT *previously* saved
                var remainingChannels = inactivateChannelTypes.Where( c => !_inactivateSettings.NoInteractions.Any( x => x.Guid == c.Guid ) );
                foreach ( var nonSavedInteractionItem in remainingChannels )
                {
                    nonSavedInteractionItem.IsInteractionTypeEnabled = false;
                }
            }

            rNoInteractions.DataSource = inactivateChannelTypes;
            rNoInteractions.DataBind();

            // campus Update
            cbCampusUpdate.Checked = _campusSettings.IsEnabled;
            pnlCampusUpdate.Enabled = _campusSettings.IsEnabled;
            cbMostFamilyAttendance.Checked = _campusSettings.IsMostFamilyAttendanceEnabled;
            nbMostFamilyAttendance.Text = _campusSettings.MostFamilyAttendancePeriod.ToStringSafe();
            nbTimesToTriggerCampusChange.Text = _campusSettings.TimesToTriggerCampusChange.ToStringSafe();
            cbMostFamilyGiving.Checked = _campusSettings.IsMostFamilyGivingEnabled;
            nbMostFamilyGiving.Text = _campusSettings.MostFamilyGivingPeriod.ToStringSafe();
            ddlAttendanceOrGiving.SetValue( _campusSettings.MostAttendanceOrGiving.ConvertToInt() );
            cbIgnoreIfManualUpdate.Checked = _campusSettings.IsIgnoreIfManualUpdateEnabled;
            nbIgnoreIfManualUpdate.Text = _campusSettings.IgnoreIfManualUpdatePeriod.ToStringSafe();

            cbIgnoreCampusChanges.Checked = _campusSettings.IsIgnoreCampusChangesEnabled;
            if ( _campusSettings.IgnoreCampusChanges != null && _campusSettings.IgnoreCampusChanges.Any() )
            {
                int i = 1;
                _ignoreCampusChangeRows = _campusSettings.IgnoreCampusChanges
                    .Select( a => new IgnoreCampusChangeRow()
                    {
                        Id = i++,
                        CampusCriteria = a.BasedOn,
                        FromCampusId = a.FromCampus,
                        ToCampusId = a.ToCampus
                    } ).ToList();
            }
            else
            {
                _ignoreCampusChangeRows = new List<IgnoreCampusChangeRow>() { new IgnoreCampusChangeRow() { Id = 1 } };
            }

            rptIgnoreCampusChanges.DataSource = _ignoreCampusChangeRows;
            rptIgnoreCampusChanges.DataBind();

            if ( _campusSettings.ExcludeSchedules != null && _campusSettings.ExcludeSchedules.Count > 0 )
            {
                var excludedSchedules = new ScheduleService( _rockContext ).Queryable().Where( s => _campusSettings.ExcludeSchedules.Contains( s.Id ) ).ToList();
                spExcludeSchedules.SetValues( excludedSchedules );
            }

            // Adult Children
            cbAdultChildren.Checked = _adultChildrenSettings.IsEnabled;
            cbisMoveGraduated.Checked = _adultChildrenSettings.IsOnlyMoveGraduated;
            pnlAdultChildren.Enabled = _adultChildrenSettings.IsEnabled;
            nbAdultAge.Text = _adultChildrenSettings.AdultAge.ToString();
            rpParentRelationship.GroupRoleId = _adultChildrenSettings.ParentRelationshipId;
            rpSiblingRelationship.GroupRoleId = _adultChildrenSettings.SiblingRelationshipId;
            cbSameAddress.Checked = _adultChildrenSettings.UseSameHomeAddress;
            cbSamePhone.Checked = _adultChildrenSettings.UseSameHomePhone;
            wfWorkflows.SetValues( _adultChildrenSettings.WorkflowTypeIds );
            nbMaxRecords.Text = _adultChildrenSettings.MaximumRecords.ToString();

            // Update Connection Status
            cbUpdatePersonConnectionStatus.Checked = _updatePersonConnectionStatus.IsEnabled;
            pnlUpdatePersonConnectionStatus.Enabled = _updatePersonConnectionStatus.IsEnabled;
            var personConnectionStatusDataViewSettingsList = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).DefinedValues
                .Select( a => new PersonConnectionStatusDataView
                {
                    PersonConnectionStatusValue = a,
                    DataViewId = _updatePersonConnectionStatus.ConnectionStatusValueIdDataviewIdMapping.GetValueOrNull( a.Id )
                } ).ToList();

            rptPersonConnectionStatusDataView.DataSource = personConnectionStatusDataViewSettingsList;
            rptPersonConnectionStatusDataView.DataBind();

            // Update Family Status
            cbUpdateFamilyStatus.Checked = _updateFamilyStatus.IsEnabled;
            pnlUpdateFamilyStatus.Enabled = _updateFamilyStatus.IsEnabled;
            var groupStatusDataViewSettingsList = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FAMILY_GROUP_STATUS.AsGuid() ).DefinedValues
                .Select( a => new FamilyStatusDataView
                {
                    GroupStatusValue = a,
                    DataViewId = _updateFamilyStatus.GroupStatusValueIdDataviewIdMapping.GetValueOrNull( a.Id )
                } ).ToList();

            rptFamilyStatusDataView.DataSource = groupStatusDataViewSettingsList;
            rptFamilyStatusDataView.DataBind();
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        private void SaveSettings()
        {
            // Save General
            Rock.Web.SystemSettings.SetValue( SystemSetting.GENDER_AUTO_FILL_CONFIDENCE, nbGenderAutoFill.Text );

            // Save Data Automation
            _reactivateSettings = new ReactivatePeople();
            _inactivateSettings = new InactivatePeople();
            _campusSettings = new UpdateFamilyCampus();

            // Reactivate 
            _reactivateSettings.IsEnabled = cbReactivatePeople.Checked;

            _reactivateSettings.IsLastContributionEnabled = cbLastContribution.Checked;
            _reactivateSettings.LastContributionPeriod = nbLastContribution.Text.AsInteger();

            _reactivateSettings.IsAttendanceInServiceGroupEnabled = cbAttendanceInServiceGroup.Checked;
            _reactivateSettings.AttendanceInServiceGroupPeriod = nbAttendanceInServiceGroup.Text.AsInteger();

            _reactivateSettings.IsRegisteredInAnyEventEnabled = cbRegisteredInAnyEvent.Checked;
            _reactivateSettings.RegisteredInAnyEventPeriod = nbRegisteredInAnyEvent.Text.AsInteger();

            _reactivateSettings.IsAttendanceInGroupTypeEnabled = cbAttendanceInGroupType.Checked;
            _reactivateSettings.AttendanceInGroupType = rlbAttendanceInGroupType.SelectedValues.AsIntegerList();
            _reactivateSettings.AttendanceInGroupTypeDays = nbAttendanceInGroupType.Text.AsInteger();

            _reactivateSettings.IsSiteLoginEnabled = cbSiteLogin.Checked;
            _reactivateSettings.SiteLoginPeriod = nbSiteLogin.Text.AsInteger();

            _reactivateSettings.IsPrayerRequestEnabled = cbPrayerRequest.Checked;
            _reactivateSettings.PrayerRequestPeriod = nbPrayerRequest.Text.AsInteger();

            _reactivateSettings.IsPersonAttributesEnabled = cbPersonAttributes.Checked;
            _reactivateSettings.PersonAttributes = rlbPersonAttributes.SelectedValues.AsIntegerList();
            _reactivateSettings.PersonAttributesDays = nbPersonAttributes.Text.AsInteger();

            _reactivateSettings.IsIncludeDataViewEnabled = cbIncludeDataView.Checked;
            _reactivateSettings.IncludeDataView = dvIncludeDataView.SelectedValueAsInt();

            _reactivateSettings.IsExcludeDataViewEnabled = cbExcludeDataView.Checked;
            _reactivateSettings.ExcludeDataView = dvExcludeDataView.SelectedValueAsInt();

            _reactivateSettings.IsInteractionsEnabled = cbInteractions.Checked;

            foreach ( RepeaterItem rItem in rInteractions.Items )
            {
                RockCheckBox isInterationTypeEnabled = rItem.FindControl( "cbInterationType" ) as RockCheckBox;
                _reactivateSettings.Interactions = _reactivateSettings.Interactions ?? new List<InteractionItem>();
                HiddenField interactionTypeId = rItem.FindControl( "hfInteractionTypeId" ) as HiddenField;
                NumberBox lastInteractionDays = rItem.FindControl( "nbInteractionDays" ) as NumberBox;
                var item = new InteractionItem( interactionTypeId.Value.AsGuid(), string.Empty )
                {
                    IsInteractionTypeEnabled = isInterationTypeEnabled.Checked,
                    LastInteractionDays = lastInteractionDays.Text.AsInteger()
                };
                _reactivateSettings.Interactions.Add( item );
            }

            // Inactivate
            _inactivateSettings.IsEnabled = cbInactivatePeople.Checked;

            _inactivateSettings.IsNoLastContributionEnabled = cbNoLastContribution.Checked;
            _inactivateSettings.NoLastContributionPeriod = nbNoLastContribution.Text.AsInteger();

            _inactivateSettings.IsNoAttendanceInGroupTypeEnabled = cbNoAttendanceInGroupType.Checked;
            _inactivateSettings.AttendanceInGroupType = rlbNoAttendanceInGroupType.SelectedValues.AsIntegerList();
            _inactivateSettings.NoAttendanceInGroupTypeDays = nbNoAttendanceInGroupType.Text.AsInteger();

            _inactivateSettings.IsNotRegisteredInAnyEventEnabled = cbNoRegistrationInAnyEvent.Checked;
            _inactivateSettings.NotRegisteredInAnyEventDays = nbNoRegistrationInAnyEvent.Text.AsInteger();

            _inactivateSettings.IsNoSiteLoginEnabled = cbNoSiteLogin.Checked;
            _inactivateSettings.NoSiteLoginPeriod = nbNoSiteLogin.Text.AsInteger();

            _inactivateSettings.IsNoPrayerRequestEnabled = cbNoPrayerRequest.Checked;
            _inactivateSettings.NoPrayerRequestPeriod = nbNoPrayerRequest.Text.AsInteger();

            _inactivateSettings.IsNoPersonAttributesEnabled = cbNoPersonAttributes.Checked;
            _inactivateSettings.PersonAttributes = rlbNoPersonAttributes.SelectedValues.AsIntegerList();
            _inactivateSettings.NoPersonAttributesDays = nbNoPersonAttributes.Text.AsInteger();

            _inactivateSettings.IsNotInDataviewEnabled = cbNotInDataView.Checked;
            _inactivateSettings.NotInDataview = dvNotInDataView.SelectedValueAsInt();

            _inactivateSettings.IsNoInteractionsEnabled = cbNoInteractions.Checked;

            _inactivateSettings.RecordsOlderThan = nbRecordsOlderThan.Text.AsInteger();

            foreach ( RepeaterItem rItem in rNoInteractions.Items )
            {
                RockCheckBox isInterationTypeEnabled = rItem.FindControl( "cbInterationType" ) as RockCheckBox;
                _inactivateSettings.NoInteractions = _inactivateSettings.NoInteractions ?? new List<InteractionItem>();
                HiddenField interactionTypeId = rItem.FindControl( "hfInteractionTypeId" ) as HiddenField;
                NumberBox lastInteractionDays = rItem.FindControl( "nbNoInteractionDays" ) as NumberBox;
                var item = new InteractionItem( interactionTypeId.Value.AsGuid(), string.Empty )
                {
                    IsInteractionTypeEnabled = isInterationTypeEnabled.Checked,
                    LastInteractionDays = lastInteractionDays.Text.AsInteger()
                };

                _inactivateSettings.NoInteractions.Add( item );

            }

            // Campus Update
            _campusSettings.IsEnabled = cbCampusUpdate.Checked;

            _campusSettings.IsMostFamilyAttendanceEnabled = cbMostFamilyAttendance.Checked;
            _campusSettings.MostFamilyAttendancePeriod = nbMostFamilyAttendance.Text.AsInteger();
            _campusSettings.TimesToTriggerCampusChange = nbTimesToTriggerCampusChange.Text.AsInteger();

            _campusSettings.IsMostFamilyGivingEnabled = cbMostFamilyGiving.Checked;
            _campusSettings.MostFamilyGivingPeriod = nbMostFamilyGiving.Text.AsInteger();

            _campusSettings.MostAttendanceOrGiving = ddlAttendanceOrGiving.SelectedValueAsEnum<CampusCriteria>();

            _campusSettings.IsIgnoreIfManualUpdateEnabled = cbIgnoreIfManualUpdate.Checked;
            _campusSettings.IgnoreIfManualUpdatePeriod = nbIgnoreIfManualUpdate.Text.AsInteger();

            _campusSettings.IsIgnoreCampusChangesEnabled = cbIgnoreCampusChanges.Checked;
            _campusSettings.IgnoreCampusChanges =
                _ignoreCampusChangeRows
                    .Where( a => a.FromCampusId.HasValue && a.ToCampusId.HasValue )
                    .Select( a => new IgnoreCampusChangeItem
                    {
                        FromCampus = a.FromCampusId.Value,
                        ToCampus = a.ToCampusId.Value,
                        BasedOn = a.CampusCriteria
                    } )
                    .ToList();

            _campusSettings.ExcludeSchedules = spExcludeSchedules.SelectedIds.ToList();

            // Adult Children
            _adultChildrenSettings.IsEnabled = cbAdultChildren.Checked;
            _adultChildrenSettings.IsOnlyMoveGraduated = cbisMoveGraduated.Checked;
            _adultChildrenSettings.AdultAge = nbAdultAge.Text.AsIntegerOrNull() ?? 18;
            _adultChildrenSettings.ParentRelationshipId = rpParentRelationship.GroupRoleId;
            _adultChildrenSettings.SiblingRelationshipId = rpSiblingRelationship.GroupRoleId;
            _adultChildrenSettings.UseSameHomeAddress = cbSameAddress.Checked;
            _adultChildrenSettings.UseSameHomePhone = cbSamePhone.Checked;
            _adultChildrenSettings.WorkflowTypeIds = wfWorkflows.SelectedValuesAsInt().ToList();
            _adultChildrenSettings.MaximumRecords = nbMaxRecords.Text.AsIntegerOrNull() ?? 200;

            // Update Connection Status
            _updatePersonConnectionStatus.IsEnabled = cbUpdatePersonConnectionStatus.Checked;
            _updatePersonConnectionStatus.ConnectionStatusValueIdDataviewIdMapping.Clear();
            foreach ( var item in rptPersonConnectionStatusDataView.Items.OfType<RepeaterItem>() )
            {
                HiddenField hfPersonConnectionStatusValueId = item.FindControl( "hfPersonConnectionStatusValueId" ) as HiddenField;
                DataViewItemPicker dvpPersonConnectionStatusDataView = item.FindControl( "dvpPersonConnectionStatusDataView" ) as DataViewItemPicker;
                _updatePersonConnectionStatus.ConnectionStatusValueIdDataviewIdMapping.AddOrReplace( hfPersonConnectionStatusValueId.Value.AsInteger(), dvpPersonConnectionStatusDataView.SelectedValueAsId() );
            }

            // Update Family Status
            _updateFamilyStatus.IsEnabled = cbUpdateFamilyStatus.Checked;
            _updateFamilyStatus.GroupStatusValueIdDataviewIdMapping.Clear();
            foreach ( var item in rptFamilyStatusDataView.Items.OfType<RepeaterItem>() )
            {
                HiddenField hfGroupStatusValueId = item.FindControl( "hfGroupStatusValueId" ) as HiddenField;
                DataViewItemPicker dvpGroupStatusDataView = item.FindControl( "dvpGroupStatusDataView" ) as DataViewItemPicker;
                _updateFamilyStatus.GroupStatusValueIdDataviewIdMapping.AddOrReplace( hfGroupStatusValueId.Value.AsInteger(), dvpGroupStatusDataView.SelectedValueAsId() );
            }

            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_REACTIVATE_PEOPLE, _reactivateSettings.ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_INACTIVATE_PEOPLE, _inactivateSettings.ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_CAMPUS_UPDATE, _campusSettings.ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_ADULT_CHILDREN, _adultChildrenSettings.ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_UPDATE_PERSON_CONNECTION_STATUS, _updatePersonConnectionStatus.ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_UPDATE_FAMILY_STATUS, _updateFamilyStatus.ToJson() );
        }

        /// <summary>
        /// Gets the ignore campus changes repeater data.
        /// </summary>
        private void GetIgnoreCampusChangesRepeaterData()
        {
            _ignoreCampusChangeRows = new List<IgnoreCampusChangeRow>();

            foreach ( RepeaterItem item in rptIgnoreCampusChanges.Items )
            {
                CampusPicker fromCampus = item.FindControl( "cpFromCampus" ) as CampusPicker;
                CampusPicker toCampus = item.FindControl( "cpToCampus" ) as CampusPicker;
                HiddenField hiddenField = item.FindControl( "hfRowId" ) as HiddenField;
                RockDropDownList ddlCampusCriteria = item.FindControl( "ddlAttendanceOrGiving" ) as RockDropDownList;

                _ignoreCampusChangeRows.Add( new IgnoreCampusChangeRow
                {
                    Id = hiddenField.ValueAsInt(),
                    ToCampusId = toCampus.SelectedCampusId,
                    FromCampusId = fromCampus.SelectedCampusId,
                    CampusCriteria = ddlCampusCriteria.SelectedValueAsEnumOrNull<CampusCriteria>()
                } );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class IgnoreCampusChangeRow
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets from campus identifier.
            /// </summary>
            /// <value>
            /// From campus identifier.
            /// </value>
            public int? FromCampusId { get; set; }

            /// <summary>
            /// Gets or sets to campus identifier.
            /// </summary>
            /// <value>
            /// To campus identifier.
            /// </value>
            public int? ToCampusId { get; set; }

            /// <summary>
            /// Gets or sets the campus criteria.
            /// </summary>
            /// <value>
            /// The campus criteria.
            /// </value>
            public CampusCriteria? CampusCriteria { get; set; }
        }

        #endregion

        #region Update Connection Status

        /// <summary>
        /// Handles the ItemDataBound event of the rptPersonConnectionStatusDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPersonConnectionStatusDataView_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            PersonConnectionStatusDataView personConnectionStatusDataView = e.Item.DataItem as PersonConnectionStatusDataView;
            HiddenField hfPersonConnectionStatusValueId = e.Item.FindControl( "hfPersonConnectionStatusValueId" ) as HiddenField;
            DataViewItemPicker dvpPersonConnectionStatusDataView = e.Item.FindControl( "dvpPersonConnectionStatusDataView" ) as DataViewItemPicker;
            if ( personConnectionStatusDataView != null )
            {
                hfPersonConnectionStatusValueId.Value = personConnectionStatusDataView.PersonConnectionStatusValue.Id.ToString();
                dvpPersonConnectionStatusDataView.EntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();
                dvpPersonConnectionStatusDataView.Label = personConnectionStatusDataView.PersonConnectionStatusValue.ToString();
                dvpPersonConnectionStatusDataView.SetValue( personConnectionStatusDataView.DataViewId );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class PersonConnectionStatusDataView
        {
            /// <summary>
            /// Gets or sets the connection status value.
            /// </summary>
            /// <value>
            /// The connection status value.
            /// </value>
            public DefinedValueCache PersonConnectionStatusValue { get; set; }

            /// <summary>
            /// Gets or sets the data view identifier.
            /// </summary>
            /// <value>
            /// The data view identifier.
            /// </value>
            public int? DataViewId { get; set; }
        }

        #endregion

        #region Update Family Status

        /// <summary>
        /// Handles the ItemDataBound event of the rptFamilyStatusDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptFamilyStatusDataView_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            FamilyStatusDataView groupStatusDataView = e.Item.DataItem as FamilyStatusDataView;
            HiddenField hfGroupStatusValueId = e.Item.FindControl( "hfGroupStatusValueId" ) as HiddenField;
            DataViewItemPicker dvpGroupStatusDataView = e.Item.FindControl( "dvpGroupStatusDataView" ) as DataViewItemPicker;
            if ( groupStatusDataView != null )
            {
                hfGroupStatusValueId.Value = groupStatusDataView.GroupStatusValue.Id.ToString();
                dvpGroupStatusDataView.EntityTypeId = EntityTypeCache.GetId<Rock.Model.Group>();
                dvpGroupStatusDataView.Label = groupStatusDataView.GroupStatusValue.ToString();
                dvpGroupStatusDataView.SetValue( groupStatusDataView.DataViewId );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class FamilyStatusDataView
        {
            /// <summary>
            /// Gets or sets the group status value.
            /// </summary>
            /// <value>
            /// The group status value.
            /// </value>
            public DefinedValueCache GroupStatusValue { get; set; }

            /// <summary>
            /// Gets or sets the data view identifier.
            /// </summary>
            /// <value>
            /// The data view identifier.
            /// </value>
            public int? DataViewId { get; set; }
        }

        #endregion
    }
}