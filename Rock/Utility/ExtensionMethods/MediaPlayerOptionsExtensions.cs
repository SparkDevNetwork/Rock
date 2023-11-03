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
using System.Linq;

using Rock.Data;
using Rock.Media;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Extension methods related to <see cref="MediaPlayerOptions"/>.
    /// </summary>
    internal static class MediaPlayerOptionsExtensions
    {
        /// <summary>
        /// Updates the options from a <see cref="MediaElement"/>. If one is
        /// not found then no changes are made.
        /// </summary>
        /// <param name="options">The options to be updated.</param>
        /// <param name="mediaElementId">The media element identifier.</param>
        /// <param name="mediaElementGuid">The media element unique identifier.</param>
        /// <param name="autoResumeInDays">The number of days back to look for an existing watch map to auto-resume from. Pass -1 to mean forever or 0 to disable.</param>
        /// <param name="combinePlayStatisticsInDays">The number of days back to look for an existing interaction to be updated. Pass -1 to mean forever or 0 to disable.</param>
        /// <param name="currentPerson">The person to use when searching for existing interactions.</param>
        /// <param name="personAliasId">If <paramref name="currentPerson"/> is <c>null</c> then this value will be used to optionally find an existing interaction.</param>
        internal static void UpdateValuesFromMedia( this MediaPlayerOptions options, int? mediaElementId, Guid? mediaElementGuid, int autoResumeInDays, int combinePlayStatisticsInDays, Person currentPerson, int? personAliasId )
        {
            if ( !mediaElementId.HasValue && !mediaElementGuid.HasValue )
            {
                return;
            }

            // If they specified both the media element guid and the url
            // then assume they provided everything we need.
            if ( options.MediaElementGuid.HasValue && options.MediaUrl.IsNotNullOrWhiteSpace() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var mediaElementService = new MediaElementService( rockContext );
                var interactionService = new InteractionService( rockContext );
                var mediaEventsChannelGuid = Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS.AsGuid();
                var now = RockDateTime.Now;
                MediaElement mediaElement = null;

                // Load the media element by either Id or Guid value.
                if ( mediaElementId.HasValue )
                {
                    mediaElement = mediaElementService.Get( mediaElementId.Value );
                }
                else
                {
                    mediaElement = mediaElementService.Get( mediaElementGuid.Value );
                }

                // No media found means we don't have anything to do.
                if ( mediaElement == null )
                {
                    return;
                }

                options.MediaUrl = mediaElement.DefaultFileUrl;
                options.MediaElementGuid = mediaElement.Guid;

                // Let the users value override the default thumbnail.
                if ( options.PosterUrl.IsNullOrWhiteSpace() )
                {
                    options.PosterUrl = mediaElement.DefaultThumbnailUrl;
                }

                // Check if either autoResumeInDays or combinePlayStatisticsInDays
                // are enabled. If not we are done.
                if ( autoResumeInDays == 0 && combinePlayStatisticsInDays == 0 )
                {
                    return;
                }

                // Build a query to find Interactions for this person having
                // previously watched this media element.
                var interactionQry = interactionService.Queryable()
                    .Where( i => i.InteractionComponent.InteractionChannel.Guid == mediaEventsChannelGuid
                        && i.InteractionComponent.EntityId == mediaElement.Id );

                if ( currentPerson != null )
                {
                    interactionQry = interactionQry.Where( i => i.PersonAlias.PersonId == currentPerson.Id );
                }
                else if ( personAliasId.HasValue )
                {
                    interactionQry = interactionQry.Where( i => i.PersonAliasId == personAliasId.Value );
                }
                else
                {
                    // If we don't have a person, then we can't get interactions.
                    return;
                }

                // A negative value means "forever".
                int daysBack = Math.Max( autoResumeInDays >= 0 ? autoResumeInDays : int.MaxValue,
                    combinePlayStatisticsInDays >= 0 ? combinePlayStatisticsInDays : int.MaxValue );

                // A value of MaxValue means "forever" now so we don't need
                // to filter on it.
                if ( daysBack != int.MaxValue )
                {
                    var limitDateTime = now.AddDays( -daysBack );

                    interactionQry = interactionQry.Where( i => i.InteractionDateTime >= limitDateTime );
                }

                // Look for the most recent Interaction.
                var interaction = interactionQry.OrderByDescending( i => i.InteractionDateTime )
                    .Select( i => new
                    {
                        i.Guid,
                        i.InteractionDateTime,
                        i.InteractionData
                    } )
                    .FirstOrDefault();

                // If we didn't find any interaction then we are done.
                if ( interaction == null )
                {
                    return;
                }

                // Check if this interaction is within our auto-resume window.
                if ( autoResumeInDays != 0 )
                {
                    if ( autoResumeInDays < 0 || interaction.InteractionDateTime >= now.AddDays( -autoResumeInDays ) )
                    {
                        if ( !options.ResumePlaying.HasValue )
                        {
                            options.ResumePlaying = true;
                        }

                        var data = interaction.InteractionData.FromJsonOrNull<MediaWatchedInteractionData>();
                        options.Map = data?.WatchMap;
                    }
                }

                // Check if this interaction is within our combine window.
                if ( combinePlayStatisticsInDays != 0 )
                {
                    if ( combinePlayStatisticsInDays < 0 || interaction.InteractionDateTime >= now.AddDays( -combinePlayStatisticsInDays ) )
                    {
                        options.InteractionGuid = interaction.Guid;
                    }
                }
            }
        }
    }
}
