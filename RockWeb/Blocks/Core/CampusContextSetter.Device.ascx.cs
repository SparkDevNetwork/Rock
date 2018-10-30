﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block that can be used to set the default campus context for the site
    /// </summary>
    [DisplayName( "Campus Context Setter - Device" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the campus context for the site based on the location of the device." )]
    [CodeEditorField( "Display Lava", "The Lava template to use when displaying the current campus.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, true, @"{% if Device %}

    {% if Campus %}
        Campus: {{Campus.Name}}
    {% else %}
        Could not determine the campus from the device {{ Device.Name }}.
    {% endif %}
    
{% else %}
    <div class='alert alert-danger'>
        Unable to determine the device. Please check the device settings.
        <br/>
        IP Address: {{ ClientIp }}
    </div>
{% endif %}" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.DEVICE_TYPE, "Device Type", "Optional filter to limit to specific device types.", false )]
    [CustomRadioListField("Context Scope", "The scope of context to set", "Site,Page", true, "Site")]
    public partial class CampusContextSetter : RockBlock
    {
        #region Base Control Methods

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

            SetCampus();
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetCampus();
        }

        #endregion

        #region Methods

        private void SetCampus()
        {
            RockContext rockContext = new RockContext();
            Campus campus = null;
            
            // get device
            string deviceIp = GetIPAddress();
            DeviceService deviceService = new DeviceService(rockContext);
            
            var deviceQry = deviceService.Queryable( "Location" )
                    .Where( d => d.IPAddress == deviceIp );
            
            // add device type filter
            if (!string.IsNullOrWhiteSpace(GetAttributeValue("DeviceType"))) {
                Guid givingKioskGuid = new Guid( GetAttributeValue("DeviceType"));
                deviceQry = deviceQry.Where( d => d.DeviceType.Guid == givingKioskGuid);
            }

            var device = deviceQry.FirstOrDefault();

            if ( device != null )
            {
                if ( device.Locations.Count > 0 )
                {
                    campus = new CampusService( new RockContext() ).Get( device.Locations.First().CampusId.Value );

                    // set the context
                    if ( campus != null )
                    {
                        var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
                        var currentCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                        if ( currentCampus == null || currentCampus.Id != campus.Id )
                        {
                            bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";
                            RockPage.SetContextCookie( campus, pageScope, true );
                        }
                    }
                }
            }

            // set display output
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "ClientIp", deviceIp );
            mergeFields.Add( "Device", device );
            mergeFields.Add( "Campus", campus );

            lOutput.Text = GetAttributeValue( "DisplayLava" ).ResolveMergeFields( mergeFields );

        }


        /// <summary>
        /// Gets the ip address. From: http://stackoverflow.com/questions/735350/how-to-get-a-users-client-ip-address-in-asp-net
        /// </summary>
        /// <returns></returns>
        private string GetIPAddress()
        {
            string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if ( !string.IsNullOrEmpty( ipAddress ) )
            {
                string[] addresses = ipAddress.Split( ',' );
                if ( addresses.Length != 0 )
                {
                    return addresses[0];
                }
            }

            return Request.ServerVariables["REMOTE_ADDR"];
        }

        #endregion
    }
}