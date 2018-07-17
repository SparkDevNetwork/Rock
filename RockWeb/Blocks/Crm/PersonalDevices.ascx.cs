// <copyright>
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
using System.ComponentModel;
using System.Web.UI;
using Rock.Attribute;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Linq;
using System.Collections.Generic;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Personal Devices" )]
    [Category( "CRM" )]
    [Description( "Shows a list of all person devices." )]

    [LinkedPage( "Interactions Page", "The interactions associated with a specific personal device." )]
    [CodeEditorField( "Lava Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"
<div class=""panel panel-block"">       
    <div class=""panel-heading"">
        <h4 class=""panel-title"">
            <div class=""panel-panel-title""><i class=""fa fa-mobile""></i> {{ Person.FullName }}</div>
        </h4>
    </div>
    <div class=""panel-body"">
        <div class=""row row-eq-height-md"">
            {% for item in PersonalDevices %}
                <div class=""col-md-3 col-sm-4"">                  
                    <div class=""well margin-b-none rollover-container"">                        
                        <a class=""pull-right rollover-item btn btn-xs btn-danger"" href=""#"" onclick=""Rock.dialogs.confirm('Are you sure you want to delete this device?', function (result) { if (result ){{ item.PersonalDevice.Id | Postback:'DeleteDevice' }}}) ""><i class=""fa fa-times""></i></a>
                        <div style=""min-height: 120px;"">
                            <h3 class=""margin-v-none"">
                                {% if item.DeviceIconCssClass != '' %}
                                    <i class=""fa {{ item.DeviceIconCssClass }}""></i>
                                {% endif %}
                                {% if item.PersonalDevice.NotificationsEnabled == true %}
                                    <i class=""fa fa-comment-o""></i>
                                {% endif %}
                            </h3>
                            <dl>
                                {% if item.PlatformValue != '' %}<dt>{{ item.PlatformValue }} {{ item.PersonalDevice.DeviceVersion }}</dt>{% endif %}
                                {% if item.PersonalDevice.CreatedDateTime != null %}<dt>Discovered</dt><dd>{{ item.PersonalDevice.CreatedDateTime }}</dd>{% endif %}                              
                                {% if item.PersonalDevice.MACAddress != '' and item.PersonalDevice.MACAddress != null %}<dt>MAC Address</dt><dd>{{ item.PersonalDevice.MACAddress }}</dd>{% endif %}
                            </dl>
                        </div>
                        {% if LinkUrl != '' %}
                            <a href=""{{ LinkUrl | Replace:'[Id]',item.PersonalDevice.Id }}"" class=""btn btn-default btn-xs""> Interactions</a>
                        {% endif %}
                    </div>
                </div>
            {% endfor %}
        </div>
    </div>
</div>
", "", 2, "LavaTemplate" )]
    [ContextAware( typeof( Person ) )]
    public partial class PersonalDevices : RockBlock
    {
        #region Fields

        private Person _person = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            Guid? personGuid = PageParameter( "PersonGuid" ).AsGuidOrNull();
            if ( personGuid.HasValue )
            {
                _person = new PersonService( new RockContext() ).Get( personGuid.Value );
            }
            else if ( this.ContextEntity<Person>() != null )
            {
                _person = this.ContextEntity<Person>();
            }

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            RouteAction();
            if ( !Page.IsPostBack )
            {
                LoadContent();
            }
            base.OnLoad( e );
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
            RouteAction();
            LoadContent();
        }

        #endregion

        #region Methods

        protected void LoadContent()
        {
            if ( _person != null )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "Person", _person );

                RockContext rockContext = new RockContext();

                var personalDeviceService = new PersonalDeviceService( rockContext );
                var personalDevices = personalDeviceService.Queryable().Where( a => a.PersonAlias != null && a.PersonAlias.PersonId == _person.Id );

                var items = personalDevices.Select( a => new PersonalDeviceItem
                {
                    PersonalDevice = a
                } ).ToList();

                foreach ( var item in items )
                {
                    if ( item.PersonalDevice.PersonalDeviceTypeValueId.HasValue )
                    {
                        var value = DefinedValueCache.Get( item.PersonalDevice.PersonalDeviceTypeValueId.Value );
                        item.DeviceIconCssClass = value.GetAttributeValue( "IconCssClass" );
                    }

                    if ( item.PersonalDevice.PlatformValueId.HasValue )
                    {
                        item.PlatformValue = DefinedValueCache.Get( item.PersonalDevice.PlatformValueId.Value ).Value;

                    }
                }

                mergeFields.Add( "PersonalDevices", items );

                var queryParams = new Dictionary<string, string>();
                queryParams.Add( "personalDeviceId", "_PersonDeviceIdParam_" );
                string url = LinkedPageUrl( "InteractionsPage", queryParams );
                if ( !string.IsNullOrWhiteSpace( url ) )
                {
                    url = url.Replace( "_PersonDeviceIdParam_", "[Id]" );
                }

                mergeFields.Add( "LinkUrl", url );


                string template = GetAttributeValue( "LavaTemplate" );
                lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );
            }
            else
            {
                lContent.Text = string.Format( "<div class='alert alert-warning'>No Person is selected." );
            }
        }

        /// <summary>
        /// Route the request to the correct panel
        /// </summary>
        private void RouteAction()
        {
            int personalDeviceId = 0;
            var sm = ScriptManager.GetCurrent( Page );

            if ( Request.Form["__EVENTARGUMENT"] != null )
            {
                string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];

                    int argument = 0;
                    int.TryParse( parameters, out argument );

                    switch ( action )
                    {
                        case "DeleteDevice":
                            personalDeviceId = int.Parse( parameters );
                            DeleteDevice( personalDeviceId );
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the personal device.
        /// </summary>
        /// <param name="personalDeviceId">The personal device identifier.</param>
        private void DeleteDevice( int personalDeviceId )
        {
            try
            {
                var rockContext = new RockContext();
                var interactionService = new InteractionService( rockContext );
                var personalDeviceService = new PersonalDeviceService( rockContext );
                var personalDevice = personalDeviceService.Get( personalDeviceId );
                if ( personalDevice != null )
                {
                    var interactions = interactionService.Queryable( "PersonalDevice" )
                                    .Where( a => a.PersonalDeviceId == personalDeviceId )
                                    .ToList();
                    if ( interactions.Count > 0 )
                    {
                        foreach ( var interaction in interactions )
                        {
                            interaction.PersonalDevice = null;
                        }
                    }
                    personalDeviceService.Delete( personalDevice );
                    rockContext.SaveChanges();

                }
            }
            catch {}

            LoadContent();
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A class to store personal device for lava
        /// </summary>
        [DotLiquid.LiquidType( "PersonalDevice", "DeviceIconCssClass", "PlatformValue" )]
        public class PersonalDeviceItem
        {
            /// <summary>
            /// Gets or sets the personal device.
            /// </summary>
            /// <value>
            /// The personal device.
            /// </value>
            public PersonalDevice PersonalDevice { get; set; }

            /// <summary>
            /// Gets or sets the device iconCssClass.
            /// </summary>
            /// <value>
            /// The device iconCssClass.
            /// </value>
            public string DeviceIconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the platform value.
            /// </summary>
            /// <value>
            /// The platform value.
            /// </value>
            public string PlatformValue { get; set; }
        }

        #endregion
    }
}