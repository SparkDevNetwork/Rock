////////////////////////////////
// Live Video All Staff Plugin
//
// Author
// -------
// Brian Kalwat
// NewSpring Church
//
// Version 2.0
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

namespace RockWeb.Plugins.cc_newspring.Blocks.AllStaffLive {

    [DisplayName( "All Staff Live" )]
    [Category( "NewSpring" )]
    [Description( "Live video all staff display" )]
    [SchedulesField( "Live Schedule", "Choose a schedule for all staff to be live" )]
    public partial class AllStaffLive : Rock.Web.UI.RockBlock {

        protected override void OnLoad( EventArgs e ) {

            base.OnLoad( e );

            var scheduleGuids = GetAttributeValue( "LiveSchedule" );

            // Check to make sure that scheduleGuids is not null
            if ( scheduleGuids != null ) {

                var scheduleArray = scheduleGuids.Split( ',' ).AsGuidList();

                var scheduleService = new ScheduleService( new RockContext() );

                // Support for multiples schedules loops through each
                foreach ( var scheduleGuid in scheduleArray ) {

                    var schedule = new ScheduleService( new RockContext() ).Get( scheduleGuid );

                    if ( schedule != null ) {

                        var scheduleExpired = schedule.IsValid;

                        bool scheduleActive = schedule.IsScheduleOrCheckInActive;

                        // Check if Check in or Schedule is active and set the state accordingly
                        if ( scheduleActive ) {

                            // Active Schedule, set liveFeedStatus to on
                            liveFeedStatus.Value = "on";

                            // Get the ip address
                            String currentIp = GetLanIPAddress();

                            String internalIp = "204.116.47.244";

                            if ( currentIp == internalIp ) {

                                localIP.Value = "true";

                            } else {

                                localIP.Value = "false";

                            }

                            break;

                        } else {

                            liveFeedStatus.Value = "off";

                        }

                    } else {

                        liveFeedStatus.Value = "off";

                    }
                }
            }
        }

        public String GetLanIPAddress() {

            //The X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating IP address of a
            //client connecting to a web server through an HTTP proxy or load balancer
            String ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if ( string.IsNullOrEmpty( ip ) ) {

                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            }

            return ip;

        }

    }
}