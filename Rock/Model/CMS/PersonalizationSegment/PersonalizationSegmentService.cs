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
using System.Linq;
using System.Diagnostics;

using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Class PersonalizationSegmentService.
    /// </summary>
    public partial class PersonalizationSegmentService
    {
        /// <summary>
        /// Gets a Queryable of <see cref="PersonAlias"/> that have a <see cref="PersonAliasPersonalization.PersonalizationType"/>
        /// of <see cref="PersonalizationType.Segment"/>
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns></returns>
        public IQueryable<PersonAlias> GetPersonAliasSegmentQuery( PersonalizationSegmentCache segment )
        {
            var rockContext = this.Context as RockContext;
            var personAliasService = new PersonAliasService( rockContext );
            var parameterExpression = personAliasService.ParameterExpression;

            var whereExpression = segment.GetPersonAliasFiltersWhereExpression( personAliasService, parameterExpression );

            var personAliasQuery = personAliasService.Get( parameterExpression, whereExpression );

            var dataViewFilterId = segment.FilterDataViewId;
            if ( dataViewFilterId.HasValue )
            {
                var args = new DataViewGetQueryArgs { DbContext = rockContext };
                var dataView = new DataViewService( rockContext ).Get( dataViewFilterId.Value );

                var personDataViewQuery = new PersonService( rockContext ).GetQueryUsingDataView( dataView );
                personAliasQuery = personAliasQuery.Where( pa => personDataViewQuery.Any( person => person.Aliases.Any( alias => alias.Id == pa.Id ) ) );
            }

            return personAliasQuery;
        }

        /// <summary>
        /// Gets a Queryable of <see cref="PersonAliasPersonalization"/> that have a <see cref="PersonAliasPersonalization.PersonalizationType"/>
        /// of <see cref="PersonalizationType.Segment"/>
        /// </summary>
        public IQueryable<Rock.Model.PersonAliasPersonalization> GetPersonAliasPersonalizationSegmentQuery()
        {
            return ( this.Context as RockContext ).Set<PersonAliasPersonalization>().Where( a => a.PersonalizationType == PersonalizationType.Segment );
        }

        /// <summary>
        /// Gets a Queryable of <see cref="PersonAliasPersonalization"/> that have a <see cref="PersonAliasPersonalization.PersonalizationType"/>
        /// of <see cref="PersonalizationType.Segment"/>
        /// </summary>
        /// <param name="personalizationSegment">The personalization segment.</param>
        public IQueryable<Rock.Model.PersonAliasPersonalization> GetPersonAliasPersonalizationSegmentQuery( PersonalizationSegmentCache personalizationSegment )
        {
            return GetPersonAliasPersonalizationSegmentQuery().Where( a => a.PersonalizationEntityId == personalizationSegment.Id );
        }

        /// <summary>
        /// Cleanups the person alias personalization data for segments that no longer exist.
        /// </summary>
        internal int CleanupPersonAliasPersonalizationDataForSegmentsThatDontExist()
        {
            var qry = GetPersonAliasPersonalizationSegmentQuery();
            var segmentIdsThatExist = this.Queryable().Select( a => a.Id );
            var orphanedData = qry.Where( a => a.PersonalizationType == PersonalizationType.Segment && !segmentIdsThatExist.Contains( a.PersonalizationEntityId ) );
            var deletedRows = ( this.Context as RockContext ).BulkDelete( orphanedData );
            return deletedRows;
        }

        /// <inheritdoc cref="UpdatePersonAliasPersonalizationDataForSegment(PersonalizationSegmentCache)"/>
        public void UpdatePersonAliasPersonalizationData( PersonalizationSegmentCache segment )
        {
            this.UpdatePersonAliasPersonalizationDataForSegment( segment );
        }

        /// <summary>
        /// Gets a Queryable of <see cref="PersonalizedEntity"/> that have a <see cref="PersonAliasPersonalization.PersonalizationType"/>
        /// of <see cref="PersonalizationType.Segment"/>
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        public IQueryable<PersonalizedEntity> GetPersonalizedEntitySegmentQuery( int entityTypeId, int entityId )
        {
            return ( this.Context as RockContext ).Set<PersonalizedEntity>()
                .Where( a => a.PersonalizationType == PersonalizationType.Segment && a.EntityTypeId == entityTypeId && a.EntityId == entityId );
        }

        /// <summary>
        /// Updates the data in <see cref="Rock.Model.PersonalizedEntity"/> table based on the specified segments.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="segmentIds">The segment ids.</param>
        public void UpdatePersonalizedEntityForSegments( int entityTypeId, int entityId, List<int> segmentIds )
        {
            var rockContext = this.Context as RockContext;
            var personalizedEntities = GetPersonalizedEntitySegmentQuery( entityTypeId, entityId );
            // Delete personalizedEntities that are no longer in the segment Ids provided.
            var personalizedEntitiesToDelete = personalizedEntities.Where( a => !segmentIds.Contains( a.PersonalizationEntityId ) );
            var countRemovedFromPersonalizedEntities = rockContext.BulkDelete( personalizedEntitiesToDelete );

            // Add personalizationEntityIds that are new.
            var personAliasIdsToAddToSegment = segmentIds
                .Where( segmentId => !personalizedEntities.Any( pe => pe.PersonalizationEntityId == segmentId ) )
                .ToList();
            var personalizedEntitiesToInsert = personAliasIdsToAddToSegment.Distinct().Select( personalizationEntityId => new PersonalizedEntity
            {
                EntityId = entityId,
                EntityTypeId = entityTypeId,
                PersonalizationType = PersonalizationType.Segment,
                PersonalizationEntityId = personalizationEntityId
            } ).ToList();

            /*
             SK - 07-27-2022
             AddRange is used intentionally below instead of BulkInsert as it throws error Unexpected existing transaction.
            */
            rockContext.Set<PersonalizedEntity>().AddRange( personalizedEntitiesToInsert );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Updates the data in <see cref="Rock.Model.PersonAliasPersonalization"/> table based on the specified segment's criteria.
        /// </summary>
        /// <param name="segment">The segment.</param>
        internal SegmentUpdateResults UpdatePersonAliasPersonalizationDataForSegment( PersonalizationSegmentCache segment )
        {
            var stopwatch = Stopwatch.StartNew();

            var rockContext = this.Context as RockContext;
            var personAliasService = new PersonAliasService( rockContext );
            var parameterExpression = personAliasService.ParameterExpression;

            var whereExpression = segment.GetPersonAliasFiltersWhereExpression( personAliasService, parameterExpression );

            var anonymousVisitorPersonId = new PersonService( rockContext ).GetOrCreateAnonymousVisitorPersonId();

            var personAliasQueryForSegment = personAliasService.Get( parameterExpression, whereExpression );

            var dataViewFilterId = segment.FilterDataViewId;
            if ( dataViewFilterId.HasValue )
            {
                var args = new DataViewGetQueryArgs { DbContext = rockContext };
                var dataView = new DataViewService( rockContext ).Get( dataViewFilterId.Value );

                var personDataViewQuery = new PersonService( rockContext ).GetQueryUsingDataView( dataView );
                personAliasQueryForSegment = personAliasQueryForSegment.Where( pa => personDataViewQuery.Any( person => person.Aliases.Any( alias => alias.Id == pa.Id ) ) );
            }

            // From the Segment Query, get the PrimaryAlias for the person associated with the SegmentQuery's PersonAliasId.
            // Also get the PersonId and personAliasId from SegmentQuery (which might not be a primary alias).
            // If the Person is the AnonymousVisitor, use the PersonAliasId from the SegmentQuery, otherwise use the Person's PrimaryAlias.
            // Also, this shouldn't happen, but just in case PrimaryAliasId is null, fall back to SegmentQueryPersonAliasId.
            var personAliasIdsInSegmentQry = personAliasQueryForSegment.Select( s => new
            {
                PrimaryAliasId = ( int? ) s.Person.Aliases.Where( a => a.AliasPersonId == a.PersonId && a.AliasPersonId.HasValue ).Select( a => a.Id ).FirstOrDefault(),
                PersonId = s.PersonId,
                SegmentQueryPersonAliasId = s.Id
            } ).Select( s =>
                s.PersonId == anonymousVisitorPersonId
                    ? s.SegmentQueryPersonAliasId
                    : s.PrimaryAliasId ?? s.SegmentQueryPersonAliasId );

            var personAliasPersonalizationQry = this.GetPersonAliasPersonalizationSegmentQuery( segment );

            // Delete PersonAliasIds that are no longer in the segment.
            var personAliasToDeleteFromSegment = personAliasPersonalizationQry.Where( a => !personAliasIdsInSegmentQry.Contains( a.PersonAliasId ) );
            var countRemovedFromSegment = rockContext.BulkDelete( personAliasToDeleteFromSegment );

            // Add PersonAliasIds that are new in the segment.
            var personAliasIdsToAddToSegment = personAliasIdsInSegmentQry
                .Where( personAliasId => !personAliasPersonalizationQry.Any( pp => pp.PersonAliasId == personAliasId ) )
                .ToList();

            List<PersonAliasPersonalization> personAliasPersonalizationsToInsert = personAliasIdsToAddToSegment.Distinct()
                .Select( personAliasId => new PersonAliasPersonalization
                {
                    PersonAliasId = personAliasId,
                    PersonalizationType = PersonalizationType.Segment,
                    PersonalizationEntityId = segment.Id
                } ).ToList();

            var countAddedToSegment = personAliasPersonalizationsToInsert.Count();
            if ( countAddedToSegment > 0 )
            {
                rockContext.BulkInsert( personAliasPersonalizationsToInsert );
            }

            stopwatch.Stop();
            var durationMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            var segmentEntity = rockContext.Set<PersonalizationSegment>().FirstOrDefault( s => s.Id == segment.Id );

            if ( segmentEntity != null )
            {
                segmentEntity.TimeToUpdateDurationMilliseconds = durationMilliseconds;
                rockContext.SaveChanges();
            }

            return new SegmentUpdateResults( countAddedToSegment, countRemovedFromSegment );
        }

        internal struct SegmentUpdateResults
        {
            public int CountAddedSegment;
            public int CountRemovedFromSegment;

            public SegmentUpdateResults( int countAddedSegment, int countRemovedFromSegment )
            {
                CountAddedSegment = countAddedSegment;
                CountRemovedFromSegment = countRemovedFromSegment;
            }
        }

        /// <summary>
        /// Gets the personalization segment identifier keys for person alias identifier.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns>System.String[].</returns>
        public string[] GetPersonalizationSegmentIdKeysForPersonAliasId( int personAliasId )
        {
            var rockContext = ( RockContext ) this.Context;

            // Get the list of Personalization Entity Ids associated with the specified person alias.
            var qryEntityId = rockContext.Set<PersonAliasPersonalization>()
                .Where( a => a.PersonalizationType == PersonalizationType.Segment && a.PersonAliasId == personAliasId )
                .Select( a => a.PersonalizationEntityId );
            // Get the active Personalization Segments associated with the Entity Ids.
            var qrySegmentId = new PersonalizationSegmentService( rockContext ).Queryable()
                .Where( s => s.IsActive && qryEntityId.Contains( s.Id ) )
                .Select(s => s.Id);
            // Return a set of hashed Ids identifying the Personalization Segments.
            var segmentIdKeys = qrySegmentId.ToList()
                .Select( a => IdHasher.Instance.GetHash( a ) )
                .ToArray();

            return segmentIdKeys;
        }

        #region Actions

        /// <summary>
        /// Add Personalization segments for the specified person.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="segmentIdList"></param>
        /// <returns></returns>
        public int AddSegmentsForPerson( int personId, IEnumerable<int> segmentIdList )
        {
            if ( segmentIdList == null || !segmentIdList.Any() )
            {
                return 0;
            }

            var rockContext = (RockContext)this.Context;
            var existingSegmentIdList = rockContext.Set<PersonAliasPersonalization>()
                .Where( x => x.PersonAlias.PersonId == personId
                             && x.PersonalizationType == PersonalizationType.Segment )
                .Select( x => x.PersonalizationEntityId )
                .ToList();

            var addSegmentIdList = segmentIdList.Except( existingSegmentIdList ).ToList();
            if ( addSegmentIdList.Any() )
            {
                var personService = new PersonService( rockContext );
                var person = personService.Get( personId );
                var personAliasId = person.PrimaryAliasId.GetValueOrDefault(0);
                if ( personAliasId != 0 )
                {
                    foreach ( var segmentId in addSegmentIdList )
                    {
                        var pap = new PersonAliasPersonalization();
                        pap.PersonalizationType = PersonalizationType.Segment;
                        pap.PersonAliasId = personAliasId;
                        pap.PersonalizationEntityId = segmentId;

                        rockContext.Set<PersonAliasPersonalization>().Add( pap );
                    }
                }
            }

            return addSegmentIdList.Count;
        }

        /// <summary>
        /// Remove Personalization segments for the specified person.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="segmentIdList"></param>
        /// <returns></returns>
        public int RemoveSegmentsForPerson( int personId, IEnumerable<int> segmentIdList )
        {
            if ( segmentIdList == null || !segmentIdList.Any() )
            {
                return 0;
            }

            var rockContext = ( RockContext ) this.Context;
            var removeSegments = rockContext.Set<PersonAliasPersonalization>()
                .Where( pap => pap.PersonAlias.PersonId == personId
                             && pap.PersonalizationType == PersonalizationType.Segment
                             && segmentIdList.Contains( pap.PersonalizationEntityId ) )
                .ToList();

            rockContext.Set<PersonAliasPersonalization>().RemoveRange( removeSegments );

            return removeSegments.Count;
        }

        #endregion
    }
}