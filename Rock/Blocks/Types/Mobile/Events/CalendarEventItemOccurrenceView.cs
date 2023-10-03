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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;

namespace Rock.Blocks.Types.Mobile.Events
{
    /// <summary>
    /// Displays a particular calendar event item occurrence.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Calendar Event Item Occurrence View" )]
    [Category( "Mobile > Events" )]
    [Description( "Displays a particular calendar event item occurrence." )]
    [IconCssClass( "fa fa-calendar-day" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [TextField( "Registration Url",
        Description = "The base URL to use when linking to the registration page.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKeys.RegistrationUrl,
        Order = 0 )]

    [BlockTemplateField( "Template",
        Description = "The template to use when rendering the event.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_ITEM_OCCURRENCE_VIEW,
        IsRequired = true,
        DefaultValue = "6593D4EB-2B7A-4C24-8D30-A02991D26BC0",
        Key = AttributeKeys.Template,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_EVENTS_CALENDAREVENTITEMOCCURRENCEVIEW_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "15DD270A-A0BB-45BF-AA36-FE37856C60DE")]
    public class CalendarEventItemOccurrenceView : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the MobileContent block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The registration URL key.
            /// </summary>
            public const string RegistrationUrl = "RegistrationUrl";

            /// <summary>
            /// The template key attribute key.
            /// </summary>
            public const string Template = "Template";
        }

        /// <summary>
        /// Gets the registration URL.
        /// </summary>
        /// <value>
        /// The registration URL.
        /// </value>
        protected string RegistrationUrl => GetAttributeValue( AttributeKeys.RegistrationUrl );

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <inheritdoc/>
        public override Guid? MobileBlockTypeGuid => new Guid( "7258A210-E936-4260-B573-9FA1193AD9E2" ); // Content block.

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            var additionalSettings = BlockCache?.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();

            return new Rock.Common.Mobile.Blocks.Content.Configuration
            {
                ProcessLava = additionalSettings.ProcessLavaOnClient,
                DynamicContent = true
            };
        }

        #endregion

        #region Methods

        private string GetContent()
        {
            Guid eventItemOccurrenceGuid = Guid.Empty;
            RockContext rockContext = new RockContext();

            // get the calendarItem id
            if ( !string.IsNullOrWhiteSpace( RequestContext.GetPageParameter( "EventOccurrenceGuid" ) ) )
            {
                eventItemOccurrenceGuid = RequestContext.GetPageParameter( "EventOccurrenceGuid" ).AsGuid();
            }

            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
            var qry = eventItemOccurrenceService
                .Queryable( "EventItem, EventItem.Photo, Campus, Linkages" )
                .Where( i => i.Guid == eventItemOccurrenceGuid );

            var eventItemOccurrence = qry.FirstOrDefault();

            if ( eventItemOccurrence == null )
            {
                return "<Rock:NotificationBox NotificationType=\"Warning\" Text=\"We could not find that event.\" />";
            }

            var mergeFields = new Dictionary<string, object>
            {
                { "RegistrationUrl", RegistrationUrl },
                { "EventItemOccurrence", eventItemOccurrence },
                { "Event", eventItemOccurrence?.EventItem },
                { "CurrentPerson", RequestContext.CurrentPerson }
            };

            //var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
            var contextCampus = RequestContext.GetContextEntity<Campus>();
            if ( contextCampus != null )
            {
                mergeFields.Add( "CampusContext", contextCampus );
            }

            // determine registration status (Register, Full, or Join Wait List) for each unique registration instance
            Dictionary<int, string> registrationStatusLabels = new Dictionary<int, string>();
            foreach ( var registrationInstance in eventItemOccurrence.Linkages.Select( a => a.RegistrationInstance ).Distinct().ToList() )
            {
                int? maxRegistrantCount = null;
                var currentRegistrationCount = 0;

                if ( registrationInstance != null )
                {
                    maxRegistrantCount = registrationInstance.MaxAttendees;
                }


                int? registrationSpotsAvailable = null;
                if ( maxRegistrantCount.HasValue )
                {
                    currentRegistrationCount = new RegistrationRegistrantService( rockContext ).Queryable()
                        .AsNoTracking()
                        .Where( r => r.Registration.RegistrationInstanceId == registrationInstance.Id && r.OnWaitList == false )
                        .Count();
                    registrationSpotsAvailable = maxRegistrantCount - currentRegistrationCount;
                }

                string registrationStatusLabel = "Register";

                if ( registrationSpotsAvailable.HasValue && registrationSpotsAvailable.Value < 1 )
                {
                    if ( registrationInstance.RegistrationTemplate.WaitListEnabled )
                    {
                        registrationStatusLabel = "Join Wait List";
                    }
                    else
                    {
                        registrationStatusLabel = "Full";
                    }
                }

                registrationStatusLabels.Add( registrationInstance.Id, registrationStatusLabel );
            }

            // Status of first registration instance
            mergeFields.Add( "RegistrationStatusLabel", registrationStatusLabels.Values.FirstOrDefault() );

            // Status of each registration instance 
            mergeFields.Add( "RegistrationStatusLabels", registrationStatusLabels );

            return Template.ResolveMergeFields( mergeFields );
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the initial content for this block.
        /// </summary>
        /// <returns>The initial content.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            return new CallbackResponse
            {
                Content = GetContent()
            };
        }

        #endregion
    }
}
