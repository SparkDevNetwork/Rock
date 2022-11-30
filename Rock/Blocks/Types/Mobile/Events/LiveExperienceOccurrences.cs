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
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Event.InteractiveExperiences;
using Rock.Model;

namespace Rock.Blocks.Types.Mobile.Events
{
    /// <summary>
    /// Displays a lava formatted list of experience occurrences that are
    /// currently happening.
    /// </summary>
    /// <seealso cref="RockMobileBlockType" />
    [DisplayName( "Live Experience Occurrences" )]
    [Category( "Mobile > Events" )]
    [Description( "Displays a lava formatted list of experience occurrences that are currently happening." )]
    [IconCssClass( "fa fa-tv" )]

    #region Block Attributes

    [LinkedPage( "Destination Page",
        Description = "The page to link to when selecting an occurrence.",
        IsRequired = true,
        Key = AttributeKeys.DestinationPage,
        Order = 0
    )]

    [LinkedPage( "Login Page",
        Description = "The page to use when showing the login page. If not set then the default application login page will be used instead.",
        IsRequired = false,
        Key = AttributeKeys.LoginPage,
        Order = 1 )]

    [BooleanField( "Show All",
        Description = "When enabled, normal filtering is not performed and all active occurrences will be shown. Intended for use on admin pages.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Key = AttributeKeys.ShowAll,
        Order = 2 )]

    [BooleanField( "Always Request Location",
        Description = "When enabled, the device location will always be requested. Otherwise it will only be used if it has already been requested in the past.",
        DefaultBooleanValue = false,
        Key = AttributeKeys.AlwaysRequestLocation,
        Order = 3 )]

    #endregion

    [SystemGuid.EntityTypeGuid( "af20692a-9ae1-4faf-a506-d408b14652d1" )]
    [SystemGuid.BlockTypeGuid( "c45ba1c6-ce7f-4c37-82bf-a86d28bb28fe" )]
    public class LiveExperienceOccurrences : RockMobileBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the live experience page unique identifier.
        /// </summary>
        /// <value>The live experience page unique identifier.</value>
        private Guid? LiveExperiencePageGuid => GetAttributeValue( AttributeKeys.DestinationPage ).AsGuidOrNull();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version.
        /// </summary>
        /// <value>The required mobile application binary interface version.</value>
        public override int RequiredMobileAbiVersion => 4;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>The class name of the mobile block to use during rendering on the device</value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Events.InteractiveExperienceOccurrences";

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                AlwaysRequestLocation = GetAttributeValue( AttributeKeys.AlwaysRequestLocation ).AsBoolean(),
                LoginPageGuid = GetAttributeValue( AttributeKeys.LoginPage ).AsGuidOrNull()
            };
        }

        #endregion

        #region Keys

        /// <summary>
        /// The attribute keys we plan to use in this block.
        /// </summary>
        private static class AttributeKeys
        {
            public const string AlwaysRequestLocation = "AlwaysRequestLocation";
            public const string DestinationPage = "DestinationPage";
            public const string LoginPage = "LoginPage";
            public const string ShowAll = "ShowAll";
        }

        #endregion

        #region Methods

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the content to display after merging all the information with
        /// Lava into a XAML template.
        /// </summary>
        /// <param name="latitude">The latitude of the device, if known.</param>
        /// <param name="longitude">The longitude of the device, if known.</param>
        /// <returns>A callback response object that contains the XAML content to display.</returns>
        [BlockAction]
        public BlockActionResult GetContent( double? latitude, double? longitude )
        {
            var showAll = GetAttributeValue( AttributeKeys.ShowAll ).AsBoolean();

            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );
                var validOccurrences = showAll
                    ? new ValidOccurrencesResult( occurrenceService.GetActiveOccurrences().ToList(), false, false )
                    : occurrenceService.GetValidOccurrences( RequestContext.CurrentPerson, latitude, longitude );

                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "DestinationPageGuid", GetAttributeValue( AttributeKeys.DestinationPage ).AsGuid() );
                mergeFields.Add( "Occurrences", validOccurrences.Occurrences.ToList() );
                mergeFields.Add( "LoginRecommended", validOccurrences.LoginRecommended );
                mergeFields.Add( "GeoLocationRecommended", validOccurrences.GeoLocationRecommended );

                var lavaTemplate = @"<StackLayout>
{% for occurrence in Occurrences %}
{% if occurrence.Campus != null %}
    {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }} at {{ occurrence.Campus.Name }}{% endcapture %}
{% else %}
    {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }}{% endcapture %}
{% endif %}
<Button Text=""{{ occurrenceName | Escape }}"" StyleClass=""btn,btn-primary"" Command=""{Binding PushPage}"" CommandParameter=""{{ DestinationPageGuid }}?InteractiveExperienceOccurrenceKey={{ occurrence.Guid }}"" />
{% endfor %}

{% if LoginRecommended == true %}
<Button Text=""Login"" StyleClass=""btn,btn-secondary"" Command=""{Binding InteractiveExperienceOccurrences.Login}"" />
{% endif %}

{% if GeoLocationRecommended == true %}
<Button Text=""Provide Location"" StyleClass=""btn,btn-secondary"" />
{% endif %}
</StackLayout>";

                return ActionOk( new CallbackResponse
                {
                    Content = lavaTemplate.ResolveMergeFields( mergeFields )
                } );
            }
        }

        #endregion
    }
}
