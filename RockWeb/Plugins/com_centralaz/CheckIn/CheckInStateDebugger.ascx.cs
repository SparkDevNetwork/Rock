// <copyright>
// Copyright by Central Christian Church
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
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.CheckIn
{
    /// <summary>
    /// A simple debugging tool for developers and super-admins that dumps the CheckInState object
    /// and a few other details to the screen when enabled. Just drop the block on a check-in page
    /// and enable the debug block setting or pass in debug=true to see the data dump.
    /// </summary>
    [DisplayName( "Check-in State Debugger" )]
    [Category( "com_centralaz > Check-in" )]
    [Description( "Exposes the CurrentCheckInState object and a few other details for assistance with development and troubleshooting for the current family when 'debug' is passed via the querystring or the debug block setting is enabled." )]
    [BooleanField( "Enable Debug", "When enabled, outputs the debugging information. Alternatively you can pass 'debug=true' into the querystring to enable debugging.", true )]

    public partial class CheckInStateDebugger : CheckInBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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

            if ( !this.IsPostBack &&
                ( !string.IsNullOrEmpty( PageParameter( "debug" ) ) || GetAttributeValue( "EnableDebug" ).AsBoolean() ) )
            {
                upnlContent.Visible = true;
                DumpCheckInState();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DumpCheckInState();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Dumps the state of the check in.
        /// </summary>
        private void DumpCheckInState()
        {
            if ( CurrentCheckInState == null )
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder sbFull = new StringBuilder();

            sbFull.AppendFormat( "<small>Kiosk: {0} w/GroupTypeIds: {1}</small><br/>", CurrentCheckInState.Kiosk.Device.Name, String.Join( ",", CurrentGroupTypeIds ) );
            sbFull.Append( "<small>Locations:<ul>" );
            foreach ( var location in CurrentCheckInState.Kiosk.Locations( CurrentGroupTypeIds, new RockContext() ) )
            {
                sbFull.AppendFormat( "<li>{0}</li>", location.Name );
            }
            sbFull.Append( "</ul></small>" );

            if ( CurrentCheckInState.CheckIn != null &&
                CurrentCheckInState.CheckIn.CurrentFamily != null )
            {
                var family = CurrentCheckInState.CheckIn.CurrentFamily;

                foreach ( var person in family.People.ToList() )
                {
                    foreach ( var gt in person.GroupTypes )
                    {
                        foreach ( var g in gt.Groups )
                        {
                            sbFull.AppendFormat( "<h2>{0}</h2><br/><pre>{1}</pre>", person.Person.FullName, Newtonsoft.Json.JsonConvert.SerializeObject( gt, Newtonsoft.Json.Formatting.Indented ) );

                            StringBuilder sblocs = new StringBuilder();
                            foreach ( var l in g.Locations )
                            {
                                sblocs.AppendFormat( "{0}{1}; ", l.Location.Name, ( l.Location.IsActive ) ? "" : " (closed)" );
                            }
                            sb.AppendFormat( "{0} - {1} - {2} ({3})<br>", person.Person.FullName, gt.ToString(), g.Group.Name, sblocs.ToString() );
                        }
                    }
                }
            }

            lDebugBrief.Text = sb.ToString();
            lDebugFull.Text = sbFull.ToString();
        }
        #endregion
    }
}