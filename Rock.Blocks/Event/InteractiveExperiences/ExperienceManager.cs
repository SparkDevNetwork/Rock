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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Event;
using Rock.Event.InteractiveExperiences;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.ExperienceManager;
using Rock.ViewModels.Event.InteractiveExperiences;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event.InteractiveExperiences
{
    /// <summary>
    /// Manages an active interactive experience.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Experience Manager" )]
    [Category( "Event > Interactive Experiences" )]
    [Description( "Manages an active interactive experience." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Live Experience Page",
        "The page that will provide the live experience preview.",
        IsRequired = false,
        Key = AttributeKey.LiveExperiencePage,
        Order = 0 )]

    [IntegerField( "Participant Count Update Interval",
        Description = "The number of seconds between updates to the participant count. Setting this value too low can cause extra load on the server.",
        IsRequired = true,
        DefaultIntegerValue = 30,
        Key = AttributeKey.ParticipantCountUpdateInterval,
        Order = 1 )]

    [CustomCheckboxListField( "Tabs to Display",
        Description = "The tabs to be made visible to people managing the experience.",
        IsRequired = true,
        DefaultValue = "Live Event,Moderation,Live Questions",
        ListSource = "Live Event,Moderation,Live Questions",
        Key = AttributeKey.TabsToDisplay,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "5d2594d9-2695-41be-880c-966ff25bcf11" )]
    [Rock.SystemGuid.BlockTypeGuid( "7af57181-dd9a-446a-b321-abad900df9bc" )]
    public class ExperienceManager : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string LiveExperiencePage = "LiveExperiencePage";
            public const string ParticipantCountUpdateInterval = "ParticipantCountUpdateInterval";
            public const string TabsToDisplay = "TabsToDisplay";
        }

        private static class PageParameterKey
        {
            public const string InteractiveExperienceOccurrenceId = "InteractiveExperienceOccurrenceId";

            public const string NoCount = "NoCount";
        }

        private static class NavigationUrlKey
        {
            public const string LiveExperiencePage = "LiveExperiencePage";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );
                var box = new ExperienceManagerInitializationBox();
                var occurrence = GetInteractiveExperienceOccurrence( rockContext, PageParameterKey.InteractiveExperienceOccurrenceId );
                var experience = occurrence?.InteractiveExperienceSchedule.InteractiveExperience;

                if ( occurrence == null )
                {
                    box.ErrorMessage = "Interactive Experience Occurrence was not found.";
                    return box;
                }

                if ( !occurrence.InteractiveExperienceSchedule.InteractiveExperience.IsActive )
                {
                    box.ErrorMessage = "This Interactive Experience is not currently active.";
                    return box;
                }

                box.OccurrenceIdKey = occurrence.IdKey;
                box.ExperienceName = experience.Name;
                box.IsNotificationAvailable = experience.PushNotificationType == InteractiveExperiencePushNotificationType.SpecificActions;
                box.IsExperienceInactive = !occurrence.IsOccurrenceActive;
                box.ParticipantCount = occurrenceService.GetRecentParticipantCount( occurrence.Id );
                box.ParticipantCountUpdateInterval = GetParticipantCountUpdateInterval();
                box.Actions = GetExperienceActions( experience );
                box.TabsToShow = GetAttributeValue( AttributeKey.TabsToDisplay ).SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ).ToList();
                box.SecurityGrantToken = GetSecurityGrantToken();
                box.NavigationUrls = GetBoxNavigationUrls();

                var token = new ExperienceToken
                {
                    OccurrenceId = occurrence.IdKey,
                    IsModerator = true
                };

                box.ExperienceToken = Encryption.EncryptString( token.ToJson() );

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
                .Include( o => o.InteractiveExperienceSchedule.InteractiveExperience )
                .SingleOrDefault();
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.LiveExperiencePage] = this.GetLinkedPageUrl( AttributeKey.LiveExperiencePage, new Dictionary<string, string>
                {
                    [PageParameterKey.InteractiveExperienceOccurrenceId] = RequestContext.GetPageParameter( PageParameterKey.InteractiveExperienceOccurrenceId ),
                    [PageParameterKey.NoCount] = "true"
                } )
            };
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
        /// Gets the actions for this experience.
        /// </summary>
        /// <param name="experience">The experience.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> objects that represent the actions.</returns>
        private static List<ListItemBag> GetExperienceActions( InteractiveExperience experience )
        {
            var items = new List<ListItemBag>();

            foreach ( var action in experience.InteractiveExperienceActions )
            {
                var actionComponent = ActionTypeContainer.GetComponentFromEntityType( action.ActionEntityTypeId );

                if ( actionComponent == null )
                {
                    continue;
                }

                items.Add( new ListItemBag
                {
                    Value = action.IdKey,
                    Text = actionComponent.GetDisplayTitle( action ),
                    Category = actionComponent.IconCssClass
                } );
            }

            return items;
        }

        /// <summary>
        /// Gets the participant count update interval in seconds.
        /// </summary>
        /// <returns>The number of seconds between updates.</returns>
        private int GetParticipantCountUpdateInterval()
        {
            var seconds = GetAttributeValue( AttributeKey.ParticipantCountUpdateInterval ).AsInteger();

            return Math.Max( 1, seconds );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets all the answers for the specified experience occurrence.
        /// </summary>
        /// <param name="occurrenceKey">The occurrence identifier.</param>
        /// <returns>A collection of <see cref="ExperienceAnswerBag"/> objects that represent the answers.</returns>
        [BlockAction]
        public BlockActionResult GetExperienceAnswers( string occurrenceKey )
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceIntegerId = IdHasher.Instance.GetId( occurrenceKey );

                if ( !occurrenceIntegerId.HasValue )
                {
                    return ActionNotFound( "Experience occurrence was not found." );
                }

                var answerService = new InteractiveExperienceAnswerService( rockContext );
                var answers = answerService.GetAnswerBagsForOccurrence( occurrenceIntegerId.Value ).ToList();

                return ActionOk( answers );
            }
        }

        /// <summary>
        /// Deletes the experience answer completely from the system.
        /// </summary>
        /// <param name="key">The identifier of the answer to be deleted.</param>
        /// <returns>A 200 OK result if the answer was deleted; otherwise an error result.</returns>
        [BlockAction]
        public async Task<BlockActionResult> DeleteExperienceAnswer( string key )
        {
            var answerId = IdHasher.Instance.GetId( key );

            if ( !answerId.HasValue )
            {
                return ActionNotFound( "Response was not found." );
            }

            var deleted = await InteractiveExperienceAnswerService.DeleteAnswer( answerId.Value );

            if ( !deleted )
            {
                return ActionNotFound( "Response could not be deleted." );
            }

            return ActionOk();
        }

        /// <summary>
        /// Updates the experience answer approval status.
        /// </summary>
        /// <param name="key">The identifier of the answer to be updated.</param>
        /// <param name="status">The new status of the answer.</param>
        /// <returns>A 200 OK result if the answer was updated; otherwise an error result.</returns>
        [BlockAction]
        public async Task<BlockActionResult> UpdateExperienceAnswerStatus( string key, InteractiveExperienceApprovalStatus status )
        {
            var answerId = IdHasher.Instance.GetId( key );

            if ( !answerId.HasValue )
            {
                return ActionNotFound( "Response was not found." );
            }

            var updated = await InteractiveExperienceAnswerService.UpdateAnswerStatus( answerId.Value, status );

            if ( !updated )
            {
                return ActionNotFound( "Response could not be updated." );
            }

            return ActionOk();
        }

        #endregion
    }
}
