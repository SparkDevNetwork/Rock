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
using Rock.Utility;
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.ExperienceVisualizer;
using Rock.ViewModels.Event.InteractiveExperiences;
using Rock.Web.Cache;

namespace Rock.Blocks.Event.InteractiveExperiences
{
    /// <summary>
    /// Displays the visuals of an experience.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Experience Visualizer" )]
    [Category( "Event > Interactive Experiences" )]
    [Description( "Displays the visuals of an experience." )]
    [IconCssClass( "fa fa-chart" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField( "Keep Alive Interval",
        Description = "How often in seconds the browser will inform the server it is still here.",
        IsRequired = true,
        DefaultIntegerValue = 30,
        Key = AttributeKey.KeepAliveInterval,
        Order = 0 )]

    [CustomDropdownListField( "Interactive Experience",
        Description = "The interactive experience to use when determining the active occurrence.",
        ListSource = "SELECT [Guid] as [Value], [Name] as [Text] FROM [InteractiveExperience] WHERE [IsActive] = 1",
        IsRequired = false,
        Key = AttributeKey.InteractiveExperience,
        Order = 1 )]

    [CampusField( "Campus",
        includeInactive: false,
        Description = "The campus to use when determining which experience occurrence to display. If no campus is selected then only occurrences with no campus will be considered.",
        IsRequired = false,
        Key = AttributeKey.Campus,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "932a5219-0822-4d88-b8a7-e0f5c301348a" )]
    [Rock.SystemGuid.BlockTypeGuid( "b98abf9b-9345-48c6-a15d-4dd5ac73f0bc" )]
    public class ExperienceVisualizer : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string KeepAliveInterval = "KeepAliveInterval";
            public const string InteractiveExperience = "InteractiveExperience";
            public const string Campus = "Campus";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new ExperienceVisualizerInitializationBox
                {
                    SecurityGrantToken = GetSecurityGrantToken(),
                    NavigationUrls = GetBoxNavigationUrls(),
                    KeepAliveInterval = GetKeepAliveInterval()
                };

                return box;
            }
        }

        /// <summary>
        /// Gets the interactive experience unique identifier configured in the
        /// block settings.
        /// </summary>
        /// <returns>The interactive experience unique identifier.</returns>
        private Guid? GetBlockInteractiveExperienceGuid()
        {
            return GetAttributeValue( AttributeKey.InteractiveExperience ).AsGuidOrNull();
        }

        /// <summary>
        /// Gets the campus unique identifier configured in the block settings.
        /// </summary>
        /// <returns>The campus unique identifier or <c>null</c> if not configured.</returns>
        private Guid? GetBlockCampusGuid()
        {
            return GetAttributeValue( AttributeKey.Campus ).AsGuidOrNull();
        }

        /// <summary>
        /// Gets the best interactive experience occurrence that is currently active.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="VisualizerOccurrenceBag"/> to be shown on the page.</returns>
        private VisualizerOccurrenceBag GetCurrentOccurrence( RockContext rockContext )
        {
            var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );
            var experienceGuid = GetBlockInteractiveExperienceGuid();
            var campusGuid = GetBlockCampusGuid();
            var qry = occurrenceService.GetActiveOccurrences();

            // If we have an experience specified in settings then limit to that.
            if ( experienceGuid.HasValue )
            {
                qry = qry.Where( o => o.InteractiveExperienceSchedule.InteractiveExperience.Guid == experienceGuid.Value );
            }

            // If we have a campus then only include that campus, otherwise
            // only include occurrences with no campus.
            if ( campusGuid.HasValue )
            {
                qry = qry.Where( o => o.Campus.Guid == campusGuid.Value );
            }
            else
            {
                qry = qry.Where( o => !o.CampusId.HasValue );
            }

            // Get the best result item.
            var result = qry
                .AsNoTracking()
                .OrderBy( o => o.Id )
                .Select( o => new
                {
                    o.Id,
                    o.InteractiveExperienceSchedule.InteractiveExperienceId,
                    o.OccurrenceDateTime,
                    o.InteractiveExperienceSchedule.Schedule
                } )
                .FirstOrDefault();

            if ( result == null )
            {
                return null;
            }

            var experienceCache = InteractiveExperienceCache.Get( result.InteractiveExperienceId );
            var experienceToken = new ExperienceToken
            {
                OccurrenceId = IdHasher.Instance.GetHash( result.Id ),
                IsVisualizer = true
            };

            return new VisualizerOccurrenceBag
            {
                ExperienceToken = Encryption.EncryptString( experienceToken.ToJson() ),
                Style = experienceCache.GetExperienceStyleBag(),
                OccurrenceEndDateTime = result.OccurrenceDateTime.AddMinutes( result.Schedule.DurationInMinutes ).ToRockDateTimeOffset()
            };
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

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the current occurrence to be shown by the visualizer.
        /// </summary>
        /// <returns>The visualizer data to be displayed or null if no occurrence is active.</returns>
        [BlockAction]
        public BlockActionResult GetCurrentOccurrence()
        {
            using ( var rockContext = new RockContext() )
            {
                return ActionOk( GetCurrentOccurrence( rockContext ) );
            }
        }

        /// <summary>
        /// Gets all the answers for the specified experience occurrence.
        /// </summary>
        /// <param name="occurrenceKey">The occurrence identifier.</param>
        /// <returns>A collection of <see cref="ExperienceAnswerBag"/> objects that represent the answers.</returns>
        [BlockAction]
        public BlockActionResult GetExperienceAnswers( string token )
        {
            var experienceToken = Encryption.DecryptString( token ).FromJsonOrNull<ExperienceToken>();
            var occurrenceIdKey = experienceToken?.OccurrenceId;

            if ( occurrenceIdKey.IsNullOrWhiteSpace() )
            {
                return ActionNotFound( "Invalid experience token." );
            }

            using ( var rockContext = new RockContext() )
            {
                var occurrenceIntegerId = IdHasher.Instance.GetId( occurrenceIdKey );

                if ( !occurrenceIntegerId.HasValue )
                {
                    return ActionNotFound( "Experience occurrence was not found." );
                }

                var answerService = new InteractiveExperienceAnswerService( rockContext );
                var answers = answerService.GetAnswerBagsForOccurrence( occurrenceIntegerId.Value ).ToList();

                return ActionOk( answers );
            }
        }

        #endregion
    }
}
