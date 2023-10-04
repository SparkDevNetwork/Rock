
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.SimpleDonation.Utils.Settings;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_simpledonation.Communication
{
    /// <summary>
    /// Data Automation Settings
    /// </summary>
    [DisplayName( "Give Nurture Settings" )]
    [Category( "Simple Donation" )]
    [Description( "Block used to set settings for Simple Donation's Give Nurture project." )]
    public partial class GiveNurtureSettings : RockBlock
    {
        #region private variables

        private RockContext _rockContext = new RockContext();

        private FirstGift _firstGiftSettings = new FirstGift();
        private SetupRecurring _setupRecurringSettings = new SetupRecurring();
        private RescueLapsedGivers _rescueLapsedGivers = new RescueLapsedGivers();
        private NonGiversInGroup _nonGiversInGroup = new NonGiversInGroup();
        private NewishGiversNotInGroup _newishGiversNotInGroup = new NewishGiversNotInGroup();
        private YearEndAssetGiving _yearEndAssetGiving = new YearEndAssetGiving();

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
                GetSettings();
                SetPanels();
            }
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
        /// Handles the CheckedChanged event of the cbGiveNurtureEnabled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbGiveNurtureEnabled_CheckedChanged( object sender, EventArgs e )
        {
            SetPanels();
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

         #endregion

        #region Methods

        /// <summary>
        /// Enables the data automation panels and sets the titles.
        /// </summary>
        private void SetPanels()
        {
            SetPanel( pwFirstGift, pnlFirstGift, "First Gift", cbFirstGift.Checked );
            SetPanel( pwSetupRecurring, pnlSetupRecurring, "Setup Recurring Giving", cbSetupRecurring.Checked );
            SetPanel( pwRescueLapsedGivers, pnlRescueLapsedGivers, "Rescue Lapsed Givers", cbRescueLapsedGivers.Checked );
            SetPanel( pwNonGiversInGroup, pnlNonGiversInGroup, "Ask non-givers who are in a group to give", cbNonGiversInGroup.Checked );
            SetPanel( pwNewishGiversNotInGroup, pnlNewishGiversNotInGroup, "Ask new-ish givers, not in a group, to find a group", cbNewishGiversNotInGroup.Checked );
            SetPanel( pwYearEndAssetGiving, pnlYearEndAssetGiving, "Year-end, Asset-giving info. Goes out Nov 10 to ages 55+ who give $3,000+ per year", cbYearEndAssetGiving.Checked );
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

            panelWidget.Title = string.Format( "<h3 class='panel-title pull-left margin-r-sm margin-t-xs'>{0}</h3> <div class='pull-right'>{1}</div>", title, enabledLabel );
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        private void GetSettings()
        {
            // First Gift
            _firstGiftSettings = Rock.Web.SystemSettings.GetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_FIRST_GIFT ).FromJsonOrNull<FirstGift>() ?? new FirstGift();
            cbFirstGift.Checked = _firstGiftSettings.IsEnabled;
            nbFirstGiftMaxRecipients.Text = _firstGiftSettings.MaxRecipients.ToString();
            tbFirstGiftSubject.Text = _firstGiftSettings.EmailSubject;
            ceFirstGiftBody.Text = _firstGiftSettings.EmailBody;

            // Setup Recurring
            _setupRecurringSettings = Rock.Web.SystemSettings.GetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_SETUP_RECURRING ).FromJsonOrNull<SetupRecurring>() ?? new SetupRecurring();
            cbSetupRecurring.Checked = _setupRecurringSettings.IsEnabled;
            nbSetupRecurringMaxRecipients.Text = _setupRecurringSettings.MaxRecipients.ToString();
            tbSetupRecurringSubject.Text = _setupRecurringSettings.EmailSubject;
            ceSetupRecurringBody.Text = _setupRecurringSettings.EmailBody;

            // Rescue Lapsed Givers
            _rescueLapsedGivers = Rock.Web.SystemSettings.GetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_RESCUE_LAPSED_GIVERS ).FromJsonOrNull<RescueLapsedGivers>() ?? new RescueLapsedGivers();
            cbRescueLapsedGivers.Checked = _rescueLapsedGivers.IsEnabled;
            nbRescueLapsedGiversMaxRecipients.Text = _rescueLapsedGivers.MaxRecipients.ToString();
            tbRescueLapsedGiversSubject.Text = _rescueLapsedGivers.EmailSubject;
            ceRescueLapsedGiversBody.Text = _rescueLapsedGivers.EmailBody;

            // Non Givers In Group
            _nonGiversInGroup = Rock.Web.SystemSettings.GetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_NON_GIVERS_IN_GROUP ).FromJsonOrNull<NonGiversInGroup>() ?? new NonGiversInGroup();
            cbNonGiversInGroup.Checked = _nonGiversInGroup.IsEnabled;
            nbNonGiversInGroupMaxRecipients.Text = _nonGiversInGroup.MaxRecipients.ToString();
            tbNonGiversInGroupSubject.Text = _nonGiversInGroup.EmailSubject;
            ceNonGiversInGroupBody.Text = _nonGiversInGroup.EmailBody;

            // Newish Givers Not In Group
            _newishGiversNotInGroup = Rock.Web.SystemSettings.GetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_NEWISH_GIVERS_NOT_IN_GROUP ).FromJsonOrNull<NewishGiversNotInGroup>() ?? new NewishGiversNotInGroup();
            cbNewishGiversNotInGroup.Checked = _newishGiversNotInGroup.IsEnabled;
            nbNewishGiversNotInGroupMaxRecipients.Text = _newishGiversNotInGroup.MaxRecipients.ToString();
            tbNewishGiversNotInGroupSubject.Text = _newishGiversNotInGroup.EmailSubject;
            ceNewishGiversNotInGroupBody.Text = _newishGiversNotInGroup.EmailBody;

            // Year End Asset Giving
            _yearEndAssetGiving = Rock.Web.SystemSettings.GetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_YEAR_END_ASSET_GIVING ).FromJsonOrNull<YearEndAssetGiving>() ?? new YearEndAssetGiving();
            cbYearEndAssetGiving.Checked = _yearEndAssetGiving.IsEnabled;
            nbYearEdndAssetGivingMaxRecipients.Text = _yearEndAssetGiving.MaxRecipients.ToString();
            tbYearEndAssetGivingSubject.Text = _yearEndAssetGiving.EmailSubject;
            ceYearEndAssetGivingBody.Text = _yearEndAssetGiving.EmailBody;
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        private void SaveSettings()
        {
            // First Gift
            _firstGiftSettings = new FirstGift();
            _firstGiftSettings.IsEnabled = cbFirstGift.Checked;
            _firstGiftSettings.MaxRecipients = nbFirstGiftMaxRecipients.Text.AsInteger();
            _firstGiftSettings.EmailSubject = tbFirstGiftSubject.Text;
            _firstGiftSettings.EmailBody= ceFirstGiftBody.Text;
            Rock.Web.SystemSettings.SetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_FIRST_GIFT, _firstGiftSettings.ToJson() );

            // Setup Recurring
            _setupRecurringSettings = new SetupRecurring();
            _setupRecurringSettings.IsEnabled = cbSetupRecurring.Checked;
            _setupRecurringSettings.MaxRecipients = nbSetupRecurringMaxRecipients.Text.AsInteger();
            _setupRecurringSettings.EmailSubject = tbSetupRecurringSubject.Text;
            _setupRecurringSettings.EmailBody = ceSetupRecurringBody.Text;
            Rock.Web.SystemSettings.SetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_SETUP_RECURRING, _setupRecurringSettings.ToJson() );

            // Rescue Lapsed Givers
            _rescueLapsedGivers = new RescueLapsedGivers();
            _rescueLapsedGivers.IsEnabled = cbRescueLapsedGivers.Checked;
            _rescueLapsedGivers.MaxRecipients = nbRescueLapsedGiversMaxRecipients.Text.AsInteger();
            _rescueLapsedGivers.EmailSubject = tbRescueLapsedGiversSubject.Text;
            _rescueLapsedGivers.EmailBody = ceRescueLapsedGiversBody.Text;
            Rock.Web.SystemSettings.SetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_RESCUE_LAPSED_GIVERS, _rescueLapsedGivers.ToJson() );

            // Non Givers In Group
            _nonGiversInGroup = new NonGiversInGroup();
            _nonGiversInGroup.IsEnabled = cbNonGiversInGroup.Checked;
            _nonGiversInGroup.MaxRecipients = nbNonGiversInGroupMaxRecipients.Text.AsInteger();
            _rescueLapsedGivers.EmailSubject = tbNonGiversInGroupSubject.Text;
            _nonGiversInGroup.EmailBody = ceNonGiversInGroupBody.Text;
            Rock.Web.SystemSettings.SetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_NON_GIVERS_IN_GROUP, _nonGiversInGroup.ToJson() );

            // Newish Givers Not In Group
            _newishGiversNotInGroup = new NewishGiversNotInGroup();
            _newishGiversNotInGroup.IsEnabled = cbNewishGiversNotInGroup.Checked;
            _newishGiversNotInGroup.MaxRecipients = nbNewishGiversNotInGroupMaxRecipients.Text.AsInteger();
            _newishGiversNotInGroup.EmailSubject = tbNewishGiversNotInGroupSubject.Text;
            _newishGiversNotInGroup.EmailBody = ceNewishGiversNotInGroupBody.Text;
            Rock.Web.SystemSettings.SetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_NEWISH_GIVERS_NOT_IN_GROUP, _newishGiversNotInGroup.ToJson() );

            // Year End Asset Giving
            _yearEndAssetGiving = new YearEndAssetGiving();
            _yearEndAssetGiving.IsEnabled = cbYearEndAssetGiving.Checked;
            _yearEndAssetGiving.MaxRecipients = nbYearEdndAssetGivingMaxRecipients.Text.AsInteger();
            _yearEndAssetGiving.EmailSubject = tbYearEndAssetGivingSubject.Text;
            _yearEndAssetGiving.EmailBody = ceYearEndAssetGivingBody.Text;
            Rock.Web.SystemSettings.SetValue( com.simpledonation.SystemKey.SystemSetting.GIVE_NURTURE_YEAR_END_ASSET_GIVING, _yearEndAssetGiving.ToJson() );
        }

        #endregion

    }
}