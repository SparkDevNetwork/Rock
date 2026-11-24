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

using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Block that can be used to set the campus context for the site based on the location of the device.
    /// </summary>
    [DisplayName( "Campus Context Setter - Device" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the campus context for the site based on the location of the device." )]
    [IconCssClass( "ti ti-device-desktop" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Display Lava",
        Description = "The Lava template to use when displaying the current campus.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = true,
        DefaultValue = @"{% if Device %}
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
{% endif %}",
        Order = 0,
        Key = AttributeKey.DisplayLava )]

    [DefinedValueField( "Device Type",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.DEVICE_TYPE,
        Description = "Optional filter to limit to specific device types.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.DeviceType )]

    [CustomRadioListField( "Context Scope",
        Description = "The scope of context to set",
        ListSource = "Site,Page",
        IsRequired = true,
        DefaultValue = "Site",
        Order = 2,
        Key = AttributeKey.ContextScope )]

    #endregion

    [ConfigurationChangedReload( BlockReloadMode.Block )]
    [Rock.Cms.DefaultBlockRole( BlockRole.System )]
    [Rock.SystemGuid.EntityTypeGuid( "121017F0-6310-485A-9D22-A35CE9DF6E9E" )]
    [Rock.SystemGuid.BlockTypeGuid( "0B9C6253-C72E-4CAA-B38A-BA359BC712E4" )]
    public class CampusContextSetterDevice : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DeviceType = "DeviceType";
            public const string ContextScope = "ContextScope";
            public const string DisplayLava = "DisplayLava";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The HTML rendered for this request, so the device lookup and
        /// Lava rendering only run once per request.
        /// </summary>
        private string _content;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            // Set the campus context (and redirect if it changed) as part of
            // the block initialization, the same as the WebForms OnLoad.
            GetContent();

            return base.GetObsidianBlockInitialization();
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            // Render the Lava output as static content so it behaves like the
            // WebForms literal, including any script tags in the template.
            return GetContent();
        }

        /// <summary>
        /// Finds the device for the client, sets the campus context from its
        /// location and renders the Display Lava template.
        /// </summary>
        /// <returns>The rendered HTML content.</returns>
        private string GetContent()
        {
            if ( _content != null )
            {
                return _content;
            }

            var clientIp = RequestContext.ClientInformation.IpAddress;
            var device = GetDevice( clientIp );
            var campusId = device?.Locations.FirstOrDefault()?.CampusId;

            // Use the entity rather than CampusCache so the Lava "Campus"
            // merge field exposes the same properties as the WebForms block.
            var campus = campusId.HasValue ? new CampusService( RockContext ).Get( campusId.Value ) : null;

            if ( campus != null )
            {
                SetCampusContextIfChanged( campus );
            }

            var mergeFields = RequestContext.GetCommonMergeFields();
            // The client IP can come from the X-Forwarded-For header, which the
            // client controls. Only pass it to Lava when it is a real IP address
            // so a spoofed header cannot inject markup into the page.
            var isValidClientIp = clientIp == "localhost" || System.Net.IPAddress.TryParse( clientIp ?? string.Empty, out _ );
            mergeFields.Add( "ClientIp", isValidClientIp ? clientIp : null );
            mergeFields.Add( "Device", device );
            mergeFields.Add( "Campus", campus );

            _content = GetAttributeValue( AttributeKey.DisplayLava ).ResolveMergeFields( mergeFields ) ?? string.Empty;

            return _content;
        }

        /// <summary>
        /// Gets the device registered with the specified IP address, limited
        /// to the configured device type when one is set.
        /// </summary>
        /// <param name="ipAddress">The IP address of the client.</param>
        /// <returns>The matching device or <c>null</c> if none was found.</returns>
        private Device GetDevice( string ipAddress )
        {
            if ( ipAddress.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // RockRequestContext reports the IPv6 loopback "::1" as "localhost",
            // but the WebForms block matched the raw "::1" address. Accept
            // both so devices registered either way still match.
            var alternateIpAddress = ipAddress == "localhost" ? "::1" : ipAddress;

            var deviceQry = new DeviceService( RockContext )
                .Queryable()
                .Include( d => d.Locations )
                .Where( d => d.IPAddress == ipAddress || d.IPAddress == alternateIpAddress );

            var deviceTypeGuid = GetAttributeValue( AttributeKey.DeviceType ).AsGuidOrNull();

            if ( deviceTypeGuid.HasValue )
            {
                var deviceTypeValueId = DefinedValueCache.GetId( deviceTypeGuid.Value );

                if ( !deviceTypeValueId.HasValue )
                {
                    return null;
                }

                deviceQry = deviceQry.Where( d => d.DeviceTypeValueId == deviceTypeValueId.Value );
            }

            return deviceQry.FirstOrDefault();
        }

        /// <summary>
        /// Sets the campus context to the specified campus and reloads the
        /// page so every block on it sees the new context. Nothing happens
        /// when the campus is already the current context.
        /// </summary>
        /// <param name="campus">The campus to set as the context.</param>
        private void SetCampusContextIfChanged( Campus campus )
        {
            var currentCampus = RequestContext.GetContextEntity<Campus>();

            if ( currentCampus?.Id == campus.Id )
            {
                return;
            }

            var pageScope = GetAttributeValue( AttributeKey.ContextScope ) == "Page";

            RequestContext.SetContextEntity( campus, pageScope );
            RequestContext.Response.RedirectToUrl( RequestContext.RequestUri.ToString() );
        }

        #endregion
    }
}
