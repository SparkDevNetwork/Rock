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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// <see cref="MediaElement"/> REST API
    /// </summary>
    [RockGuid( "c77504af-fbed-4009-b2d4-019911413693" )]
    public partial class MediaElementsController
    {
        /// <summary>
        /// Gets the most recent MediaElementInteraction for the specified
        /// MediaElement and person.
        /// </summary>
        /// <param name="mediaElementGuid">The <see cref="MediaElement"/> unique identifier.</param>
        /// <param name="personGuid">The <see cref="Person"/> unique identifier.</param>
        /// <param name="personAliasGuid">The <see cref="PersonAlias"/> unique identifier.</param>
        /// <returns>The application data for the given MediaElement interaction.</returns>
        [Authenticate]
        [HttpGet]
        [System.Web.Http.Route( "api/MediaElements/WatchInteraction" )]
        [RockGuid( "a73244a1-c0db-4efc-a895-1612cdaaf5c2" )]
        public MediaElementInteraction GetWatchInteraction( [FromUri] Guid? mediaElementGuid = null, [FromUri] Guid? personGuid = null, Guid? personAliasGuid = null )
        {
            var rockContext = Service.Context as RockContext;
            var personService = new PersonService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );
            var interactionService = new InteractionService( rockContext );
            int? personAliasId;

            // Get the person alias to associate with the interaction in
            // order of provided Person.Guid, then PersonAlias.Guid, then
            // the logged in Person.
            if ( personGuid.HasValue )
            {
                personAliasId = personAliasService.GetPrimaryAliasId( personGuid.Value );
            }
            else if ( personAliasGuid.HasValue )
            {
                personAliasId = personAliasService.GetId( personAliasGuid.Value );
            }
            else
            {
                personAliasId = GetPersonAliasId( rockContext );
            }

            // Verify we have a person alias, otherwise bail out.
            if ( !personAliasId.HasValue )
            {
                var errorResponse = Request.CreateErrorResponse( HttpStatusCode.BadRequest, $"The personAliasId could not be determined." );
                throw new HttpResponseException( errorResponse );
            }

            MediaElement mediaElement = null;

            // In the future we might make MediaElementGuid optional so
            // perform the check this way rather than making it required
            // in the parameter list.
            if ( mediaElementGuid.HasValue )
            {
                mediaElement = new MediaElementService( rockContext ).GetNoTracking( mediaElementGuid.Value );
            }

            // Ensure we have our required MediaElement.
            if ( mediaElement == null )
            {
                var errorResponse = Request.CreateErrorResponse( HttpStatusCode.BadRequest, $"The MediaElement could not be found." );
                throw new HttpResponseException( errorResponse );
            }

            // Get (or create) the component.
            var interactionChannelId = InteractionChannelCache.Get( SystemGuid.InteractionChannel.MEDIA_EVENTS ).Id;
            var interactionComponentId = InteractionComponentCache.GetComponentIdByChannelIdAndEntityId( interactionChannelId, mediaElement.Id, mediaElement.Name );

            Interaction interaction = interactionService.Queryable()
                .AsNoTracking()
                .Include( a => a.PersonAlias )
                .Where( a => a.InteractionComponentId == interactionComponentId )
                .Where( a => a.PersonAliasId == personAliasId || a.PersonAlias.Person.Aliases.Any( b => b.Id == personAliasId ) )
                .OrderByDescending( a => a.InteractionEndDateTime )
                .ThenByDescending( a => a.InteractionDateTime )
                .FirstOrDefault();

            if ( interaction == null )
            {
                var errorResponse = Request.CreateErrorResponse( HttpStatusCode.NotFound, $"The Interaction could not be found." );
                throw new HttpResponseException( errorResponse );
            }

            var data = interaction.InteractionData.FromJsonOrNull<MediaWatchedInteractionData>();

            return new MediaElementInteraction
            {
                InteractionGuid = interaction.Guid,
                MediaElementGuid = mediaElement.Guid,
                PersonGuid = interaction.PersonAlias.Person?.Guid,
                PersonAliasGuid = interaction.PersonAlias.Guid,
                RelatedEntityTypeId = interaction.RelatedEntityTypeId,
                RelatedEntityId = interaction.RelatedEntityId,
                WatchMap = data?.WatchMap ?? string.Empty
            };
        }

        /// <summary>
        /// Writes a Watch interaction for a <see cref="MediaElement" />.
        /// </summary>
        /// <param name="mediaInteraction">The media interaction data.</param>
        /// <returns>The application data for the MediaElement interaction.</returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "api/MediaElements/WatchInteraction" )]
        [RockGuid( "2368c700-b501-457c-b52b-9786e038d47d" )]
        public MediaElementInteraction PostWatchInteraction( MediaElementInteraction mediaInteraction )
        {
            var rockContext = Service.Context as RockContext;
            var personService = new PersonService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );
            var interactionService = new InteractionService( rockContext );
            int? personAliasId;

            if ( !IsWatchMapValid( mediaInteraction.WatchMap ) )
            {
                var errorResponse = Request.CreateErrorResponse( HttpStatusCode.BadRequest, $"The WatchMap contains invalid characters." );
                throw new HttpResponseException( errorResponse );
            }

            // Get the person alias to associate with the interaction in
            // order of provided Person.Guid, then PersonAlias.Guid, then
            // the logged in Person.
            if ( mediaInteraction.PersonGuid.HasValue )
            {
                personAliasId = personAliasService.GetPrimaryAliasId( mediaInteraction.PersonGuid.Value );
            }
            else if ( mediaInteraction.PersonAliasGuid.HasValue )
            {
                personAliasId = personAliasService.GetId( mediaInteraction.PersonAliasGuid.Value );
            }
            else
            {
                personAliasId = GetPersonAliasId( rockContext );
            }

            var mediaElement = new MediaElementService( rockContext ).GetNoTracking( mediaInteraction.MediaElementGuid );

            // Ensure we have our required MediaElement.
            if ( mediaElement == null )
            {
                var errorResponse = Request.CreateErrorResponse( HttpStatusCode.BadRequest, $"The MediaElement could not be found." );
                throw new HttpResponseException( errorResponse );
            }

            // Get (or create) the component.
            var interactionChannelId = InteractionChannelCache.Get( SystemGuid.InteractionChannel.MEDIA_EVENTS ).Id;
            var interactionComponentId = InteractionComponentCache.GetComponentIdByChannelIdAndEntityId( interactionChannelId, mediaElement.Id, mediaElement.Name );
            Interaction interaction = null;

            if ( mediaInteraction.InteractionGuid.HasValue )
            {
                // Look for an existing Interaction by it's Guid and the
                // component it is supposed to show up under. But also
                // check that the RelatedEntityTypeId/RelatedEntityId values
                // are either null or match what was passed by the user.
                // Finally also make sure the Interaction Person Alias is
                // either the one for this user OR if there is a Person
                // record attached to that alias that the alias Id we have is
                // also attached to that Person record OR the interaction is
                // not tied to a person.
                interaction = interactionService.Queryable()
                    .Where( a => a.Guid == mediaInteraction.InteractionGuid.Value && a.InteractionComponentId == interactionComponentId )
                    .Where( a => !a.RelatedEntityTypeId.HasValue || !mediaInteraction.RelatedEntityTypeId.HasValue || a.RelatedEntityTypeId == mediaInteraction.RelatedEntityTypeId )
                    .Where( a => !a.RelatedEntityId.HasValue || !mediaInteraction.RelatedEntityId.HasValue || a.RelatedEntityId == mediaInteraction.RelatedEntityId )
                    .Where( a => !a.PersonAliasId.HasValue || a.PersonAliasId == personAliasId || a.PersonAlias.Person.Aliases.Any( b => b.Id == personAliasId ) )
                    .SingleOrDefault();
            }

            if ( interaction != null )
            {
                var watchedPercentage = CalculateWatchedPercentage( mediaInteraction.WatchMap );

                // Update the interaction data with the new watch map.
                var data = interaction.InteractionData.FromJsonOrNull<MediaWatchedInteractionData>() ?? new MediaWatchedInteractionData();
                data.WatchMap = mediaInteraction.WatchMap;
                data.WatchedPercentage = watchedPercentage;

                interaction.InteractionData = data.ToJson();
                interaction.InteractionLength = watchedPercentage;
                interaction.InteractionEndDateTime = RockDateTime.Now;
                interaction.PersonAliasId = interaction.PersonAliasId ?? personAliasId;

                if ( mediaInteraction.RelatedEntityTypeId.HasValue )
                {
                    interaction.RelatedEntityTypeId = mediaInteraction.RelatedEntityTypeId;
                }

                if ( mediaInteraction.RelatedEntityId.HasValue )
                {
                    interaction.RelatedEntityId = mediaInteraction.RelatedEntityId;
                }
            }
            else
            {
                // Generate the interaction data from the watch map.
                var data = new MediaWatchedInteractionData
                {
                    WatchMap = mediaInteraction.WatchMap,
                    WatchedPercentage = CalculateWatchedPercentage( mediaInteraction.WatchMap )
                };

                // If the data includes all of the device information then use it.
                if ( mediaInteraction.Application.IsNotNullOrWhiteSpace() && mediaInteraction.OperatingSystem.IsNotNullOrWhiteSpace() && mediaInteraction.ClientType.IsNotNullOrWhiteSpace() )
                {
                    interaction = interactionService.CreateInteraction( interactionComponentId,
                        null,
                        "Watch",
                        mediaInteraction.OriginalUrl?.Truncate( 500, false ),
                        data.ToJson(),
                        personAliasId,
                        RockDateTime.Now,
                        mediaInteraction.Application,
                        mediaInteraction.OperatingSystem,
                        mediaInteraction.ClientType,
                        null,
                        RockRequestContext.ClientInformation.IpAddress,
                        mediaInteraction.SessionGuid );

                    interaction.SetUTMFieldsFromURL( mediaInteraction.OriginalUrl );
                }
                else
                {
                    // Otherwise fallback to UserAgent header parsing.
                    interaction = interactionService.CreateInteraction( interactionComponentId,
                        RockRequestContext.ClientInformation.UserAgent,
                        mediaInteraction.OriginalUrl,
                        RockRequestContext.ClientInformation.IpAddress,
                        mediaInteraction.SessionGuid );

                    interaction.InteractionSummary = mediaInteraction.OriginalUrl?.Truncate( 500, false );
                    interaction.Operation = "Watch";
                    interaction.InteractionData = data.ToJson();
                    interaction.PersonAliasId = personAliasId;
                }

                interaction.InteractionEndDateTime = RockDateTime.Now;
                interaction.RelatedEntityTypeId = mediaInteraction.RelatedEntityTypeId;
                interaction.RelatedEntityId = mediaInteraction.RelatedEntityId;

                interactionService.Add( interaction );
            }

            rockContext.SaveChanges();

            mediaInteraction.InteractionGuid = interaction.Guid;

            return mediaInteraction;
        }

        /// <summary>
        /// Determines whether the watch map is valid.
        /// </summary>
        /// <param name="watchMap">The watch map.</param>
        /// <returns>
        ///   <c>true</c> if the watch map is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsWatchMapValid( string watchMap )
        {
            var segments = watchMap.Split( ',' );

            //
            // Verify all the segment values are valid.
            //
            if ( segments.Any( a => !uint.TryParse( a, out uint value ) || value < 10 ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates the watched percentage of the watch map.
        /// </summary>
        /// <param name="watchMap">The watch map.</param>
        /// <returns>The percentage of the media element that was watched.</returns>
        private double CalculateWatchedPercentage( string watchMap )
        {
            if ( watchMap.IsNullOrWhiteSpace() )
            {
                return 0d;
            }

            var segments = watchMap.Split( ',' );

            //
            // Parse out the segments into the lengths and values.
            //
            var lengthValues = segments.Select( a => new
            {
                Length = a.Substring( 0, a.Length - 1 ).AsInteger(),
                Value = a.Substring( a.Length - 1 ).AsInteger()
            } );

            int length = lengthValues.Sum( a => a.Length );
            int watched = lengthValues.Where( a => a.Value > 0 ).Sum( a => a.Length );

            return Math.Round( watched / ( double ) length * 100, 2 );
        }

        /// <summary>
        /// The data structure that is used with the WatchInteraction endpoint.
        /// </summary>
        public class MediaElementInteraction
        {
            /// <summary>
            /// Gets or sets the media element unique identifier.
            /// </summary>
            /// <value>
            /// The media element unique identifier.
            /// </value>
            public Guid MediaElementGuid { get; set; }

            /// <summary>
            /// Gets or sets the watch map as an run length encoded string.
            /// </summary>
            /// <value>
            /// The watch map as an run length encoded string.
            /// </value>
            public string WatchMap { get; set; }

            /// <summary>
            /// Gets or sets the interaction unique identifier.
            /// </summary>
            /// <value>
            /// The interaction unique identifier.
            /// </value>
            public Guid? InteractionGuid { get; set; }

            /// <summary>
            /// Gets or sets the person alias unique identifier.
            /// </summary>
            /// <value>
            /// The person alias unique identifier.
            /// </value>
            public Guid? PersonAliasGuid { get; set; }

            /// <summary>
            /// Gets or sets the person unique identifier.
            /// </summary>
            /// <value>
            /// The person unique identifier.
            /// </value>
            public Guid? PersonGuid { get; set; }

            /// <summary>
            /// Gets or sets the related entity type identifier.
            /// </summary>
            /// <value>
            /// The related entity type identifier.
            /// </value>
            public int? RelatedEntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the related entity identifier.
            /// </summary>
            /// <value>
            /// The related entity identifier.
            /// </value>
            public int? RelatedEntityId { get; set; }

            /// <summary>
            /// Gets or sets the unique session identifier. This is used to associate
            /// the interaction with the current user session.
            /// </summary>
            /// <remarks>
            /// This is not filled in when retrieving an existing watch interaction.
            /// </remarks>
            /// <value>
            /// The unique session identifier.
            /// </value>
            public Guid? SessionGuid { get; set; }

            /// <summary>
            /// Gets or sets the original page URL (or equivalent for non-web interactions)
            /// used to view this media. This is used to track UTM as well as
            /// which page the individual was on when viewing the media.
            /// </summary>
            /// <remarks>
            /// This is not filled in when retrieving an existing watch interaction.
            /// </remarks>
            /// <value>The original page URL.</value>
            public string OriginalUrl { get; set; }

            /// <summary>
            /// Gets or sets the application name that is submitting this watch
            /// interaction.
            /// </summary>
            /// <remarks>
            /// This is not filled in when retrieving an existing watch interaction.
            /// </remarks>
            /// <value>
            /// The application name that is submitting this watch interaction.
            /// </value>
            public string Application { get; set; }

            /// <summary>
            /// Gets or sets the operating system name and version of the device
            /// submitting the interaction.
            /// </summary>
            /// <remarks>
            /// This is not filled in when retrieving an existing watch interaction.
            /// </remarks>
            /// <value>
            /// The operation system name and version of the device.
            /// </value>
            public string OperatingSystem { get; set; }

            /// <summary>
            /// Gets or sets the type of client submitting this interaction.
            /// </summary>
            /// <remarks>
            /// This is not filled in when retrieving an existing watch interaction.
            /// </remarks>
            /// <value>
            /// The type of client.
            /// </value>
            public string ClientType { get; set; }
        }
    }
}
