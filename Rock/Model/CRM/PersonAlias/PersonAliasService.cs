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

using Microsoft.Extensions.Logging;

using Rock.Communication.Chat;
using Rock.Communication.Chat.DTO;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.PersonAlias"/> entity type objects.
    /// </summary>
    public partial class PersonAliasService
    {
        /// <summary>
        /// Gets the PersonAlias with the id value
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public override PersonAlias Get( int id )
        {
            return Queryable( "Person" ).FirstOrDefault( t => t.Id == id );
        }

        /// <summary>
        /// Gets the PersonAlias with the Guid value
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public override PersonAlias Get( Guid guid )
        {
            return Queryable( "Person" ).FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Gets the primary alias for the specified personId
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public virtual PersonAlias GetPrimaryAlias( int personId )
        {
            return this.GetPrimaryAliasQuery().Include( a => a.Person ).Where( a => a.PersonId == personId ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the primary alias id for the specified personId
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public virtual int? GetPrimaryAliasId( int personId )
        {
            return this.GetPrimaryAliasQuery().Where( a => a.PersonId == personId ).Select( a => ( int? ) a.Id ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the primary alias id for the specified <paramref name="personGuid"/>.
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <returns></returns>
        public virtual int? GetPrimaryAliasId( Guid personGuid )
        {
            return this.GetPrimaryAliasQuery().Where( a => a.Person.Guid == personGuid ).Select( a => ( int? ) a.Id ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the primary alias Guid for the specified personId
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public virtual Guid? GetPrimaryAliasGuid( int personId )
        {
            return this.GetPrimaryAliasQuery().Where( a => a.PersonId == personId ).Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the primary alias Guid for the specified <paramref name="personGuid"/>.
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <returns></returns>
        public virtual Guid? GetPrimaryAliasGuid( Guid personGuid )
        {
            return this.GetPrimaryAliasQuery().Where( a => a.Person.Guid == personGuid ).Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the person identifier for the specified PersonAliasId
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        public virtual int? GetPersonId( int personAliasId )
        {
            return this.GetSelect( personAliasId, s => s.PersonId );
        }

        /// <summary>
        /// Gets the person identifier for the specified PersonAliasGuid
        /// </summary>
        /// <param name="personAliasGuid">The person alias unique identifier.</param>
        /// <remarks><strong>Note:</strong> This method will return 0 if the alias identifier is not found instead of <c>null</c>.</remarks>
        /// <returns></returns>
        public virtual int? GetPersonId( Guid personAliasGuid )
        {
            return this.GetSelect( personAliasGuid, s => s.PersonId );
        }

        /// <summary>
        /// Gets the PersonAlias the by AliasPersonId
        /// </summary>
        /// <param name="aliasPersonId">The alias person identifier.</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasId( int aliasPersonId )
        {
            var personAlias = Queryable( "Person" ).Where( a => a.AliasPersonId == aliasPersonId ).FirstOrDefault();
            if ( personAlias != null )
            {
                return personAlias;
            }
            else
            {
                // If the personId is valid, there should be a personAlias with the AliasPersonID equal 
                // to that personId.  If there isn't for some reason, create it now.
                var person = new PersonService( ( RockContext ) this.Context ).Get( aliasPersonId );
                if ( person != null )
                {
                    personAlias = new PersonAlias();
                    personAlias.Guid = Guid.NewGuid();
                    personAlias.AliasPersonId = person.Id;
                    personAlias.AliasPersonGuid = person.Guid;
                    personAlias.PersonId = person.Id;

                    // Use a different context so calling method's changes are not yet saved
                    var rockContext = new RockContext();
                    new PersonAliasService( rockContext ).Add( personAlias );
                    rockContext.SaveChanges();

                    return Get( personAlias.Id );
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the PersonAlias for the person record that has the specified <see cref="Guid"/> value (The Person.Guid, not the PersonAlias.Guid)
        /// </summary>
        /// <param name="aliasPersonGuid">The GUID value of the Person Record</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasGuid( Guid aliasPersonGuid )
        {
            var personAlias = Queryable( "Person" ).Where( a => a.AliasPersonGuid == aliasPersonGuid ).FirstOrDefault();
            if ( personAlias != null )
            {
                return personAlias;
            }
            else
            {
                // If the personId is valid, there should be a personAlias with the AliasPersonID equal 
                // to that personId.  If there isn't for some reason, create it now.
                var person = new PersonService( ( RockContext ) this.Context ).Get( aliasPersonGuid );
                if ( person != null )
                {
                    personAlias = new PersonAlias();
                    personAlias.Guid = Guid.NewGuid();
                    personAlias.AliasPersonId = person.Id;
                    personAlias.AliasPersonGuid = person.Guid;
                    personAlias.PersonId = person.Id;

                    // Use a different context so calling method's changes are not yet saved
                    var rockContext = new RockContext();
                    new PersonAliasService( rockContext ).Add( personAlias );
                    rockContext.SaveChanges();

                    return Get( personAlias.Id );
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new anonymous visitor alias associated with the core <seealso cref=" Rock.SystemGuid.Person.ANONYMOUS_VISITOR">ANONYMOUS_VISITOR</seealso> person record.
        /// Make sure to call RockContext.SaveChanges to save the new PersonAlias to the database.
        /// </summary>
        /// <returns>PersonAlias.</returns>
        internal PersonAlias CreateAnonymousVisitorAlias()
        {
            var ghostVisitorPersonGuid = Rock.SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid();
            var rockContext = this.Context as RockContext;

            var ghostPersonId = new PersonService( rockContext ).GetId( ghostVisitorPersonGuid );
            if ( ghostPersonId == null )
            {
                // ## TODO can we prevent this from happening? https://app.asana.com/0/0/1202438729153510/f

                // Somehow the Person record for ANONYMOUS_VISITOR is gone!
                // I guess we can't do Visitor tracking. So just exit.
                return null;
            }

            var personAliasService = this;
            var visitorPersonAlias = new PersonAlias();
            visitorPersonAlias.PersonId = ghostPersonId.Value;
            visitorPersonAlias.LastVisitDateTime = RockDateTime.Now;

            // For an Anonymous Visitor alias, leave AliasPersonId and AliasPersonGuid null
            // Since it isn't aliasing a real person, plus all GhostPersonAliases will have
            // the same person id (the Anonymous Person Id ) and AliasPersonId has a uanique constraint.
            visitorPersonAlias.AliasPersonId = null;
            visitorPersonAlias.AliasPersonGuid = null;

            personAliasService.Add( visitorPersonAlias );

            return visitorPersonAlias;
        }

        /// <summary>
        /// Migrates the anonymous visitor alias to real person. Make sure to call RockContext.SaveChanges
        /// to save the updates to the database.
        /// Returns false if the specified anonymousPersonAlias is not a anonymous person alias.
        /// </summary>
        /// <param name="anonymousPersonAlias">The anonymous person alias.</param>
        /// <param name="realPerson">The real person.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool MigrateAnonymousVisitorAliasToRealPerson( PersonAlias anonymousPersonAlias, Person realPerson )
        {
            var ghostVisitorPersonGuid = Rock.SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid();
            var rockContext = this.Context as RockContext;

            var ghostPerson = new PersonService( rockContext ).Get( ghostVisitorPersonGuid );
            if ( anonymousPersonAlias.PersonId != ghostPerson.Id )
            {
                // AnonymousPersonAlias must be an anonymous Person Alias associated with Anonymous Visitor.
                return false;
            }

            anonymousPersonAlias.PersonId = realPerson.Id;
            anonymousPersonAlias.Person = null;

            // We need to completely disassociate person alias from Anonymous (Ghost) visitor person.
            // So make sure AliasPersonId and AliasPersonGuid are null since Anonymous Visitor Alias would have
            // been associated with Anonymous Visitor. Anonymous Visitor Aliases will get aliased to multiple people.
            // Normal Person Aliases only get aliased to a single person record (AliasPersonId has a unique constraint).
            anonymousPersonAlias.AliasPersonId = null;
            anonymousPersonAlias.AliasPersonGuid = null;
            anonymousPersonAlias.AliasPerson = null;

            return true;
        }

        /// <summary>
        /// Gets the by encrypted key.
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasEncryptedKey( string encryptedKey )
        {
            if ( encryptedKey.IsNotNullOrWhiteSpace() )
            {
                string publicKey = Rock.Security.Encryption.DecryptString( encryptedKey );
                return GetByAliasPublicKey( publicKey );
            }

            return null;
        }

        /// <summary>
        /// Gets the by public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasPublicKey( string publicKey )
        {
            try
            {
                if ( publicKey.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                string[] idParts = publicKey.Split( '>' );
                if ( idParts.Length == 2 )
                {
                    int id = Int32.Parse( idParts[0] );
                    Guid guid = new Guid( idParts[1] );

                    PersonAlias personAlias = GetByAliasId( id );
                    if ( personAlias != null && personAlias.AliasPersonGuid.HasValue && personAlias.AliasPersonGuid.Value.CompareTo( guid ) == 0 )
                    {
                        return personAlias;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the person from the specified PersonAlias.Guid
        /// </summary>
        /// <param name="personAliasGuid">The person alias unique identifier.</param>
        /// <returns></returns>
        public Person GetPerson( Guid personAliasGuid )
        {
            return Queryable()
                .Where( a => a.Guid.Equals( personAliasGuid ) )
                .Select( a => a.Person )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the person from the specified PersonAlias.Id
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        public Person GetPerson( int personAliasId )
        {
            return Queryable()
                .Where( a => a.Id.Equals( personAliasId ) )
                .Select( a => a.Person )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the person from the specified PersonAlias.Id, but doesn't include it in change tracking
        /// Use this if you aren't going to make any changes to the person record
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        public Person GetPersonNoTracking( int personAliasId )
        {
            return Queryable()
                .Where( a => a.Id.Equals( personAliasId ) )
                .Select( a => a.Person )
                .AsNoTracking()
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets a Queryable of Primary <see cref="PersonAlias"/>es.
        /// </summary>
        /// <returns>A Queryable of Primary <see cref="PersonAlias"/>es.</returns>
        public IQueryable<PersonAlias> GetPrimaryAliasQuery()
        {
            return this.Queryable().Where( a => a.PersonId == a.AliasPersonId && a.AliasPersonId.HasValue );
        }

        /// <summary>
        /// Gets a Queryable of chat-specific <see cref="PersonAlias"/>es.
        /// </summary>
        /// <returns>A Queryable of chat-specific <see cref="PersonAlias"/>es.</returns>
        internal IQueryable<PersonAlias> GetChatPersonAliasesQuery()
        {
            return this.Queryable().Where( a => a.Name == ChatHelper.ChatPersonAliasName );
        }

        /// <summary>
        /// Gets a Queryable of all <see cref="ChatUser.Key"/>s in Rock, including those for deceased individuals.
        /// </summary>
        /// <returns>A Queryable of all <see cref="ChatUser.Key"/>s in Rock.</returns>
        internal IQueryable<string> GetAllChatUserKeysQuery()
        {
            return GetChatPersonAliasesQuery().Select( pa => pa.ForeignKey );
        }

        /// <summary>
        /// Gets the mapping between all <see cref="ChatUser.Key"/>s and each respective chat-specific
        /// <see cref="PersonAlias"/> identifier.
        /// </summary>
        /// <param name="includeDeceased">Whether to include deceased individuals in the results.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> where the key is the <see cref="ChatUser.Key"/> and the value is
        /// the <see cref="PersonAlias"/> identifier.
        /// </returns>
        /// <remarks>
        /// A given <see cref="Person"/> might be represented more than once in the returned dictionary if they have
        /// more than one chat-specific <see cref="PersonAlias"/>.
        /// </remarks>
        internal Dictionary<string, int> GetChatPersonAliasIdByChatUserKeys( bool includeDeceased = false )
        {
            var chatAliasQry = GetChatPersonAliasesQuery();
            if ( !includeDeceased )
            {
                chatAliasQry = chatAliasQry.Where( pa => !pa.Person.IsDeceased );
            }

            return chatAliasQry
                .ToDictionary(
                    pa => pa.ForeignKey,
                    pa => pa.Id
                );
        }

        /// <summary>
        /// Gets a Queryable of chat-specific <see cref="PersonAlias"/>es that are marked as deleted.
        /// </summary>
        /// <returns>A Queryable of chat-specific <see cref="PersonAlias"/>es that are marked as deleted.</returns>
        internal IQueryable<PersonAlias> GetMarkedAsDeletedChatPersonAliasesQuery()
        {
            return this.Queryable().Where( a => a.Name == ChatHelper.ChatPersonAliasDeletedName );
        }

        /*
            8/24/2026 - CLAUDE

            Lifted out of RockCleanup.RemoveStaleAnonymousVisitorRecord so the
            post-update job that clears the orphaned Anonymous Visitor backlog
            can reuse it. PersonAlias is referenced by a large number of tables,
            so a bulk delete can fail on a single foreign key violation. The
            per-row retry below is what keeps one bad record from costing an
            entire batch.

            Reason: Two callers needed identical, non-obvious delete mechanics.
        */

        /// <summary>
        /// Deletes the supplied <see cref="PersonAlias"/> records in batches,
        /// falling back to individual deletes when a batch fails so that a
        /// single foreign key violation does not discard the whole batch.
        /// </summary>
        /// <param name="personAliasIds">The identifiers of the records to delete.</param>
        /// <param name="contextFactory">Creates a new <see cref="RockContext"/>. A fresh context is needed per batch, and again after any failure, because a context that threw is no longer usable.</param>
        /// <param name="logger">Receives a warning for each record that could not be deleted. May be <c>null</c>.</param>
        /// <param name="beforeBatchDelete">Runs against each batch before the delete is attempted, for callers that must clear references first. May be <c>null</c>.</param>
        /// <returns>The number of records actually deleted.</returns>
        /// <remarks>
        /// A record that cannot be deleted has the failure reason written to its
        /// <see cref="PersonAlias.InternalMessage"/> so later passes can skip it.
        /// </remarks>
        internal static int DeletePersonAliasesInBatches( List<int> personAliasIds, Func<RockContext> contextFactory, ILogger logger, Action<RockContext, List<int>> beforeBatchDelete = null )
        {
            var deleteCount = 0;

            if ( personAliasIds == null || !personAliasIds.Any() )
            {
                return deleteCount;
            }

            // personAliasIds could have over a million values. So instead of
            // using Skip().ToList() to rebuild the list, we are going to use a
            // for loop so we don't have to waste as much memory.
            for ( int bulkStart = 0; bulkStart < personAliasIds.Count; bulkStart += BatchSize )
            {
                // Work in relatively small batches. Since we have to revert to
                // single deletes if the batch fails this gives us a decent
                // balance between speed when everything works and not having to
                // do single deletes on too many records because a single record
                // failed.
                var batchPersonAliasIds = personAliasIds.Skip( bulkStart ).Take( BatchSize ).ToList();

                using ( var bulkRockContext = contextFactory() )
                {
                    beforeBatchDelete?.Invoke( bulkRockContext, batchPersonAliasIds );

                    try
                    {
                        // Try to delete all records in the batch in bulk.
                        // NOTE: This will bypass any save hooks.
                        var personAliasesQry = new PersonAliasService( bulkRockContext ).Queryable()
                            .Where( pa => batchPersonAliasIds.Contains( pa.Id ) );

                        deleteCount += bulkRockContext.BulkDelete( personAliasesQry, BatchSize );
                    }
                    catch
                    {
                        // At least one record failed. Try again one record at
                        // a time so we can log which one(s) failed.
                        deleteCount += DeletePersonAliasesIndividually( batchPersonAliasIds, contextFactory, logger );
                    }
                }
            }

            return deleteCount;
        }

        /// <summary>
        /// Deletes each record on its own so that a failure can be attributed to
        /// a specific record rather than losing the entire batch.
        /// </summary>
        /// <param name="personAliasIds">The identifiers of the records to delete.</param>
        /// <param name="contextFactory">Creates a new <see cref="RockContext"/>.</param>
        /// <param name="logger">Receives a warning for each record that could not be deleted. May be <c>null</c>.</param>
        /// <returns>The number of records actually deleted.</returns>
        private static int DeletePersonAliasesIndividually( List<int> personAliasIds, Func<RockContext> contextFactory, ILogger logger )
        {
            var deleteCount = 0;

            foreach ( var personAliasId in personAliasIds )
            {
                try
                {
                    using ( var singleRockContext = contextFactory() )
                    {
                        var singlePersonAliasService = new PersonAliasService( singleRockContext );
                        var personAlias = singlePersonAliasService.Get( personAliasId );

                        if ( personAlias != null )
                        {
                            singlePersonAliasService.Delete( personAlias );
                            singleRockContext.SaveChanges();

                            deleteCount += 1;
                        }
                    }
                }
                catch ( Exception ex )
                {
                    // Something prevented us from deleting the record. This is
                    // most likely a foreign key violation. Find the inner most
                    // exception, log it, and then update the PersonAlias record
                    // to note we couldn't delete it.
                    var innerEx = ex;

                    while ( innerEx.InnerException != null )
                    {
                        innerEx = innerEx.InnerException;
                    }

                    logger?.LogWarning( $"Error occurred deleting anonymous visitor PersonAlias record ID {personAliasId}: {innerEx.Message}" );

                    // The context we used to attempt the deletion is no good to
                    // use now since it is in a bad state. Create a new context.
                    using ( var errorRockContext = contextFactory() )
                    {
                        var singlePersonAliasService = new PersonAliasService( errorRockContext );
                        var personAlias = singlePersonAliasService.Get( personAliasId );

                        if ( personAlias != null )
                        {
                            personAlias.InternalMessage = innerEx.Message.SubstringSafe( 0, 250 );

                            errorRockContext.SaveChanges();
                        }
                    }
                }
            }

            return deleteCount;
        }

        /// <summary>
        /// The number of records to attempt to delete in a single bulk operation.
        /// </summary>
        private const int BatchSize = 500;
    }
}
