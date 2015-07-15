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
    [TextField( "Ooyala Content ID", "Paste the Ooyala Content ID For All Staff Live Here" )]
    [SchedulesField( "Live Schedule", "Choose a schedule for all staff to be live" )]
    public partial class AllStaffLive : Rock.Web.UI.RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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
                        // Get the friendly schedule text
                        scheduleText.Value = schedule.FriendlyScheduleText.ToString();

                        var scheduleExpired = schedule.IsValid;

                        bool scheduleActive = schedule.IsScheduleOrCheckInActive;

                        // Check if Check in or Schedule is active and set the state accordingly
                        if ( scheduleActive )
                        {
                            // Active Schedule, set liveFeedStatus to on
                            liveFeedStatus.Value = "on";

                            // Set the ooyala id
                            ooyalaId.Value = GetAttributeValue( "OoyalaContentID" );

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
    }
}