// <copyright>
// Copyright by LCBC Church
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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Utility;

namespace RockWeb.Plugins.com_bemaservices.CheckIn
{
    [DisplayName( "Reprint Labels" )]
    [Category( "BEMA Services > Check-in" )]
    [Description( "Used to quickly reprint a child's label" )]
    public partial class ReprintLabel : CheckInBlockMultiPerson
    {
        #region Fields

        // used for private variables
        private const string USER_SETTING_LABELGUID = "PrintTest:Label";
        private const string USER_SETTING_DEVICEID = "PrintTest:Device";
        private const string USER_SETTING_PERSONID = "PrintTest:Person";

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the Click event of the bbtnPrint control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPrint_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            if ( ppPerson.PersonId.HasValue )
            {
                PersonService personService = new PersonService( rockContext );
                var person = personService.Get( ppPerson.PersonId.Value );
                if ( person != null )
                {
                    var queryParams = new Dictionary<string, string>();
                    queryParams.Add( "PersonId", person.Id.ToString() );
                    NavigateToCurrentPage( queryParams );
                }
            }
        }

        /// <summary>
        /// Saves the user settings.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="label">The label.</param>
        /// <param name="person">The person.</param>
        private void SaveUserSettings( ListItem device, ListItem label, CheckInPerson person )
        {
            SetUserPreference( USER_SETTING_DEVICEID, device.Value );
            SetUserPreference( USER_SETTING_LABELGUID, label.Value );
            if ( person.Person != null )
            {
                SetUserPreference( USER_SETTING_PERSONID, person.Person.Id.ToString() );
            }
        }

        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

        // setup deviceready event to wait for cordova
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
                        'An error occurred while printing the labels.' + error[0],  // message
                        alertDismissed,         // callback
                        'Error',            // title
                        'Ok'                  // buttonName
                    );
			    }}
            );
	    }}
", jsonObject );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
        }

        /// <summary>
        /// Uses the user preferences.
        /// </summary>
        private void UseUserPreferences()
        {
            var deviceId = GetUserPreference( USER_SETTING_DEVICEID );
            var labelGuid = GetUserPreference( USER_SETTING_LABELGUID );
            var personId = GetUserPreference( USER_SETTING_PERSONID );


            if ( !string.IsNullOrWhiteSpace( personId ) )
            {
                PersonService personService = new PersonService( new RockContext() );
                var p = personService.Get( personId.AsInteger() );
                ppPerson.SetValue( p );
            }
        }

        #endregion

        protected void btnReturn_Click( object sender, EventArgs e )
        {
            NavigateToHomePage();
        }
    }
}