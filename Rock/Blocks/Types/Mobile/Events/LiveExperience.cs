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
using Rock.Data;
using Rock.Event.InteractiveExperiences;
using Rock.Mobile;
using Rock.Model;
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.LiveExperience;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Events
{
    /// <summary>
    /// The Live Experience mobile block..
    /// </summary>
    /// <seealso cref="RockBlockType" />
    [DisplayName( "Live Experience" )]
    [Category( "Mobile > Events" )]
    [Description( "Block that is used to connect to a Live Experience from within your mobile application." )]
    [IconCssClass( "fa fa-tv" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage("Live Experience Web Page",
        Description = "The page to link the live experience to. This page should contain a Live Experience block.",
        IsRequired = true,
        Key = AttributeKeys.LiveExperienceWebPage,
        Order = 0 
    )]

    [BooleanField( "Always Request Location",
        Description = "Location data will always be included if available. If not available and this is enabled then access to the device location will be requested.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Key = AttributeKeys.AlwaysRequestLocation,
        Order = 1
    )]

    [BooleanField( "Keep Screen On",
        Description = "Keeps the screen turned on when this page is being used by the application.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Key = AttributeKeys.KeepScreenOn,
        Order = 2
    )]

    #endregion

    [SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_EVENTS_LIVEEXPERIENCE_BLOCK_TYPE )]
    [SystemGuid.BlockTypeGuid( "969EB376-281C-41D8-B7E9-A183DEA751DB" )]
    public class LiveExperience : RockBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the live experience web page unique identifier.
        /// </summary>
        /// <value>The live experience web page unique identifier.</value>
        private Guid? LiveExperienceWebPageGuid => GetAttributeValue( AttributeKeys.LiveExperienceWebPage ).AsGuidOrNull();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 4 );

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            var url = this.GetLinkedPageUrl( AttributeKeys.LiveExperienceWebPage, new Dictionary<string, string>
            {
                ["Token"] = "((Token))"
            } );

            
            return new
            {
                Url = MobileHelper.BuildPublicApplicationRootUrl( url ),
                AlwaysRequestLocation = GetAttributeValue( AttributeKeys.AlwaysRequestLocation ).AsBoolean(),
                KeepScreenOn = GetAttributeValue( AttributeKeys.KeepScreenOn ).AsBoolean()
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
            public const string LiveExperienceWebPage = "LiveExperienceWebPage";
            public const string KeepScreenOn = "KeepScreenOn";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the interactive experience entity from page parameters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="idKey">The identifier key of the occurrence.</param>
        /// <returns>The <see cref="InteractiveExperience"/> to be viewed or edited on the page.</returns>
        private InteractiveExperienceOccurrence GetInteractiveExperienceOccurrence( RockContext rockContext, string idKey )
        {
            var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );

            return occurrenceService.GetQueryableByKey( idKey, !PageCache.Layout.Site.DisablePredictableIds )
                .AsNoTracking()
                .Include( o => o.InteractiveExperienceSchedule.InteractiveExperience )
                .SingleOrDefault();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Request to join an experience. This does not join the experience but
        /// instead returns a token that can be used to join the experience via
        /// the linked web page.
        /// </summary>
        /// <param name="occurrenceKey">The identifier key of the interactive experience occurrence.</param>
        /// <param name="personalDeviceGuid">The optional personal device unique identifier used for push notifications.</param>
        /// <param name="latitude">The optional latitude representing the location of the device.</param>
        /// <param name="longitude">The optional longitude representing the location of the device.</param>
        /// <returns>A response containing the token or an error.</returns>
        [BlockAction]
        public BlockActionResult JoinExperience( string occurrenceKey, Guid? personalDeviceGuid, double? latitude, double? longitude )
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new LiveExperienceInitializationBox();
                var occurrence = GetInteractiveExperienceOccurrence( rockContext, occurrenceKey );

                if ( occurrence == null )
                {
                    return ActionNotFound( "Interactive Experience Occurrence was not found." );
                }

                if ( !occurrence.InteractiveExperienceSchedule.InteractiveExperience.IsActive )
                {
                    return ActionBadRequest( "This Interactive Experience is not currently active." );
                }

                int? personalDeviceId = null;

                if ( personalDeviceGuid.HasValue )
                {
                    var personalDeviceService = new PersonalDeviceService( rockContext );

                    personalDeviceId = personalDeviceService.GetId( personalDeviceGuid.Value );
                }

                var experienceToken = new ExperienceToken
                {
                    OccurrenceId = occurrence.IdKey,
                    InteractionGuid = Guid.NewGuid(),
                    PersonalDeviceId = personalDeviceId,
                    CampusId = occurrence.GetIndivualCampusId( RequestContext.CurrentPerson, latitude, longitude )
                };

                return ActionOk( new
                {
                    Token = Rock.Security.Encryption.EncryptString( experienceToken.ToJson() )
                } );
            }
        }

        /// <summary>
        /// Leaves the interactive experience. This is called when the page is
        /// being removed from the navigation stack.
        /// </summary>
        /// <param name="token">The experience token to leave.</param>
        /// <returns>An object that indicates if the request was successful.</returns>
        [BlockAction]
        public BlockActionResult LeaveExperience( string token )
        {
            var decryptedToken = Rock.Security.Encryption.DecryptString( token );
            var experienceToken = decryptedToken.FromJsonOrThrow<ExperienceToken>();

            if ( experienceToken.InteractionGuid.HasValue )
            {
                InteractiveExperienceOccurrenceService.FinalizeInteraction( experienceToken.InteractionGuid.Value );
            }

            return ActionOk();
        }

        #endregion
    }
}
