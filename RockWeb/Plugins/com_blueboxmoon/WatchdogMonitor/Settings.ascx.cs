using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Settings" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "Configures system settings for the Watchdog Monitor plugin." )]
    public partial class Settings : RockBlock
    {
        #region Base Method Overrides

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
            if ( !IsPostBack )
            {
                ShowDetails();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            var rockContext = new RockContext();

            var systemCommunications = new SystemCommunicationService( rockContext ).Queryable()
                .Where( e => e.Category != null )
                .OrderBy( e => e.Category.Name )
                .ThenBy( e => e.Title );

            ddlStateChangeCommunication.Items.Clear();
            ddlStateCommunication.Items.Clear();
            ddlStateChangeCommunication.Items.Add( new ListItem() );
            ddlStateCommunication.Items.Add( new ListItem() );
            systemCommunications.ToList().ForEach( e =>
            {
                ddlStateChangeCommunication.Items.Add( new ListItem( string.Format( "{0} > {1}", e.Category.Name, e.Title ), e.Guid.ToString().ToLower() ) );
                ddlStateCommunication.Items.Add( new ListItem( string.Format( "{0} > {1}", e.Category.Name, e.Title ), e.Guid.ToString().ToLower() ) );
            } );
            ddlStateChangeCommunication.SetValue( Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.WatchdogMonitor.ServiceStateChangedEmail" ).ToLower() );
            ddlStateCommunication.SetValue( Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.WatchdogMonitor.ServiceStateEmail" ).ToLower() );

            dvSMSFromNumber.Items.Clear();
            dvSMSFromNumber.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() ).Id;
            dvSMSFromNumber.SetValue( Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.WatchdogMonitor.SMSFromDefinedValue" ).AsGuidOrNull() );

            ceStateChangeSMS.Text = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.WatchdogMonitor.ServiceStateChangedSMS" );
            ceStateSMS.Text = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.WatchdogMonitor.ServiceStateSMS" );

            tbCollectorSharedSecret.Text = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.WatchdogMonitor.CollectorSharedSecret" );
            tbHistoryConnectionString.Text = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.WatchdogMonitor.HistoryConnectionString" );

            lbSave.Visible = IsUserAuthorized( Rock.Security.Authorization.EDIT );
            ddlStateChangeCommunication.Enabled = lbSave.Visible;
            ddlStateCommunication.Enabled = lbSave.Visible;
            dvSMSFromNumber.Enabled = lbSave.Visible;
            ceStateChangeSMS.Enabled = lbSave.Visible;
            ceStateSMS.Enabled = lbSave.Visible;
            tbHistoryConnectionString.Enabled = lbSave.Visible;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.WatchdogMonitor.ServiceStateChangedEmail", ddlStateChangeCommunication.SelectedValue );
            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.WatchdogMonitor.ServiceStateEmail", ddlStateCommunication.SelectedValue );

            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.WatchdogMonitor.ServiceStateChangedSMS", ceStateChangeSMS.Text );
            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.WatchdogMonitor.ServiceStateSMS", ceStateSMS.Text );

            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.WatchdogMonitor.SMSFromDefinedValue", dvSMSFromNumber.SelectedValue );
            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.WatchdogMonitor.CollectorSharedSecret", tbCollectorSharedSecret.Text );
            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.WatchdogMonitor.HistoryConnectionString", tbHistoryConnectionString.Text );

            if ( !string.IsNullOrWhiteSpace( tbHistoryConnectionString.Text ) )
            {
                WatchdogServiceCheckHistory.InitializeTable();
            }

            nbSaved.Visible = true;
        }

        #endregion
    }
}