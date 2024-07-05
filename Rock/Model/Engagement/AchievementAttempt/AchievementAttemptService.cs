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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Rock.Data;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.Utility;
using Rock.ViewModels.Engagement;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="AchievementAttempt"/> entity objects.
    /// </summary>
    public partial class AchievementAttemptService
    {
        /// <summary>
        /// Queries attempts by person identifier.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<AchievementAttempt> QueryByPersonId( int personId )
        {
            var rockContext = Context as RockContext;
            var personAliasService = new PersonAliasService( rockContext );
            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;

            var personAliasIdQuery = personAliasService.Queryable()
                .AsNoTracking()
                .Where( pa => pa.PersonId == personId )
                .Select( pa => pa.Id );

            return Queryable().Where( aa =>
                (
                    aa.AchievementType.AchieverEntityTypeId == personEntityTypeId &&
                    aa.AchieverEntityId == personId
                ) ||
                (
                    aa.AchievementType.AchieverEntityTypeId == personAliasEntityTypeId &&
                    personAliasIdQuery.Contains( aa.AchieverEntityId )
                ) );
        }

        /// <summary>
        /// Queries attempts by person alias ids
        /// </summary>
        /// <param name="personAliasIds">The person alias ids.</param>
        /// <returns></returns>
        public IQueryable<AchievementAttempt> QueryByPersonAliasIds( int[] personAliasIds )
        {
            var rockContext = Context as RockContext;
            var personAliasService = new PersonAliasService( rockContext );
            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;

            var personIdQuery = personAliasService.Queryable()
                .AsNoTracking()
                .Where( pa => personAliasIds.Contains( pa.Id ) )
                .Select( pa => pa.PersonId );

            return Queryable().Where( aa =>
                (
                    aa.AchievementType.AchieverEntityTypeId == personEntityTypeId &&
                    personIdQuery.Contains( aa.AchieverEntityId )
                ) ||
                (
                    aa.AchievementType.AchieverEntityTypeId == personAliasEntityTypeId &&
                    personAliasIds.Contains( aa.AchieverEntityId )
                ) );
        }

        /// <summary>
        /// Returns a queryable of <see cref="AchievementAttemptWithPersonAlias"/> where <see cref="AchievementType.AchieverEntityTypeId"/> is a Person or PersonAlias EntityType
        /// </summary>
        /// <returns></returns>
        public IQueryable<AchievementAttemptWithPersonAlias> GetAchievementAttemptWithAchieverPersonAliasQuery()
        {
            int personAliasEntityTypeId = EntityTypeCache.Get<Rock.Model.PersonAlias>().Id;
            int personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            var personEntityQry = this.GetAchievementAttemptWithAchieverPersonAliasQuery( personEntityTypeId );
            var personAliasEntityQry = this.GetAchievementAttemptWithAchieverPersonAliasQuery( personAliasEntityTypeId );

            return personEntityQry.Union( personAliasEntityQry );
        }

        /// <summary>
        /// Includes the AchieverPersonAlias as Rock.Model.Person for AchievementType that is either a PersonAlias or Person (see <see cref="AchievementType.AchieverEntityTypeId" />)
        /// </summary>
        /// <param name="achieverEntityTypeId">The achiever entity type identifier.</param>
        /// <returns></returns>
        private IQueryable<AchievementAttemptWithPersonAlias> GetAchievementAttemptWithAchieverPersonAliasQuery( int achieverEntityTypeId )
        {
            var achievementAttemptQuery = Queryable().Where( a => a.AchievementType.AchieverEntityTypeId == achieverEntityTypeId );
            IQueryable<AchievementAttemptWithPersonAlias> achievementAttemptWithPersonQuery;

            int personAliasEntityTypeId = EntityTypeCache.Get<Rock.Model.PersonAlias>().Id;
            int personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            if ( achieverEntityTypeId == personAliasEntityTypeId )
            {
                var personAliasQry = new PersonAliasService( this.Context as RockContext ).Queryable();
                achievementAttemptWithPersonQuery = achievementAttemptQuery.Join(
                    personAliasQry,
                    a => a.AchieverEntityId,
                    pa => pa.Id,
                    ( a, pa ) => new AchievementAttemptWithPersonAlias
                    {
                        AchievementAttempt = a,
                        AchieverPersonAlias = pa
                    } );
            }
            else if ( achieverEntityTypeId == personEntityTypeId )
            {
                var personQry = new PersonService( this.Context as RockContext ).Queryable();
                achievementAttemptWithPersonQuery = achievementAttemptQuery.Join(
                    personQry,
                    a => a.AchieverEntityId,
                    p => p.Id,
                    ( a, p ) => new AchievementAttemptWithPersonAlias
                    {
                        AchievementAttempt = a,
                        AchieverPersonAlias = p.Aliases.Where( pa => pa.AliasPersonId == pa.PersonId ).FirstOrDefault()
                    } );
            }
            else
            {
                return null;
            }

            return achievementAttemptWithPersonQuery;
        }

        /// <summary>
        /// Gets the ordered person attempts.
        /// </summary>
        /// <param name="attemptsQuery">The attempts query.</param>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <param name="achieverEntityId">The achiever entity identifier.</param>
        /// <returns></returns>
        public List<AchievementAttempt> GetOrderedAchieverAttempts( IQueryable<AchievementAttempt> attemptsQuery, AchievementTypeCache achievementTypeCache, int achieverEntityId )
        {
            attemptsQuery = attemptsQuery.Where( aa => aa.AchievementTypeId == achievementTypeCache.Id );

            // If the achiever type is person alias we need to add all achievements of this type for that person.
            if ( EntityTypeCache.Get<PersonAlias>().Id == achievementTypeCache.AchieverEntityTypeId )
            {
                var personAliasService = new PersonAliasService( ( RockContext ) Context );
                var personAliasQuery = personAliasService
                    .Queryable()
                    .AsNoTracking()
                    .Where( pa => pa.Id == achieverEntityId )
                    .SelectMany( pa => pa.Person.Aliases )
                    .Select( pa => pa.Id );

                attemptsQuery = attemptsQuery
                    .Where( aa => personAliasQuery.Contains( aa.AchieverEntityId ) );
            }
            else
            {
                attemptsQuery = attemptsQuery
                    .Where( aa => aa.AchieverEntityId == achieverEntityId );
            }

            return attemptsQuery
                .OrderByDescending( saa => saa.AchievementAttemptStartDateTime )
                .ToList();
        }

        #region RealTime Related

        /// <summary>
        /// Sends the achievement completed real time notifications for the
        /// specified achievement attempt records.
        /// </summary>
        /// <param name="achievementAttemptGuids">The achievement attempt unique identifiers.</param>
        /// <returns>A task that represents this operation.</returns>
        internal static async Task SendAchievementCompletedRealTimeNotificationsAsync( IEnumerable<Guid> achievementAttemptGuids )
        {
            var guids = achievementAttemptGuids.ToList();

            using ( var rockContext = new RockContext() )
            {
                var achievementAttemptService = new AchievementAttemptService( rockContext );

                while ( guids.Any() )
                {
                    // Work with at most 1,000 records at a time since it
                    // translates to an IN query which doesn't perform well
                    // on large sets.
                    var guidsToProcess = guids.Take( 1_000 ).ToList();
                    guids = guids.Skip( 1_000 ).ToList();

                    try
                    {
                        var qry = achievementAttemptService
                            .Queryable()
                            .AsNoTracking()
                            .Where( aa => achievementAttemptGuids.Contains( aa.Guid ) );

                        await SendAchievementCompletedRealTimeNotificationsAsync( qry, rockContext );
                    }
                    catch ( Exception ex )
                    {
                        Logging.RockLogger.Log.WriteToLog( Logging.RockLogLevel.Error, Logging.RockLogDomains.RealTime, ex.Message );
                    }
                }
            }
        }

        /// <summary>
        /// Send achievement completed real time notifications for the achievement
        /// attempt records returned by the query.
        /// </summary>
        /// <param name="qry">The query that provides the achievement attempt records.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A task that represents this operation.</returns>
        private static async Task SendAchievementCompletedRealTimeNotificationsAsync( IQueryable<AchievementAttempt> qry, RockContext rockContext )
        {
            var bags = GetAchievementCompletedMessageBags( qry, rockContext );

            if ( !bags.Any() )
            {
                return;
            }

            var topicClients = RealTimeHelper.GetTopicContext<IEntityUpdated>().Clients;

            var tasks = bags
                .Select( b =>
                {
                    return Task.Run( () =>
                    {
                        var channels = EntityUpdatedTopic.GetAchievementCompletedChannelsForBag( b );

                        return topicClients
                            .Channels( channels )
                            .AchievementCompleted( b );
                    } );
                } )
                .ToArray();

            try
            {
                await Task.WhenAll( tasks );
            }
            catch ( Exception ex )
            {
                Logging.RockLogger.Log.WriteToLog( Logging.RockLogLevel.Error, Logging.RockLogDomains.RealTime, ex.Message );
            }
        }

        /// <summary>
        /// Gets the achievement completed message bags from the query using the
        /// most optimal pattern.
        /// </summary>
        /// <param name="qry">The query that provides the achievement attempt records.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A list of <see cref="AchievementCompletedMessageBag"/> objects that represent the achievements.</returns>
        private static List<AchievementCompletedMessageBag> GetAchievementCompletedMessageBags( IQueryable<AchievementAttempt> qry, RockContext rockContext )
        {
            var publicApplicationRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" );
            var bags = new List<AchievementCompletedMessageBag>();

            // Query the database and group the results by the achievement type.
            var groupedRecords = qry
                .Select( aa => new
                {
                    aa.Guid,
                    AchievementTypeGuid = aa.AchievementType.Guid,
                    aa.AchieverEntityId
                } )
                .GroupBy( aa => aa.AchievementTypeGuid )
                .ToList();

            foreach ( var records in groupedRecords )
            {
                // Determine the entity type for the achievers of this group.
                var achievementTypeCache = AchievementTypeCache.Get( records.Key );
                var entityType = achievementTypeCache?.AchieverEntityTypeCache?.GetEntityType();
                var entityIds = records.Select( aa => aa.AchieverEntityId ).ToList();

                if ( entityType == null )
                {
                    continue;
                }

                Dictionary<int, IEntity> entityLookup;

                if ( entityType == typeof( PersonAlias ) )
                {
                    // A bit of special logic to deal with PersonAlias so we
                    // actually get back a Person object.
                    entityLookup = new PersonAliasService( rockContext )
                        .Queryable()
                        .Where( pa => entityIds.Contains( pa.Id ) )
                        .Select( pa => new
                        {
                            pa.Id,
                            pa.Person
                        } )
                        .ToList()
                        .ToDictionary( pa => pa.Id, pa => ( IEntity ) pa.Person );
                }
                else
                {
                    // Dynamically get the IService for the entity type and
                    // then get a queryable to load them.
                    var entityService = Rock.Reflection.GetServiceForEntityType( entityType, rockContext );

                    var asQueryableMethod = entityService?.GetType()
                        .GetMethod( "Queryable", Array.Empty<Type>() );

                    // Must not really be an IEntity type...
                    if ( asQueryableMethod == null )
                    {
                        return bags;
                    }

                    var entityQry = ( IQueryable<IEntity> ) asQueryableMethod?.Invoke( entityService, Array.Empty<object>() );

                    // Load all entities referenced by one of the achievement
                    // attempts and the populate the lookup dictionary.
                    entityLookup = entityQry
                        .AsNoTracking()
                        .Where( e => entityIds.Contains( e.Id ) )
                        .ToList()
                        .ToDictionary( e => e.Id, e => e );
                }

                var groupBags = records
                    .Select( r =>
                    {
                        // Since there is no foreign key, it's possible to have
                        // an achievement attempt without an entity.
                        if ( !entityLookup.TryGetValue( r.AchieverEntityId, out var entity ) )
                        {
                            return null;
                        }

                        var bag = new AchievementCompletedMessageBag
                        {
                            AchievementAttemptGuid = r.Guid,
                            AchievementTypeGuid = r.AchievementTypeGuid,
                            AchievementTypeName = achievementTypeCache.Name,
                            EntityGuid = entity.Guid,
                            EntityName = entity.ToString()
                        };

                        if ( achievementTypeCache.ImageBinaryFileId.HasValue )
                        {
                            bag.AchievementTypeImageUrl = FileUrlHelper.GetImageUrl( achievementTypeCache.ImageBinaryFileId.Value, new GetImageUrlOptions { PublicAppRoot = publicApplicationRoot } );
                        }

                        if ( achievementTypeCache.AlternateImageBinaryFileId.HasValue )
                        {
                            bag.AchievementTypeAlternateImageUrl = FileUrlHelper.GetImageUrl( achievementTypeCache.AlternateImageBinaryFileId.Value, new GetImageUrlOptions { PublicAppRoot = publicApplicationRoot } );
                        }

                        // If the entity is a person, populate the photo URL.
                        if ( entity is Person person )
                        {
                            bag.EntityPhotoUrl = $"{publicApplicationRoot}{person.PhotoUrl.TrimStart( '~', '/' )}";
                        }

                        return bag;
                    } )
                    .Where( b => b != null );

                bags.AddRange( groupBags );
            }

            return bags;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public class AchievementAttemptWithPersonAlias
        {
            /// <summary>
            /// Gets or sets the achievement attempt.
            /// </summary>
            /// <value>
            /// The achievement attempt.
            /// </value>
            public AchievementAttempt AchievementAttempt { get; set; }

            /// <summary>
            /// Gets or sets the achiever person.
            /// </summary>
            /// <value>
            /// The achiever person.
            /// </value>
            public PersonAlias AchieverPersonAlias { get; set; }
        }
    }
}