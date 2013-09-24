//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Confirmation Block" )]
    [LinkedPage("Activity Select Page")]
    public partial class Confirm : CheckInBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( this.Page, "http://www.sparkdevnetwork.org/public/js/cordova-2.4.0.js" );
            RockPage.AddScriptLink( this.Page, "http://www.sparkdevnetwork.org/public/js/ZebraPrint.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
                }
            }
        }

        /// <summary>
        /// Creates the grid data source.
        /// </summary>
        protected void BindGrid()
        {
            var selectedPeopleList = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Selected ).OrderBy( p => p.Person.FullNameLastFirst ).ToList();
            var checkInGrid = new System.Data.DataTable();
            checkInGrid.Columns.Add( "PersonId", typeof(int) );
            checkInGrid.Columns.Add( "Name", typeof(string) );
            checkInGrid.Columns.Add( "Location", typeof(string) );
            checkInGrid.Columns.Add( "LocationId", typeof(int) );
            checkInGrid.Columns.Add( "Schedule", typeof(string) );
            checkInGrid.Columns.Add( "ScheduleId", typeof(int) );
            
            foreach ( var person in selectedPeopleList )
            {
                int personId = person.Person.Id;
                string personName = person.Person.FullName;
                var locations = person.GroupTypes.Where( gt => gt.Selected )
                    .SelectMany( gt => gt.Groups ).Where( g => g.Selected )
                    .SelectMany( g => g.Locations ).Where( l => l.Selected ).ToList();                    
                                
                foreach ( var location in locations )
                {
                    var schedule = location.Schedules.Where( s => s.Selected ).FirstOrDefault();                    
                    checkInGrid.Rows.Add( personId, personName, location.Location.Name, location.Location.Id, schedule.Schedule.Name, schedule.Schedule.Id );
                }                
            }

            gPersonList.DataSource = checkInGrid;
            gPersonList.DataBind();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        /// <summary>
        /// Handles the Click event of the lbDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDone_Click( object sender, EventArgs e )
        {
            GoNext();
        }
        
        /// <summary>
        /// Handles the Click event of the lbPrintAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrintAll_Click( object sender, EventArgs e )
        {
            SaveAttendance();
            foreach ( DataKey dataKey in gPersonList.DataKeys )
            {               
                //var dataKeyValues = gPersonList.DataKeys[index].Values;
                var personId = Convert.ToInt32( dataKey["PersonId"] );
                var locationId = Convert.ToInt32( dataKey["LocationId"] );
                var scheduleId = Convert.ToInt32( dataKey["ScheduleId"] );
                PrintLabel( personId, locationId, scheduleId );               
            }
        }

        /// <summary>
        /// Handles the Edit event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonList_Edit( object sender, RowEventArgs e )
        {
            var dataKeyValues = gPersonList.DataKeys[e.RowIndex].Values;
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "personId", dataKeyValues["PersonId"].ToString() );
            queryParams.Add( "locationId", dataKeyValues["LocationId"].ToString() );
            NavigateToLinkedPage( "ActivitySelectPage", queryParams);
        }

        /// <summary>
        /// Handles the Delete event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonList_Delete( object sender, RowEventArgs e )
        {
            var dataKeyValues = gPersonList.DataKeys[e.RowIndex].Values;
            var personId = Convert.ToInt32( dataKeyValues["PersonId"] );
            var locationId = Convert.ToInt32( dataKeyValues["LocationId"] );
            var scheduleId = Convert.ToInt32( dataKeyValues["ScheduleId"] );
            
            var selectedPerson = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == personId ).FirstOrDefault();
            var selectedGroups = selectedPerson.GroupTypes.Where( gt => gt.Selected ).SelectMany( gt => gt.Groups );
            CheckInGroup selectedGroup = selectedGroups.Where( g => g.Selected
                && g.Locations.Any( l => l.Location.Id == locationId
                    && l.Schedules.Any( s => s.Schedule.Id == scheduleId ) ) ).FirstOrDefault();
            CheckInLocation selectedLocation = selectedGroup.Locations.Where( l => l.Selected 
                && l.Location.Id == locationId 
                    && l.Schedules.Any( s => s.Schedule.Id == scheduleId ) ).FirstOrDefault();
            CheckInSchedule selectedSchedule = selectedLocation.Schedules.Where( s => s.Selected 
                && s.Schedule.Id == scheduleId ).FirstOrDefault();
            
            selectedSchedule.Selected = false;
            selectedSchedule.PreSelected = false;
            selectedLocation.Selected = false;
            selectedLocation.PreSelected = false;
            if ( selectedGroups.Count() == 1 )
            {
                selectedGroup.Selected = false;
                selectedGroup.PreSelected = false;
                selectedPerson.Selected = false;
                selectedPerson.PreSelected = false;
            }
            
            BindGrid();
        }

        /// <summary>
        /// Handles the RowCommand event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCommandEventArgs"/> instance containing the event data.</param>
        protected void gPersonList_Print( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "Print" )
            {
                SaveAttendance();                
                int index = Convert.ToInt32( e.CommandArgument );                
                var dataKeyValues = gPersonList.DataKeys[index].Values;
                var personId = Convert.ToInt32( dataKeyValues["PersonId"] );
                var locationId = Convert.ToInt32( dataKeyValues["LocationId"] );
                var scheduleId = Convert.ToInt32( dataKeyValues["ScheduleId"] );
                PrintLabel( personId, locationId, scheduleId );
            }
        }        

        #endregion

        #region Internal Methods

        /// <summary>
        /// Saves the attendance and loads the labels.
        /// </summary>
        private void SaveAttendance()
        {
            var errors = new List<string>();
            if ( ProcessActivity( "Save Attendance", out errors ) )
            {
                SaveState();
                NavigateToNextPage();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }
               
        /// <summary>
        /// Goes the back.
        /// </summary>
        private void GoBack()
        {            
            SaveState();
            NavigateToPreviousPage();
        }

        /// <summary>
        /// Goes the next.
        /// </summary>
        private void GoNext()
        {
            CurrentCheckInState.CheckIn.SearchType = null;
            CurrentCheckInState.CheckIn.SearchValue = string.Empty;
            SaveState();
            NavigateToNextPage();
        }

        /// <summary>
        /// Prints the label.
        /// </summary>
        /// <param name="person">The person.</param>
        private void PrintLabel( int personId, int locationId, int scheduleId )
        {
            CheckInPerson selectedPerson = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();
            List<CheckInGroupType> selectedGroupTypes = selectedPerson.GroupTypes.Where( gt => gt.Selected 
                && gt.Groups.Any( g => g.Selected && g.Locations.Any( l => l.Location.Id == locationId 
                    && l.Schedules.Any( s => s.Schedule.Id == scheduleId ) ) ) ).ToList();
            
            foreach ( var groupType in selectedGroupTypes )
            {
                var printFromClient = groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Client);
                if ( printFromClient.Any() )
                {
                    AddLabelScript( printFromClient.ToJson() );
                }

                var printFromServer = groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server );
                if ( printFromServer.Any() )
                {
                    Socket socket = null;
                    string currentIp = string.Empty;

                    foreach ( var label in printFromServer )
                    {
                        var labelCache = KioskLabel.Read( label.FileId );
                        if ( labelCache != null )
                        {
                            if ( label.PrinterAddress != currentIp )
                            {
                                if ( socket != null && socket.Connected )
                                {
                                    socket.Shutdown( SocketShutdown.Both );
                                    socket.Close();
                                }

                                currentIp = label.PrinterAddress;
                                var printerIp = new IPEndPoint( IPAddress.Parse( currentIp ), 9100 );

                                socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                                IAsyncResult result = socket.BeginConnect( printerIp, null, null );
                                bool success = result.AsyncWaitHandle.WaitOne( 5000, true );
                            }

                            string printContent = labelCache.FileContent;
                            foreach ( var mergeField in label.MergeFields )
                            {
                                var rgx = new Regex( string.Format( @"(?<=\^FD){0}(?=\^FS)", mergeField.Key ) );
                                printContent = rgx.Replace( printContent, mergeField.Value );
                            }

                            if ( socket.Connected )
                            {
                                var ns = new NetworkStream( socket );
                                byte[] toSend = System.Text.Encoding.ASCII.GetBytes( printContent );
                                ns.Write( toSend, 0, toSend.Length );
                            }
                            else
                            {
                                maWarning.Show( "Could not connect to printer.", ModalAlertType.Warning );
                            }
                        }
                    }

                    if ( socket != null && socket.Connected )
                    {
                        socket.Shutdown( SocketShutdown.Both );
                        socket.Close();
                    }
                }
            }            
        }

        /// <summary>
        /// Adds the label script.
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

            // setup deviceready event to wait for cordova
	        document.addEventListener('deviceready', onDeviceReady, false);

	        // label data
            var labelData = {0};

		    function onDeviceReady() {{
	
			    //navigator.notification.alert('Oh boy! It's going to be a good day!, alertDismissed, 'Success', 'Continue');
			    printLabels();
		    }}
		
		    function alertDismissed() {{
		        // do something
		    }}
		
		    function printLabels() {{
		        ZebraPrintPlugin.printTags(
            	    JSON.stringify(labelData), 
            	    function(result) {{ 
			            console.log('I printed that tag like a champ!!!');
			        }},
			        function(error) {{   
				        // error is an array where:
				        // error[0] is the error message
				        // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			            console.log('An error occurred: ' + error[0]);
			        }}
                );
	        }}", jsonObject );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
        }

        #endregion        
    }
}