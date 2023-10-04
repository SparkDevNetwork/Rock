using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

using SystemGuid = com.shepherdchurch.DigitalSignage.SystemGuid;

namespace RockWeb.Plugins.com_shepherdchurch.DigitalSignage
{
    [DisplayName( "Digital Sign Rotator" )]
    [Category( "Shepherd Church > Digital Signage" )]
    [Description( "Displays a full-screen interface for displaying images and videos in a rotator." )]

    [IntegerField( "Slide Interval", "How long each slide should remain on screen before the next transition happens. Default is 8 seconds. Must be at least 4 seconds. If a value is set on the Content Channel then that value will be used instead.", false, 0, order: 0 )]
    [IntegerField( "Update Interval", "How often should the slide rotator check for updates to the content channel. Default is 60 seconds. Must be at least 10 seconds.", false, 0, order: 1 )]
    [CustomCheckboxListField( "Transitions", "Which transitions should be used. If none are selected then all available transitions will be used. If a value is set on the Content Channel then that value will be used instead.", "bars^Bars,blinds^Blinds,blocks^Blocks,blocks2^Blocks 2,dissolve^Dissolve,slide^Slide,zip^Zip,bars3d^Bars 3D,blinds3d^Blinds 3D,cube^Cube 3D,tiles3d^Tiles 3D,turn^Turn 3D", false, order: 2 )]
    [ContentChannelField( "Content Channel Override", "By default the configured schedules on the device will determine what content channel to display. You can override this behavior to always show this content channel no matter what.", false, order: 3 )]
    [BooleanField( "Enable Device Match By Name", "Enable a match by computer name by doing reverse IP lookup to get computer name based on IP address", true, "", 4, "EnableReverseLookup" )]
    public partial class DigitalSignRotator : RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Plugins/com_shepherdchurch/DigitalSignage/Scripts/flux.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_shepherdchurch/DigitalSignage/Scripts/digitalsignrotator.js" );
            RockPage.AddCSSLink( "~/Plugins/com_shepherdchurch/DigitalSignage/Styles/digitalsignrotator.css" );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            pnlContent.Visible = false;
            pnlError.Visible = false;

            if ( !IsPostBack )
            {
                ShowContent();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Show and prepare the content panel, or show the error panel.
        /// </summary>
        protected void ShowContent()
        {
            var rockContext = new RockContext();
            var deviceService = new DeviceService( rockContext );
            var digitalDisplayId = DefinedValueCache.Get( SystemGuid.DefinedValue.DEVICE_TYPE_DIGITAL_DISPLAY.AsGuid() ).Id;
            var ipAddress = Request.UserHostAddress;
            bool enableReverseLookup = GetAttributeValue( "EnableReverseLookup" ).AsBoolean( true );
            Device device = null;
            ContentChannel contentChannel = null;

            if ( !string.IsNullOrWhiteSpace( PageParameter( "deviceId" ) ) )
            {
                device = deviceService.Get( PageParameter( "deviceId" ).AsInteger() );
            }
            else
            {
                device = deviceService.Queryable()
                    .Where( d => d.DeviceTypeValueId == digitalDisplayId && d.IPAddress == ipAddress )
                    .FirstOrDefault();

                if ( device == null && enableReverseLookup )
                {
                    try
                    {
                        var hostName = System.Net.Dns.GetHostEntry( ipAddress ).HostName;
                        device = deviceService.Queryable()
                            .Where( d => d.DeviceTypeValueId == digitalDisplayId && d.IPAddress == hostName )
                            .FirstOrDefault();
                    }
                    catch
                    {
                        /* Intentionally ignored. */
                    }
                }
            }

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "ContentChannelOverride" ) ) )
            {
                contentChannel = new ContentChannelService( rockContext ).Get( GetAttributeValue( "ContentChannelOverride" ).AsGuid() );
            }

            //
            // If we have a valid device then show the content panel. Othrwise show an error.
            //
            if ( device != null || contentChannel != null )
            {
                if ( device != null )
                {
                    hfDevice.Value = device.Id.ToString();
                }

                if ( GetAttributeValue( "SlideInterval" ).AsInteger() >= 4 )
                {
                    hfSlideInterval.Value = ( GetAttributeValue( "SlideInterval" ).AsInteger() * 1000 ).ToString();
                }

                if ( GetAttributeValue( "UpdateInterval" ).AsInteger() >= 10 )
                {
                    hfUpdateInterval.Value = ( GetAttributeValue( "UpdateInterval" ).AsInteger() * 1000 ).ToString();
                }

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "Transitions" ) ) )
                {
                    hfTransitions.Value = GetAttributeValue( "Transitions" );
                }

                if ( contentChannel != null )
                {
                    hfContentChannel.Value = contentChannel.Id.ToString();
                }

                hfAudio.Value = PageParameter( "Audio" ).AsBoolean( true ).ToString().ToLower();

                pnlError.Visible = false;
                pnlContent.Visible = true;
            }
            else
            {
                pnlContent.Visible = false;
                pnlError.Visible = true;

                nbError.Text = string.Format( "This kiosk has not been configured in the system.<br />IP address: {0}",
                    ipAddress );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbReload_Click( object sender, EventArgs e )
        {
            ShowContent();
        }

        #endregion
    }
}