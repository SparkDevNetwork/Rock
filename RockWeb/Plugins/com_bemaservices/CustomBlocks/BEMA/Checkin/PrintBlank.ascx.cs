// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Communication = Rock.Model.Communication;
using FieldType = Rock.SystemGuid.FieldType;

/*
 * BEMA Custom Block ( v9.2.1)
 * 
 * Features:
 * Print fake testing label based on Label design and printer. useful when setting up printers or testing labels. 
 */

namespace RockWeb.Plugins.com_bemaservices.CustomBlocks.BEMA.Checkin
{
    [DisplayName( "Print Test Label" )]
    [Category( "BEMA > Checkin" )]
    [Description( "Print Test Labels" )]
   
    public partial class PrintBlank : RockBlock
    {
        

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/cordova-2.4.0.js", false );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/ZebraPrint.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );
            
        }
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !IsPostBack )
            {
                BindControls();
            }
            
        }

       

        private void BindControls()
        {
            
            // Printer list
            ddlKiosk.Items.Clear();
            rddlLabel.Items.Clear();

            //Get Printers in Devices that have a location of the selected Campus (does not iterate through parent locations to find campus, must be assigned campus directly)
            var printers = new DeviceService( new RockContext() ).GetByDeviceTypeGuid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER.AsGuid() ).ToList();

            //Filter down printers by Campus
            //printers = printers.Where( p => p.Locations.Any( l => l.CampusId == hfCampusId.Value.AsIntegerOrNull() ) ).ToList();

            //var listPrinters = new List<Device>();

            //Add Local Printer with 0 as ID
            //listPrinters.Add( new Device { Name = "Local Print", Id = 0, PrintFrom = Rock.Model.PrintFrom.Client } );

            //listPrinters.AddRange( printers );
            
            ddlKiosk.DataSource = printers.Select( d => new { d.Id, d.Name } ).ToList();
            ddlKiosk.DataBind();
            ddlKiosk.Items.Insert( 0, new ListItem( None.Text, "-1" ) );

            var selectedPrinter = Session["LocationDetail_SelectedPrinter"];
            
            if (selectedPrinter != null && ddlKiosk.Items.FindByValue(selectedPrinter.ToString()) != null)
            {
                ddlKiosk.SelectedValue = selectedPrinter.ToString();
            }
            
            
            btnPrint.Enabled = ( ddlKiosk.SelectedValue.AsIntegerOrNull() >= 0 );


            Guid labelTypeGuid = Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid();
            foreach ( var labelFile in new BinaryFileService( new RockContext() )
                .Queryable().AsNoTracking()
                .Where( f => f.BinaryFileType.Guid == labelTypeGuid )
                .OrderBy( f => f.FileName ) )
            {
                rddlLabel.Items.Add( new ListItem( labelFile.FileName, labelFile.Guid.ToString() ) );
            }

        }
        
        
        protected void btnPrint_Click( object sender, EventArgs e )
        {
            try
            {
                using ( var rockContext = new RockContext() )
                {
                    //var attendanceService = new AttendanceService( rockContext );
                    //var attendance = attendanceService.Get( attendanceId.Value );
                    //attendance.LoadAttributes();

                    var commonMergeFields = LavaHelper.GetCommonMergeFields( null );

                    var schedule = new CheckInSchedule
                    {
                        Schedule = new Schedule() { Name = "Test Schedule" },
                        Selected = true
                    };

                    var location = new CheckInLocation
                    {
                        Location = new Location() { Name = "Test Location" },
                        Schedules = new List<CheckInSchedule> { schedule },
                        Selected = true
                    };

                    var group = new CheckInGroup
                    {
                        Group = new Rock.Model.Group() { Name = "Test Group" },
                        Locations = new List<CheckInLocation> { location },
                        Selected = true
                    };

                    var groupType = new CheckInGroupType
                    {
                        GroupType = GroupTypeCache.All().First(),
                        Groups = new List<CheckInGroup> { group },
                        Labels = new List<CheckInLabel>(),
                        Selected = true
                    };

                    var person = new CheckInPerson
                    {
                        Person = new Rock.Model.Person() { FirstName = "Test", NickName = "Test", LastName = "Testing" },
                        SecurityCode = "1234",
                        GroupTypes = new List<CheckInGroupType> { groupType },
                        FirstTime = true,
                        Selected = true
                    };

                    //Gets kiosk label from binary file guid
                    var labelCache = KioskLabel.Get( rddlLabel.SelectedValueAsGuid().Value );


                    if ( labelCache != null )
                    {
                        person.SetOptions( labelCache );

                        var mergeObjects = new Dictionary<string, object>();
                        foreach ( var keyValue in commonMergeFields )
                        {
                            mergeObjects.Add( keyValue.Key, keyValue.Value );
                        }

                        mergeObjects.Add( "Location", location );
                        mergeObjects.Add( "Group", group );
                        mergeObjects.Add( "Person", person );
                        mergeObjects.Add( "GroupType", groupType );


                       

                        var groupMembers = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                m.PersonId == person.Person.Id &&
                                                m.GroupId == group.Group.Id )
                                            .ToList();
                        mergeObjects.Add( "GroupMembers", groupMembers );

                        //get printer
                        var printer = new DeviceService( rockContext ).Get( ddlKiosk.SelectedValue.AsInteger() );

                        var label = new CheckInLabel( labelCache, mergeObjects, person.Person.Id )
                        {
                            FileGuid = labelCache.Guid,
                            PrintTo = PrintTo.Kiosk,
                            PrinterDeviceId = printer.IsNotNull() ? ( int? ) printer.Id : null,
                            PrinterAddress = printer.IsNotNull() ? printer.IPAddress : null
                        };
                        if ( label.PrinterDeviceId.HasValue )
                        {
                            if ( label.PrinterDeviceId == 0 ) // print to local printer
                            {
                                var urlRoot = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority );
                                label.PrintFrom = PrintFrom.Client;
                                label.PrinterAddress = "127.0.0.1";
                                label.LabelFile = urlRoot + label.LabelFile;
                                AddLabelScript( new List<CheckInLabel> { label }.ToJson() ); //copied from success.ascs.cs in checkin
                            }
                            if ( label.PrintFrom == PrintFrom.Client )
                            {
                                var urlRoot = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority );
                                label.LabelFile = urlRoot + label.LabelFile;
                                AddLabelScript( new List<CheckInLabel> { label }.ToJson() ); //copied from success.ascs.cs in checkin
                            }
                            else // print to IP address
                            {
                                var printerDevice = new DeviceService( rockContext ).Get( label.PrinterDeviceId.Value );
                                if ( printerDevice != null )
                                {
                                    label.PrinterAddress = printerDevice.IPAddress;
                                    groupType.Labels.Add( label );

                                    ZebraPrint.PrintLabels( new List<CheckInLabel> { label } );
                                }
                            }

                        }


                    }
                    else
                    { btnPrint.Text = "No Label Found"; }
                }
            }
            catch ( Exception ex )
            {
                LogException( ex );
            }

        }

        

        protected void ddlKiosk_SelectedIndexChanged( object sender, EventArgs e )
        {
            Session["LocationDetail_SelectedPrinter"] = ddlKiosk.SelectedValue;
            btnPrint.Enabled = ( ddlKiosk.SelectedValue.AsIntegerOrNull() >= 0 );
        }

        
        
        /// <summary>
        /// Adds the label script for client print. Copied from Success.ascx.cs from checkin
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

	    if (navigator.userAgent.match(/(iPhone|iPod|iPad)/)) {{
            document.addEventListener('deviceready', onDeviceReady, false);
        }} else {{
            $( document ).ready(function() {{
                onDeviceReady();
            }});
        }}


	    // label data
        var labelData = {0};

		function onDeviceReady() {{
            try {{			
                printLabels();
            }} 
            catch (err) {{
                console.log('An error occurred printing labels: ' + err);
            }}
		}}
		
		function alertDismissed() {{
		    // do something
		}}
		
		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData), 
            	function(result) {{ 
			        console.log('Tag printed');
			    }},
			    function(error) {{   
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
                    navigator.notification.alert(
                        'An error occurred while printing the labels. ' + error[0] + ' ' + JSON.stringify(labelData),  // message plus label
                        alertDismissed,         // callback
                        'Error',            // title
                        'Ok'                  // buttonName
                    );
			    }}
            );
	    }}
        
        //manually start
        onDeviceReady();

", jsonObject );
            //ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
            ScriptManager.RegisterClientScriptBlock( upContent, upContent.GetType(), "addLabel", script, true );
            
        }

        
    }


}