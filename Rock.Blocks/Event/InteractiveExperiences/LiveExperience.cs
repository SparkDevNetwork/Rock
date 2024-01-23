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
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.LiveExperience;
using Rock.ViewModels.Event.InteractiveExperiences;
using Rock.Web.Cache;

namespace Rock.Blocks.Event.InteractiveExperiences
{
    /// <summary>
    /// Displays a live interactive experience.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Live Experience" )]
    [Category( "Event > Interactive Experiences" )]
    [Description( "Displays a live interactive experience" )]
    [IconCssClass( "fa fa-tv" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField( "Keep Alive Interval",
        Description = "How often in seconds the browser will inform the server it is still here.",
        IsRequired = true,
        DefaultIntegerValue = 30,
        Key = AttributeKey.KeepAliveInterval,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "9a853836-5155-4ce5-817f-49bfcbe7502c" )]
    [Rock.SystemGuid.BlockTypeGuid( "ba26f4fc-f6db-462e-9697-bd6a0504a0a8" )]
    public class LiveExperience : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string KeepAliveInterval = "KeepAliveInterval";
        }

        private static class PageParameterKey
        {
            public const string InteractiveExperienceOccurrenceId = "InteractiveExperienceOccurrenceId";
            public const string Latitude = "Latitude";
            public const string Longitude = "Longitude";
            public const string NoCount = "NoCount";
            public const string PersonalDeviceId = "PersonalDeviceId";
            public const string Token = "Token";
        }

        private static class NavigationUrlKey
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new LiveExperienceInitializationBox();
                var token = RequestContext.GetPageParameter( PageParameterKey.Token );
                var experienceToken = token.IsNotNullOrWhiteSpace()
                    ? Rock.Security.Encryption.DecryptString( token ).FromJsonOrNull<ExperienceToken>()
                    : null;

                if ( experienceToken == null )
                {
                    var occurrence = GetInteractiveExperienceOccurrence( rockContext, PageParameterKey.InteractiveExperienceOccurrenceId );
                    var noCount = RequestContext.GetPageParameter( PageParameterKey.NoCount ).AsBoolean();
                    var latitude = RequestContext.GetPageParameter( PageParameterKey.Latitude ).AsDoubleOrNull();
                    var longitude = RequestContext.GetPageParameter( PageParameterKey.Longitude ).AsDoubleOrNull();
                    var personalDeviceIdKey = RequestContext.GetPageParameter( PageParameterKey.PersonalDeviceId );

                    if ( occurrence == null )
                    {
                        box.ErrorMessage = "Interactive Experience Occurrence was not found.";
                        return box;
                    }

                    var experienceCache = InteractiveExperienceCache.Get( occurrence.InteractiveExperienceSchedule.InteractiveExperienceId );

                    if ( !experienceCache.IsActive )
                    {
                        box.ErrorMessage = "This Interactive Experience is not currently active.";
                        return box;
                    }

                    int? personalDeviceId = null;

                    if ( personalDeviceIdKey.IsNotNullOrWhiteSpace() )
                    {
                        var personalDeviceService = new PersonalDeviceService( rockContext );

                        // Don't ever allow passing an integer identifier otherwise
                        // somebody could get another person to receive push notifications.
                        personalDeviceId = personalDeviceService.GetQueryableByKey( personalDeviceIdKey, false )
                            .Select( pd => ( int? ) pd.Id )
                            .FirstOrDefault();
                    }

                    experienceToken = new ExperienceToken
                    {
                        OccurrenceId = occurrence.IdKey,
                        InteractionGuid = !noCount ? ( Guid? ) Guid.NewGuid() : null,
                        PersonalDeviceId = personalDeviceId,
                        CampusId = occurrence.GetIndivualCampusId( RequestContext.CurrentPerson, latitude, longitude )
                    };

                    box.ExperienceToken = Encryption.EncryptString( experienceToken.ToJson() );
                    box.Style = experienceCache.GetExperienceStyleBag();
                    box.IsExperienceInactive = !occurrence.IsOccurrenceActive;
                    box.ExperienceEndedContent = GetExperienceEndedContent( occurrence, experienceCache );
                }
                else
                {
                    var occurrence = new InteractiveExperienceOccurrenceService( rockContext )
                        .GetInclude( experienceToken.OccurrenceId, o => o.InteractiveExperienceSchedule );
                    var experienceCache = InteractiveExperienceCache.Get( occurrence.InteractiveExperienceSchedule.InteractiveExperienceId );

                    box.ExperienceToken = token;
                    box.Style = experienceCache.GetExperienceStyleBag();
                    box.IsExperienceInactive = !occurrence.IsOccurrenceActive;
                    box.ExperienceEndedContent = GetExperienceEndedContent( occurrence, experienceCache );
                }

                box.SecurityGrantToken = GetSecurityGrantToken();
                box.NavigationUrls = GetBoxNavigationUrls();
                box.KeepAliveInterval = GetKeepAliveInterval();

                return box;
            }
        }

        /// <summary>
        /// Gets the interactive experience entity from page parameters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="InteractiveExperience"/> to be viewed or edited on the page.</returns>
        private InteractiveExperienceOccurrence GetInteractiveExperienceOccurrence( RockContext rockContext, string entityIdKey )
        {
            var entityId = RequestContext.GetPageParameter( entityIdKey );
            var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );

            return occurrenceService.GetQueryableByKey( entityId, !PageCache.Layout.Site.DisablePredictableIds )
                .AsNoTracking()
                .Include( o => o.InteractiveExperienceSchedule )
                .SingleOrDefault();
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Gets the keep alive interval in seconds.
        /// </summary>
        /// <returns>A number indicating the number of seconds between keep-alive messages.</returns>
        private int GetKeepAliveInterval()
        {
            var seconds = GetAttributeValue( AttributeKey.KeepAliveInterval ).AsInteger();

            return Math.Max( 1, seconds );
        }

        /// <summary>
        /// Gets the content to display when the experience has ended.
        /// </summary>
        /// <param name="occurrence">The occurrence that will be monitored.</param>
        /// <param name="experience">The experience the occurrences belongs to.</param>
        /// <returns>A string that contains the content to be displayed.</returns>
        private string GetExperienceEndedContent( InteractiveExperienceOccurrence occurrence, InteractiveExperienceCache experience )
        {
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.AddOrReplace( "Occurrence", occurrence );
            mergeFields.AddOrReplace( "Experience", experience );

            return experience.ExperienceSettings
                .ExperienceEndedTemplate
                .ResolveMergeFields( mergeFields )
                .Trim();
        }

        #endregion
    }
}
