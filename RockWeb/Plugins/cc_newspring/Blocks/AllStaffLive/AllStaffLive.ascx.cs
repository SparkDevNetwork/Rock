////////////////////////////////
// Live Video All Staff Plugin
//
// Author
// -------
// Edolyne Long
// NewSpring Church
//
// Version 0.9
////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using DDay.iCal;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.AllStaffLive
{
    [DisplayName( "All Staff Live" )]
    [Category( "NewSpring" )]
    [Description( "Live video all staff display" )]
    [SchedulesField( "Live Schedule", "Choose a schedule for all staff to be live" )]
    public partial class AllStaffLive : Rock.Web.UI.RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Add Styles and scripts
            HtmlGenericContainer ooyalaStyles = new HtmlGenericContainer( "link" );
            ooyalaStyles.Attributes["rel"] = "stylesheet";
            ooyalaStyles.Attributes["href"] = "//s3.amazonaws.com/ns.assets/newspring/ooyalaPlayer.css";
            Page.Header.Controls.Add( ooyalaStyles );


            HtmlGenericContainer ooyalaCore = new HtmlGenericContainer( "script" );
            ooyalaCore.Attributes["type"] = "text/javascript";
            ooyalaCore.Attributes["src"] = "//player.ooyala.com/static/v4/stable/4.8.5/core.min.js";
            Page.Header.Controls.Add( ooyalaCore );

            HtmlGenericContainer ooyalaHtml = new HtmlGenericContainer( "script" );
            ooyalaHtml.Attributes["type"] = "text/javascript";
            ooyalaHtml.Attributes["src"] = "//player.ooyala.com/static/v4/stable/4.8.5/video-plugin/main_html5.min.js";
            Page.Header.Controls.Add( ooyalaHtml );

            HtmlGenericContainer ooyalaBitWrapper = new HtmlGenericContainer( "script" );
            ooyalaBitWrapper.Attributes["type"] = "text/javascript";
            ooyalaBitWrapper.Attributes["src"] = "//player.ooyala.com/static/v4/stable/4.8.5/video-plugin/bit_wrapper.min.js";
            Page.Header.Controls.Add( ooyalaBitWrapper );

            HtmlGenericContainer ooyalaFlash = new HtmlGenericContainer( "script" );
            ooyalaFlash.Attributes["type"] = "text/javascript";
            ooyalaFlash.Attributes["src"] = "//player.ooyala.com/static/v4/stable/4.8.5/video-plugin/osmf_flash.min.js";
            Page.Header.Controls.Add( ooyalaFlash );

            HtmlGenericContainer ooyalaSkin = new HtmlGenericContainer( "script" );
            ooyalaSkin.Attributes["type"] = "text/javascript";
            ooyalaSkin.Attributes["src"] = "//player.ooyala.com/static/v4/stable/4.8.5/skin-plugin/html5-skin.js";
            Page.Header.Controls.Add( ooyalaSkin );

            var scheduleGuids = GetAttributeValue( "LiveSchedule" );

            // Check to make sure that scheduleGuids is not null
            if ( scheduleGuids != null )
            {
                var scheduleArray = scheduleGuids.Split( ',' ).AsGuidList();

                var scheduleService = new ScheduleService( new RockContext() );

                // Support for multiples schedules loops through each
                foreach ( var scheduleGuid in scheduleArray )
                {
                    var schedule = new ScheduleService( new RockContext() ).Get( scheduleGuid );

                    if ( schedule != null )
                    {
                        var scheduleExpired = schedule.IsValid;

                        bool scheduleActive = schedule.IsScheduleOrCheckInActive;

                        // Check if Check in or Schedule is active and set the state accordingly
                        if ( scheduleActive )
                        {
                            // Active Schedule, set liveFeedStatus to on
                            liveFeedStatus.Value = "on";

                            liveHeading.Text = BlockName;

                            // Get the ip address
                            String currentIp = GetLanIPAddress();

                            String internalIp = "204.116.47.244";

                            if ( currentIp == internalIp )
                            {
                                localIP.Value = "true";
                            }
                            else
                            {
                                localIP.Value = "false";
                            }

                            break;
                        }
                        else
                        {
                            liveFeedStatus.Value = "off";
                        }
                    }
                    else
                    {
                        liveFeedStatus.Value = "off";
                    }
                }
            }
        }

        public String GetLanIPAddress()
        {
            //The X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating IP address of a
            //client connecting to a web server through an HTTP proxy or load balancer
            String ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if ( string.IsNullOrEmpty( ip ) )
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }


    }
}