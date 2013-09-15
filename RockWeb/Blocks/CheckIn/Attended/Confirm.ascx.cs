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
            var selectedPeopleList = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault().People.Where( p => p.Selected ).OrderBy( p => p.Person.FullNameLastFirst ).ToList();
            var checkInGrid = new System.Data.DataTable();
            checkInGrid.Columns.Add( "Id", typeof(int) );
            checkInGrid.Columns.Add( "Name", typeof(string) );
            checkInGrid.Columns.Add( "AssignedTo", typeof(string) );
            checkInGrid.Columns.Add( "Time", typeof(string) );
            checkInGrid.Columns.Add( "LocationId", typeof( int ) );

            foreach ( var person in selectedPeopleList )
            {
                int personId = person.Person.Id;
                string personName = person.Person.FullName;
                //var assignments = person.GroupTypes.Where( gt => gt.Selected )
                //    .SelectMany( gt => gt.Groups ).Where( g => g.Selected )
                //    .Select( g => new {
                //        Id = personId,
                //        Name = personName,
                //        AssignedTo = g.Group.Name,
                //        Time = g.Locations.Where( l => l.Selected )
                //          .Where( l => l.Schedules.Any( s => s.Selected ) )
                //          .SelectMany( l => l.Schedules.Select( s => s.Schedule.Name ) ).FirstOrDefault()
                //    } ).ToList();
                var assignments = person.GroupTypes.Where( gt => gt.Selected )
                    .SelectMany( gt => gt.Groups ).Where( g => g.Selected )
                    .SelectMany( g => g.Locations ).Where( l => l.Selected )
                    .Select( l => new
                    {
                        Id = personId,
                        Name = personName,
                        AssignedTo = l.Location.Name,
                        Time = l.Schedules.Where( s => s.Selected ).Select( s => s.Schedule.Name ).FirstOrDefault(),
                        LocationId = l.Location.Id.ToString()
                    } ).ToList();

                foreach ( var assignment in assignments )
                {
                    checkInGrid.Rows.Add( assignment.Id, assignment.Name, assignment.AssignedTo, assignment.Time, assignment.LocationId );
                }

                if ( !assignments.Any() )
                {
                    checkInGrid.Rows.Add( personId, personName, string.Empty, string.Empty, 0 );   
                }                        
            }

            gPersonList.DataSource = checkInGrid;
            gPersonList.DataBind();
            
            gPersonList.CssClass = string.Empty;
            gPersonList.AddCssClass( "grid-table" );
            gPersonList.AddCssClass( "table" );
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
        /// Handles the RowCommand event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCommandEventArgs"/> instance containing the event data.</param>
        protected void gPersonList_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "Print" )
            {
                // Retrieve the row index stored in the CommandArgument property.
                int index = Convert.ToInt32( e.CommandArgument );

                // Retrieve the row that contains the button from the Rows collection.
                GridViewRow row = gPersonList.Rows[index];
                // get info about person
                //PrintLabel( personId, groupTypeId );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbPrintAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrintAll_Click( object sender, EventArgs e )
        {
            // Do some crazy printing crap in here where you can print labels for everyone listed in the grid.
            maWarning.Show( "If there was any code in here you would have just printed all the labels", ModalAlertType.Information );
        }

        /// <summary>
        /// Handles the Edit event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonList_Edit( object sender, RowEventArgs e )
        {
            // throw the user back to the activity select page for the person they want to edit.
            int index = e.RowIndex;
            var row = gPersonList.Rows[index];
            var dataKeyValues = gPersonList.DataKeys[index].Values;
            var locationId = dataKeyValues["LocationId"].ToString();
            var personId = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == e.RowKeyId ).FirstOrDefault().Person.Id.ToString();
            //var blah = row["AssignedTo"];
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "personId", personId );
            queryParams.Add( "locationId", locationId);
            NavigateToLinkedPage( "ActivitySelectPage", queryParams);
        }

        /// <summary>
        /// Handles the Delete event of the gPersonList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonList_Delete( object sender, RowEventArgs e )
        {
            // remove person
            //CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            //    .People.Where( p => p.Person.Id == e.RowKeyId ).FirstOrDefault().Selected = false;
            //BindGrid();

            // just remove the one particular item selected.
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == e.RowKeyId ).FirstOrDefault();

            int index = e.RowIndex;
            var row = gPersonList.Rows[index];
            var dataKeyValues = gPersonList.DataKeys[index].Values;
            var id = int.Parse( dataKeyValues["Id"].ToString() );
            var assignedTo = dataKeyValues["AssignedTo"].ToString();

            var selectedGroupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
            var selectedGroup = selectedGroupType.Groups.Where( g => g.Selected && g.Group.Name == assignedTo ).FirstOrDefault();
            var selectedLocation = selectedGroup.Locations.Where( l => l.Selected && l.Location.Name == assignedTo ).FirstOrDefault();
            var selectedSchedule = selectedLocation.Schedules.Where( s => s.Selected ).FirstOrDefault();

            selectedGroup.Selected = false;
            selectedLocation.Selected = false;
            selectedSchedule.Selected = false;

            // if there are no groups selected anymore, unselect the group type and the person
            selectedGroup = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault().Groups.Where( g => g.Selected ).FirstOrDefault();
            if ( selectedGroup == null )
            {
                selectedGroupType.Selected = false;
                person.Selected = false;
            }

            BindGrid();

        }

        #endregion

        #region Internal Methods
               
        /// <summary>
        /// Goes the back.
        /// </summary>
        private void GoBack()
        {
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();

            List<int> peopleIds = new List<int>();
            foreach ( var person in family.People.Where( p => p.Selected ) )
            {
                peopleIds.Add( person.Person.Id );
            }


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
        private void PrintLabel( int personId, int groupTypeId )
        {
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).SelectMany( f => f.People )
                .Where( p => p.Person.Id == personId ).FirstOrDefault();                

            foreach ( var groupType in person.GroupTypes.Where( gt => gt.Selected ).ToList() )
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