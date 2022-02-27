using System;
using System.Collections.Generic;
using System.ComponentModel;

using com.blueboxmoon.WatchdogMonitor.Checks;
using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Service Check Detail" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "View the details of a single service check." )]

    public partial class ServiceCheckDetail : RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/moment.min.js" );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/hammer.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/chartjs-plugin-zoom.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/pluralize.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/Graphs.js" );

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

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="T:Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            var serviceCheck = new WatchdogServiceCheckService( new RockContext() ).Get( PageParameter( pageReference, "ServiceCheckId" ).AsInteger() );
            if ( serviceCheck != null )
            {
                breadCrumbs.Add( new BreadCrumb( serviceCheck.ServiceCheckType.Name, pageReference ) );
            }

            return breadCrumbs;
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            var serviceCheckId = PageParameter( "ServiceCheckId" ).AsInteger();
            var serviceCheck = new WatchdogServiceCheckService( new RockContext() ).Get( serviceCheckId );

            if ( serviceCheck == null )
            {
                pnlDetails.Visible = false;
                return;
            }

            if ( !serviceCheck.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbWarningMessage.Text = Rock.Constants.EditModeMessage.NotAuthorizedToView( typeof( WatchdogServiceCheck ).GetFriendlyTypeName() );
                pnlDetails.Visible = false;

                return;
            }

            nbWarningMessage.Text = string.Empty;
            pnlDetails.Visible = true;

            lDetailsName.Text = serviceCheck.ServiceCheckType.Name;
            lDevice.Text = serviceCheck.Device.Name;
            lLastCheck.Text = serviceCheck.LastCheckDateTime.HasValue ? serviceCheck.LastCheckDateTime.Value.ToShortDateTimeString() : string.Empty;
            lValue.Text = serviceCheck.FormattedValue;
            lSummary.Text = serviceCheck.Summary;
            lState.Text = GetLabelForState( serviceCheck.State );
            lLastStateChange.Text = serviceCheck.LastStateChangeDateTime.HasValue ? serviceCheck.LastStateChangeDateTime.Value.ToShortDateTimeString() : string.Empty;
            hlSilenced.Visible = serviceCheck.IsSilenced;

            lbToggleSilence.Text = serviceCheck.IsSilenced ? "Unsilence" : "Silence";
        }

        /// <summary>
        /// Gets the label HTML for the service check state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        protected string GetLabelForState( object state )
        {
            if ( state is ServiceCheckState )
            {
                return WatchdogServiceCheck.GetLabelForState( ( ServiceCheckState ) state );
            }

            return string.Empty;
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
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            var serviceCheckId = PageParameter( "ServiceCheckId" ).AsInteger();
            var serviceCheck = new WatchdogServiceCheckService( new RockContext() ).Get( serviceCheckId );

            NavigateToParentPage( new System.Collections.Generic.Dictionary<string, string>
            {
                { "DeviceId", serviceCheck.DeviceId.ToString() }
            } );
        }

        /// <summary>
        /// Handles the Click event of the lbToggleSilence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbToggleSilence_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var serviceCheckId = PageParameter( "ServiceCheckId" ).AsInteger();
            var serviceCheck = new WatchdogServiceCheckService( rockContext ).Get( serviceCheckId );

            serviceCheck.IsSilenced = !serviceCheck.IsSilenced;

            rockContext.SaveChanges();

            ShowDetails();
        }

        #endregion
    }
}